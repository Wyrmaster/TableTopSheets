using System.Net;
using TableTopSheets.Api.Endpoints;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Printable cards of spells and items
/// </summary>
public sealed class CardTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
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
  public async Task Cards_AreAPdfOfSpellsAndItems()
  {
    HttpResponseMessage response = await this._api.PostRawAsync("/api/cards.pdf", new
    {
      spells = new[] { "fireball", "wish" },
      items = new[] { "plus1-longsword", "deck-of-many-things", "chain-mail" },
    });

    response.EnsureSuccessStatusCode();
    Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
    byte[] content = await response.Content.ReadAsByteArrayAsync();
    Assert.Equal("%PDF"u8.ToArray(), content
      .Take(4)
      .ToArray());
    Assert.True(content.Length > 10_000);
    // the JPEG artwork is embedded as it is (DCTDecode is the JPEG filter of PDF)
    Assert.Contains("/DCTDecode", System.Text.Encoding.Latin1.GetString(content));
  }

  [Fact]
  public async Task Cards_RejectUnknownKeysEmptyAndTooLargeRequests()
  {
    HttpResponseMessage unknown = await this._api.PostRawAsync("/api/cards.pdf", new { spells = new[] { "no-such-spell" } });
    HttpResponseMessage empty = await this._api.PostRawAsync("/api/cards.pdf", new { spells = Array.Empty<string>() });
    HttpResponseMessage tooMany = await this._api.PostRawAsync("/api/cards.pdf", new
    {
      items = Enumerable
        .Range(0, CardEndpoints.MaxCards + 1)
        .Select(index => $"item-{index}")
        .ToArray(),
    });

    Assert.Equal(HttpStatusCode.UnprocessableEntity, unknown.StatusCode);
    Assert.Contains("no-such-spell", await unknown.Content.ReadAsStringAsync());
    Assert.Equal(HttpStatusCode.UnprocessableEntity, empty.StatusCode);
    Assert.Equal(HttpStatusCode.UnprocessableEntity, tooMany.StatusCode);
  }

  [Fact]
  public async Task Cards_NeedASignedInUser()
  {
    this._api.UseToken(null);

    HttpResponseMessage response = await this._api.PostRawAsync("/api/cards.pdf", new { spells = new[] { "fireball" } });

    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  #endregion
}
