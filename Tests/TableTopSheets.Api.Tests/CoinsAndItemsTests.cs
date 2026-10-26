using System.Net;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

/// <summary>
///   Coins (set, change, pay), buying items with coins, removing and selling items
/// </summary>
public sealed class CoinsAndItemsTests(ApiFactory factory): IClassFixture<ApiFactory>, IAsyncLifetime
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
  public async Task Coins_CanBeSetChangedAndPaid()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    Assert.Equal(new CoinsDto(), state.Coins);

    state = await this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/coins", new CoinsDto(Gold: 15, Silver: 4));
    Assert.Equal(new CoinsDto(Silver: 4, Gold: 15), state.Coins);

    // found treasure: +2 pp, spent the silver
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/coins/change", new CoinsDto(Silver: -4, Platinum: 2));
    Assert.Equal(new CoinsDto(Gold: 15, Platinum: 2), state.Coins);

    // a room at the inn: 5 sp, paid with a gold piece, change in silver
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/coins/payments", new CoinPaymentDto(50));
    Assert.Equal(new CoinsDto(Silver: 5, Gold: 14, Platinum: 2), state.Coins);

    Guid id = state.Id;
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/coins/change", new CoinsDto(Copper: -1))));
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/coins/payments", new CoinPaymentDto(1_000_000))));
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PutAsync<CharacterStateDto>($"/api/characters/{id}/coins", new CoinsDto(Gold: -1))));
  }

  [Fact]
  public async Task Purchase_PaysTheCost_AndFailsWithoutEnoughCoins()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    state = await this._api.PutAsync<CharacterStateDto>($"/api/characters/{state.Id}/coins", new CoinsDto(Gold: 20));

    // a longsword costs 15 gp
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto("longsword", Purchase: true));
    Assert.Contains(state.Items, entry => entry.ItemKey == "longsword");
    Assert.Equal(new CoinsDto(Gold: 5), state.Coins);

    // plate armor (1500 gp) is too expensive: nothing is added or paid
    Guid id = state.Id;
    HttpStatusCode status = await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/items", new AddItemDto("plate-armor", Purchase: true)));
    Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
    state = await this._api.GetAsync<CharacterStateDto>($"/api/characters/{id}");
    Assert.DoesNotContain(state.Items, entry => entry.ItemKey == "plate-armor");
    Assert.Equal(new CoinsDto(Gold: 5), state.Coins);

    // without purchase the item is free
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/items", new AddItemDto("dagger"));
    Assert.Equal(new CoinsDto(Gold: 5), state.Coins);
  }

  [Fact]
  public async Task RemoveItem_TakesPartOfAStack_OrSellsForHalfTheCost()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto("dagger", Quantity: 3));
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto("longsword"));
    CharacterItemDto daggers = state.Items.Single(entry => entry.ItemKey == "dagger");
    CharacterItemDto longsword = state.Items.Single(entry => entry.ItemKey == "longsword");

    // one dagger lost: two remain
    state = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/items/{daggers.Id}?quantity=1");
    Assert.Equal(2, state.Items.Single(entry => entry.ItemKey == "dagger").Quantity);

    // the longsword (15 gp) sells for 7 gp 5 sp
    state = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/items/{longsword.Id}?sell=true");
    Assert.DoesNotContain(state.Items, entry => entry.ItemKey == "longsword");
    Assert.Equal(new CoinsDto(Silver: 5, Gold: 7), state.Coins);

    // the rest of the stack is thrown away
    state = await this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{state.Id}/items/{daggers.Id}");
    Assert.DoesNotContain(state.Items, entry => entry.ItemKey == "dagger");

    Guid id = state.Id;
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.DeleteAsync<CharacterStateDto>($"/api/characters/{id}/items/{daggers.Id}")));
  }

  [Fact]
  public async Task AddToStack_AddsPiecesWithoutPaying()
  {
    CharacterStateDto state = await this._api.CreateDraftAsync();
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items", new AddItemDto("arrows", Quantity: 20));
    CharacterItemDto arrows = state.Items.Single(entry => entry.ItemKey == "arrows");
    CoinsDto coins = state.Coins;

    // arrows recovered after a fight join the stack
    state = await this._api.PostAsync<CharacterStateDto>($"/api/characters/{state.Id}/items/{arrows.Id}/stack", new AddToStackDto(3));
    Assert.Equal(23, state.Items.Single(entry => entry.ItemKey == "arrows").Quantity);
    Assert.Equal(coins, state.Coins);

    Guid id = state.Id;
    Assert.Equal(HttpStatusCode.UnprocessableEntity, await ApiClient.StatusOf(
      () => this._api.PostAsync<CharacterStateDto>($"/api/characters/{id}/items/{arrows.Id}/stack", new AddToStackDto(0))));
  }

  #endregion
}
