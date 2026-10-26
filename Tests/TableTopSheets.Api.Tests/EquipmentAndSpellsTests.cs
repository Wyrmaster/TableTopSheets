using System.Net;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Ready weapons (any number equipped) and the spells route (spellbook, prepared spells and limits)
/// </summary>
public sealed class EquipmentAndSpellsTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
{
  #region Fields

  private readonly ApiClient _api = new(factory.CreateClient());

  #endregion

  #region IAsyncLifetime

  public Task InitializeAsync() => this._api.SignInAsync();

  public Task DisposeAsync() => Task.CompletedTask;

  #endregion

  #region Tests

  [Fact]
  public async Task AnyNumberOfWeapons_CanBeReady_AndOnlyEquippedWeaponsAreAttacks()
  {
    CharacterStateDto state = await this.CompleteCharacterAsync("fighter");
    foreach (string key in new[] { "quarterstaff", "dagger", "longbow", "shortsword" })
    {
      state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto(key));
    }

    // three weapons ready at once, the shortsword stays in the backpack
    foreach (string key in new[] { "quarterstaff", "dagger", "longbow" })
    {
      state = await this.EquipAsync(state, key, EquipSlotEnum.Ready);
    }

    Assert.All(new[] { "quarterstaff", "dagger", "longbow" }, key => Assert.Equal(EquipSlotEnum.Ready, Item(state, key).Slot));
    CharacterSheet sheet = await this._api.GetAsync<CharacterSheet>($"/api/characters/{state.Id}/sheet");
    Assert.Contains(sheet.Attacks, entry => entry.Name == "Quarterstaff");
    Assert.Contains(sheet.Attacks, entry => entry.Name == "Dagger");
    Assert.Contains(sheet.Attacks, entry => entry.Name == "Longbow");
    Assert.DoesNotContain(sheet.Attacks, entry => entry.Name == "Shortsword");

    // taking another weapon into the occupied main hand puts the previous one back to ready
    state = await this.EquipAsync(state, "shortsword", EquipSlotEnum.MainHand);
    state = await this.EquipAsync(state, "dagger", EquipSlotEnum.MainHand);
    Assert.Equal(EquipSlotEnum.MainHand, Item(state, "dagger").Slot);
    Assert.Equal(EquipSlotEnum.Ready, Item(state, "shortsword").Slot);

    // only weapons can be ready
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto("leather-armor"));
    HttpStatusCode status = await ApiClient.StatusOf(() => this.EquipAsync(state, "leather-armor", EquipSlotEnum.Ready));
    Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
  }

  [Fact]
  public async Task Spells_ListTheSpellbook_AndPreparedSpellsAfterALongRest()
  {
    CharacterStateDto state = await this.CompleteCharacterAsync("wizard");

    CharacterSpellsDto spells = await this._api.GetAsync<CharacterSpellsDto>($"/api/characters/{state.Id}/spells");
    SpellcastingClassDto wizard = Assert.Single(spells.Classes);
    Assert.Equal("wizard", wizard.ClassKey);
    Assert.Equal(SpellPoolEnum.Spellbook, wizard.Pool);
    Assert.Equal(PrepareChangeEnum.LongRest, wizard.PrepareChange);
    Assert.Equal(1, wizard.MaxSpellLevel);
    Assert.True(wizard.PreparedLimit > 0);
    Assert.True(wizard.CantripLimit > 0);
    List<KnownSpellDto> spellbook = wizard.Spells.Where(entry => entry.InSpellbook).ToList();
    Assert.Equal(6, spellbook.Count);
    Assert.All(wizard.Spells.Where(entry => entry.Level == 0), entry => Assert.True(entry.Prepared));

    // spells of the origin feat (Magic Initiate) belong to no class
    Assert.NotEmpty(spells.Other);

    // prepare a loadout from the spellbook
    List<string> loadout = spellbook.Take(Math.Min(wizard.PreparedLimit, spellbook.Count)).Select(entry => entry.Key).ToList();
    await this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/prepared-spells/wizard", new PreparedSpellsDto(loadout));
    wizard = (await this._api.GetAsync<CharacterSpellsDto>($"/api/characters/{state.Id}/spells")).Classes.Single();
    Assert.Equal(loadout.Count, wizard.PreparedCount);
    Assert.All(loadout, key => Assert.True(wizard.Spells.Single(entry => entry.Key == key).Prepared));

    // more than the limit is a rule violation
    List<string> tooMany = spellbook.Select(entry => entry.Key).ToList();
    if (tooMany.Count > wizard.PreparedLimit)
    {
      HttpStatusCode status = await ApiClient.StatusOf(() => this._api.PutAsync<CharacterStateDto>(
        $"/api/characters/{state.Id}/prepared-spells/wizard",
        new PreparedSpellsDto(tooMany)));
      Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
    }

    // copying a spell into the spellbook adds it to the pool
    PagedResult<SpellSummaryDto> candidates = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?class=wizard&level=1&pageSize=200");
    SpellSummaryDto copy = candidates.Items.First(entry => wizard.Spells.All(known => known.Key != entry.Key));
    await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/spellbook", new SpellbookEntryDto(copy.Key));
    wizard = (await this._api.GetAsync<CharacterSpellsDto>($"/api/characters/{state.Id}/spells")).Classes.Single();
    Assert.True(wizard.Spells.Single(entry => entry.Key == copy.Key).InSpellbook);
  }

  #endregion

  #region Helpers

  /// <summary>
  ///   Elf sage at level 1 of the class with every choice answered
  /// </summary>
  private async Task<CharacterStateDto> CompleteCharacterAsync(string classKey)
  {
    CharacterStateDto state = await this._api.AnswerAllAsync(await this._api.CreateDraftAsync());
    return await this._api.AnswerAllAsync(await this._api.LevelUpAsync(state.Id, classKey));
  }

  private Task<CharacterStateDto> EquipAsync(CharacterStateDto state, string itemKey, EquipSlotEnum slot)
    => this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/items/{Item(state, itemKey).Id}/slot", new EquipDto(slot));

  private static CharacterItemDto Item(CharacterStateDto state, string itemKey) => state.Items.Single(entry => entry.ItemKey == itemKey);

  #endregion
}
