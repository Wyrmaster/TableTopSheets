using System.Net.Http.Json;
using System.Net;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Pictures of characters: uploaded and removed by the owner, served anonymously by the character id
/// </summary>
public sealed class PortraitTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
{
  #region Constants

  /// <summary>
  ///   Smallest valid PNG: one transparent pixel
  /// </summary>
  private const string Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNkYAAAAAYAAjCB0C8AAAAASUVORK5CYII=";

  #endregion

  #region Fields

  private readonly ApiClient _api = new(factory.CreateClient());

  #endregion

  #region IAsyncLifetime

  public Task InitializeAsync() => this._api.SignInAsync();

  public Task DisposeAsync() => Task.CompletedTask;

  #endregion

  #region Tests

  [Fact]
  public async Task Portrait_CanBeUploadedServedAndRemoved()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    Assert.Null(state.PortraitUrl);

    state = await this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/portrait", new SetPortraitDto { Data = Png });
    Assert.NotNull(state.PortraitUrl);
    PagedResult<CharacterSummaryDto> list = await this._api.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters");
    Assert.Equal(state.PortraitUrl, list.Items.Single(entry => entry.Id == state.Id).PortraitUrl);

    // anonymous, with an ETag for the browser cache
    ApiClient anonymous = new(factory.CreateClient());
    HttpResponseMessage image = await anonymous.Http.GetAsync(state.PortraitUrl);
    Assert.Equal(HttpStatusCode.OK, image.StatusCode);
    Assert.Equal("image/png", image.Content.Headers.ContentType?.MediaType);
    Assert.Equal(Convert.FromBase64String(Png), await image.Content.ReadAsByteArrayAsync());
    HttpRequestMessage again = new(HttpMethod.Get, state.PortraitUrl);
    again.Headers.IfNoneMatch.Add(image.Headers.ETag!);
    Assert.Equal(HttpStatusCode.NotModified, (await anonymous.Http.SendAsync(again)).StatusCode);

    state = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/portrait");
    Assert.Null(state.PortraitUrl);
    Assert.Equal(HttpStatusCode.NotFound, (await anonymous.Http.GetAsync($"/api/characters/{state.Id}/portrait")).StatusCode);
  }

  [Theory]
  [InlineData("bm90IGFuIGltYWdl")]
  [InlineData("not base64!")]
  public async Task Portrait_ThatIsNoImage_Is422(string data)
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();

    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/portrait", new SetPortraitDto { Data = data })));
  }

  [Fact]
  public async Task Portrait_OfAnotherUser_CantBeChanged()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    ApiClient other = new(factory.CreateClient());
    await other.SignInAsync();

    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(
      () => other.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/portrait", new SetPortraitDto { Data = Png })));
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(
      () => other.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/portrait")));
  }

  [Fact]
  public async Task Details_AreReturnedAfterAnUpdate()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();

    HttpResponseMessage patched = await this._api.Http.PatchAsJsonAsync($"/api/characters/{state.Id}", new UpdateCharacterDto { Appearance = "Tall, silver hair", Backstory = "Raised by owls" }, ApiClient.Json);
    state = (await patched.Content.ReadFromJsonAsync<CharacterStateDto>(ApiClient.Json))!;
    Assert.Equal("Tall, silver hair", state.Appearance);
    Assert.Equal("Raised by owls", (await this._api.GetAsync<CharacterStateDto>($"/api/characters/{state.Id}")).Backstory);
  }

  #endregion
}
