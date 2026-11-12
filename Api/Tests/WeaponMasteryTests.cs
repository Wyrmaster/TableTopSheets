using System.Net;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Weapon mastery properties: their rule text, the mastery of weapons in item lists and on item cards
/// </summary>
public sealed class WeaponMasteryTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
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
  public async Task Mastery_HasItsRuleText()
  {
    WeaponMasteryDto nick = await this._api.GetAsync<WeaponMasteryDto>("/api/rules/masteries/nick");

    Assert.Equal("Nick", nick.Name);
    Assert.Contains("Light property", nick.Description);
    Assert.NotEmpty(nick.DescriptionBlocks);
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(
      () => this._api.GetAsync<WeaponMasteryDto>("/api/rules/masteries/unknown")));
  }

  [Fact]
  public async Task ItemList_CarriesTheMasteryOfWeaponsAndTheBaseOfMagicVariants()
  {
    PagedResult<ItemSummaryDto> swords = await this._api.GetAsync<PagedResult<ItemSummaryDto>>("/api/rules/items?search=longsword&pageSize=200");
    ItemSummaryDto longsword = swords.Items.Single(entry => entry.Key == "longsword");
    ItemSummaryDto magic = swords.Items.Single(entry => entry.Key == "plus1-longsword");

    Assert.Equal(("sap", "Sap", null), (longsword.MasteryKey, longsword.MasteryName, longsword.BaseKey));
    Assert.Equal(("sap", "Sap", "longsword"), (magic.MasteryKey, magic.MasteryName, magic.BaseKey));

    ItemDetailDto rope = await this._api.GetAsync<ItemDetailDto>("/api/rules/items/rope");
    Assert.Null(rope.Item.MasteryKey);
    ItemDetailDto detail = await this._api.GetAsync<ItemDetailDto>("/api/rules/items/plus1-longsword");
    Assert.Equal(("sap", "longsword"), (detail.Item.MasteryKey, detail.Item.BaseKey));
  }

  [Fact]
  public void Card_OfAWeaponHasTheRuleTextOfItsMastery()
  {
    WeaponMastery sap = new()
    {
      Key = "sap",
      Name = "Sap",
      DescriptionHtml = "<p>If you hit a creature with this weapon, that creature has Disadvantage on its next attack roll.</p>",
    };
    Weapon longsword = new()
    {
      Key = "longsword",
      Name = "Longsword",
      Kind = ItemKindEnum.Weapon,
      Category = WeaponCategoryEnum.Martial,
      Damage = "1d8",
      DamageType = DamageTypeEnum.Slashing,
      Mastery = sap,
    };

    CardData card = CardMapper.Card(longsword, null);

    Assert.Contains(card.Facts, entry => entry == new CardFact("Mastery", "Sap"));
    Assert.Contains(card.Description, block => block.Spans.Count > 1
                                               && block.Spans[0].Text == "Mastery: Sap. "
                                               && block.Spans[0].Bold
                                               && block.Spans[1].Text.StartsWith("If you hit", StringComparison.Ordinal));
  }

  #endregion
}
