using System.Net;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

public sealed class AuthTests(ApiFactory factory): IClassFixture<ApiFactory>
{
  #region Tests

  [Fact]
  public async Task Register_ThenLogin_IssuesTokensOfTheSameUser()
  {
    ApiClient api = new(factory.CreateClient());
    string name = $"user-{Guid.NewGuid():N}";
    TokenDto registered = await api.SignInAsync(name);

    api.UseToken(null);
    TokenDto login = await api.PostAsync<TokenDto>("/api/auth/login", new LoginDto(name.ToUpperInvariant(), ApiClient.Password));
    Assert.Equal(registered.User.Id, login.User.Id);
    Assert.Equal("Bearer", login.TokenType);

    api.UseToken(login.AccessToken);
    CurrentUserDto me = await api.GetAsync<CurrentUserDto>("/api/users/me");
    Assert.Equal(name, me.UserName);
    Assert.Equal(0, me.CharacterCount);
  }

  [Fact]
  public async Task Login_WithWrongPassword_IsUnauthorized()
  {
    ApiClient api = new(factory.CreateClient());
    string name = $"user-{Guid.NewGuid():N}";
    await api.SignInAsync(name);

    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.PostAsync<TokenDto>("/api/auth/login", new LoginDto(name, "wrong password"))));
    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.PostAsync<TokenDto>("/api/auth/login", new LoginDto("nobody-" + name, ApiClient.Password))));
  }

  [Fact]
  public async Task Register_RejectsTakenNamesAndShortPasswords()
  {
    ApiClient api = new(factory.CreateClient());
    string name = $"user-{Guid.NewGuid():N}";
    await api.SignInAsync(name);

    Assert.Equal(HttpStatusCode.Conflict, await ApiClient.StatusOf(() => api.PostAsync<TokenDto>("/api/auth/register", new RegisterDto(name.ToUpperInvariant(), ApiClient.Password))));
    Assert.Equal(HttpStatusCode.BadRequest, await ApiClient.StatusOf(() => api.PostAsync<TokenDto>("/api/auth/register", new RegisterDto("other-" + name, "short"))));
  }

  [Fact]
  public async Task Characters_RequireAToken_RulesDoNot()
  {
    ApiClient api = new(factory.CreateClient());

    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters")));
    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.CreateDraftAsync()));
    await api.GetAsync<PagedResult<SpeciesSummaryDto>>("/api/rules/species");

    api.UseToken("not-a-token");
    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters")));
  }

  [Fact]
  public async Task Users_OnlySeeAndChangeTheirOwnCharacters()
  {
    ApiClient alice = new(factory.CreateClient());
    ApiClient bob = new(factory.CreateClient());
    await alice.SignInAsync();
    await bob.SignInAsync();

    CharacterStateDto aliceHero = await alice.AnswerAllAsync(await alice.CreateDraftAsync("Alice's Hero"));
    CharacterStateDto bobHero = await bob.CreateDraftAsync("Bob's Hero");

    PagedResult<CharacterSummaryDto> aliceList = await alice.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters");
    PagedResult<CharacterSummaryDto> bobList = await bob.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters");
    Assert.Equal([aliceHero.Id], aliceList.Items.Select(entry => entry.Id));
    Assert.Equal([bobHero.Id], bobList.Items.Select(entry => entry.Id));

    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(() => bob.GetAsync<CharacterStateDto>($"/api/characters/{aliceHero.Id}")));
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(() => bob.GetAsync<object>($"/api/characters/{aliceHero.Id}/sheet")));
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(() => bob.LevelUpAsync(aliceHero.Id, "wizard")));
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(() => bob.DeleteAsync<object>($"/api/characters/{aliceHero.Id}")));

    CharacterStateDto leveled = await alice.LevelUpAsync(aliceHero.Id, "wizard");
    Assert.Equal(1, leveled.Level);
    Assert.Equal(1, (await alice.GetAsync<CurrentUserDto>("/api/users/me")).CharacterCount);
  }

  #endregion
}
