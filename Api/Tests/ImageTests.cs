using System.Net;
using System.Net.Http.Headers;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Artwork written by the migrator from <c>Data/Images</c>, served anonymously
/// </summary>
public sealed class ImageTests(ApiFactory factory): IClassFixture<ApiFactory>
{
  #region Fields

  private readonly ApiClient _api = new(factory.CreateClient());

  #endregion

  #region Tests

  [Fact]
  public async Task Image_IsServedWithCacheHeaders()
  {
    HttpResponseMessage image = await this._api.Http.GetAsync("/api/images/spell/fireball");
    HttpResponseMessage thumbnail = await this._api.Http.GetAsync("/api/images/spell/fireball?thumb=true");

    Assert.Equal(HttpStatusCode.OK, image.StatusCode);
    Assert.Equal("image/webp", image.Content.Headers.ContentType?.MediaType);
    Assert.NotNull(image.Headers.ETag);
    Assert.True(image.Headers.CacheControl?.Public);
    Assert.True((await thumbnail.Content.ReadAsByteArrayAsync()).Length < (await image.Content.ReadAsByteArrayAsync()).Length);
    Assert.NotEqual(image.Headers.ETag, thumbnail.Headers.ETag);

    HttpRequestMessage again = new(HttpMethod.Get, "/api/images/spell/fireball");
    again.Headers.IfNoneMatch.Add(image.Headers.ETag!);
    HttpResponseMessage cached = await this._api.Http.SendAsync(again);
    Assert.Equal(HttpStatusCode.NotModified, cached.StatusCode);
  }

  [Theory]
  [InlineData("/api/images/item/lantern-hooded")]
  [InlineData("/api/images/coin/gold?thumb=true")]
  public async Task ItemAndCoinImages_AreServed(string url)
    => Assert.Equal(HttpStatusCode.OK, (await this._api.Http.GetAsync(url)).StatusCode);

  [Theory]
  [InlineData("/api/images/spell/unknown")]
  [InlineData("/api/images/monster/fireball")]
  [InlineData("/api/images/0/fireball")]
  public async Task UnknownImage_Is404(string url)
    => Assert.Equal(HttpStatusCode.NotFound, (await this._api.Http.GetAsync(url)).StatusCode);

  [Fact]
  public async Task Scenes_AreListed()
  {
    List<SceneDto> scenes = await this._api.GetAsync<List<SceneDto>>("/api/images/scenes");

    SceneDto tavern = Assert.Single(scenes, entry => entry.Key == "tavern");
    Assert.Equal("Tavern", tavern.Title);
    Assert.True(tavern.Width > tavern.Height);
    Assert.Equal(HttpStatusCode.OK, (await this._api.Http.GetAsync(tavern.ThumbnailUrl)).StatusCode);
  }

  [Fact]
  public async Task Rules_LinkTheirImages()
  {
    SpellDetailDto fireball = await this._api.GetAsync<SpellDetailDto>("/api/rules/spells/fireball");
    Assert.Equal("/api/images/spell/fireball", fireball.Spell.ImageUrl);
    Assert.Equal("/api/images/spell/fireball?thumb=true", fireball.Spell.ThumbnailUrl);

    PagedResult<SpellSummaryDto> spells = await this._api.GetAsync<PagedResult<SpellSummaryDto>>("/api/rules/spells?search=fire%20bolt");
    Assert.NotNull(Assert.Single(spells.Items, entry => entry.Key == "fire-bolt").ThumbnailUrl);

    ClassDetailDto wizard = await this._api.GetAsync<ClassDetailDto>("/api/rules/classes/wizard");
    Assert.Equal("/api/images/class/wizard", wizard.Class.ImageUrl);

    RuleDetailDto orc = await this._api.GetAsync<RuleDetailDto>("/api/rules/species/orc");
    Assert.Equal("/api/images/species/orc", orc.ImageUrl);

    PagedResult<BackgroundSummaryDto> backgrounds = await this._api.GetAsync<PagedResult<BackgroundSummaryDto>>("/api/rules/backgrounds");
    Assert.All(backgrounds.Items, entry => Assert.NotNull(entry.ImageUrl));

    ItemDetailDto scroll = await this._api.GetAsync<ItemDetailDto>("/api/rules/items/spell-scroll-cantrip");
    Assert.Equal("/api/images/item/spell-scroll-cantrip", scroll.Item.ImageUrl);

    // every item has a picture
    for (int page = 1; page <= 2; page++)
    {
      PagedResult<ItemSummaryDto> items = await this._api.GetAsync<PagedResult<ItemSummaryDto>>($"/api/rules/items?pageSize=200&page={page}");
      Assert.All(items.Items, entry => Assert.NotNull(entry.ThumbnailUrl));
    }

    // feats have no pictures
    RuleDetailDto alert = await this._api.GetAsync<RuleDetailDto>("/api/rules/feats/alert");
    Assert.Null(alert.ImageUrl);
  }

  #endregion
}
