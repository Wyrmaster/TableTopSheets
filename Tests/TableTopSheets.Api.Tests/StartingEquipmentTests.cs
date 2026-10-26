using System.Net;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Starting equipment of backgrounds and classes: in the rules catalog, with creation and the first level, and taken later
/// </summary>
public sealed class StartingEquipmentTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
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
  public async Task RuleDetails_ListTheStartingEquipment()
  {
    RuleDetailDto acolyte = await this._api.GetAsync<RuleDetailDto>("/api/rules/backgrounds/acolyte");
    StartingEquipmentOptionDto a = acolyte.StartingEquipment.Single(entry => entry.Key == "a");
    Assert.Equal(800, a.CoinsCp);
    StartingEquipmentItemDto symbol = a.Items.Single(entry => entry.Key == "holy-symbol");
    Assert.Null(symbol.ItemKey);
    Assert.Equal("Holy Symbol", symbol.Name);
    Assert.Contains(symbol.Options, entry => entry is { Key: "holy-symbol-amulet", Name: "Holy Symbol (Amulet)" });
    Assert.Contains(a.Items, entry => entry is { ItemKey: "parchment", Quantity: 10 });
    Assert.Equal(5000, acolyte.StartingEquipment.Single(entry => entry.Key == "b").CoinsCp);

    ClassDetailDto wizard = await this._api.GetAsync<ClassDetailDto>("/api/rules/classes/wizard");
    Assert.Contains(wizard.StartingEquipment[0].Items, entry => entry is { ItemKey: "spellbook", Name: "Spellbook" });
    Assert.Equal(5500, wizard.StartingEquipment[1].CoinsCp);

    PagedResult<ItemSummaryDto> ammunition = await this._api.GetAsync<PagedResult<ItemSummaryDto>>("/api/rules/items?category=ammunition");
    Assert.Contains(ammunition.Items, entry => entry is { Key: "arrows", Bundle: 20, CostCp: 100 });
  }

  [Fact]
  public async Task Equipment_IsTakenWithCreationAndTheFirstLevel()
  {
    CharacterStateDto state = await this._api.PostAsync<CharacterStateDto>("/api/characters", new CreateCharacterDto
    {
      Name = "Brother Ash",
      SpeciesKey = "dwarf",
      BackgroundKey = "acolyte",
      Abilities = ApiClient.Scores,
      StartingEquipment = new TakeStartingEquipmentDto("a", new Dictionary<string, string> { ["holy-symbol"] = "holy-symbol-reliquary" }),
    });
    Assert.Equal("a", state.BackgroundEquipment);
    Assert.Equal(new CoinsDto(Gold: 8), state.Coins);
    Assert.Contains(state.Items, entry => entry is { ItemKey: "book", Notes: "prayers" });
    Assert.Contains(state.Items, entry => entry.ItemKey == "holy-symbol-reliquary");

    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/levels", new LevelUpDto
    {
      ClassKey = "warlock",
      StartingEquipment = new TakeStartingEquipmentDto("b"),
    });
    Assert.Equal("b", state.ClassEquipment);
    Assert.Equal(new CoinsDto(Gold: 108), state.Coins);

    IReadOnlyList<StartingEquipmentDto> offers = await this._api.GetAsync<IReadOnlyList<StartingEquipmentDto>>($"/api/characters/{state.Id}/starting-equipment");
    Assert.Equal([("acolyte", "a"), ("warlock", "b")], offers.Select(entry => (entry.OwnerKey, entry.Taken)));

    Guid id = state.Id;
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/starting-equipment/Background", new TakeStartingEquipmentDto("b"))));
  }

  [Fact]
  public async Task Equipment_CanBeTakenLater_AndSameAsAboveShowsTheChosenTool()
  {
    CharacterStateDto state = await this._api.PostAsync<CharacterStateDto>("/api/characters", new CreateCharacterDto
    {
      Name = "Tess",
      SpeciesKey = "dwarf",
      BackgroundKey = "artisan",
      Abilities = ApiClient.Scores,
      Selections = [new SelectionDto("artisan.tool", "potters-tools")],
    });
    Assert.Null(state.BackgroundEquipment);

    IReadOnlyList<StartingEquipmentDto> offers = await this._api.GetAsync<IReadOnlyList<StartingEquipmentDto>>($"/api/characters/{state.Id}/starting-equipment");
    StartingEquipmentDto background = Assert.Single(offers);
    Assert.Equal(StartingEquipmentSourceEnum.Background, background.Source);
    StartingEquipmentItemDto tools = background.Options[0].Items.Single(entry => entry.SameAs == "artisan.tool");
    Assert.Equal("potters-tools", tools.ItemKey);

    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/starting-equipment/background", new TakeStartingEquipmentDto("a"));
    Assert.Contains(state.Items, entry => entry.ItemKey == "potters-tools");
    Assert.Contains(state.Items, entry => entry is { ItemKey: "pouch", Quantity: 2 });
    Assert.Equal(new CoinsDto(Gold: 32), state.Coins);

    Guid id = state.Id;
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/starting-equipment/class", new TakeStartingEquipmentDto("a"))));
  }

  #endregion
}
