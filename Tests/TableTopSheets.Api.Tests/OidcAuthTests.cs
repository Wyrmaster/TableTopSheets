using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TableTopSheets.Api.Contracts;

namespace TableTopSheets.Api.Tests;

public sealed class OidcAuthTests(OidcApiFactory factory): IClassFixture<OidcApiFactory>
{
  #region Tests

  [Fact]
  public async Task FirstRequest_CreatesTheUser_WhoseCharactersStayLinked()
  {
    ApiClient api = new(factory.CreateClient());
    string subject = Guid.NewGuid().ToString();
    api.UseToken(OidcApiFactory.Token(subject, "gandalf"));

    CurrentUserDto me = await api.GetAsync<CurrentUserDto>("/api/users/me");
    Assert.StartsWith("gandalf", me.UserName);
    Assert.Equal("gandalf@idp.test", me.Email);

    CharacterStateDto hero = await api.CreateDraftAsync("Mithrandir");

    // a new token of the same subject is the same user
    api.UseToken(OidcApiFactory.Token(subject, "gandalf"));
    Assert.Equal(me.Id, (await api.GetAsync<CurrentUserDto>("/api/users/me")).Id);
    PagedResult<CharacterSummaryDto> list = await api.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters");
    Assert.Equal([hero.Id], list.Items.Select(entry => entry.Id));

    // another subject with the same name gets its own user
    api.UseToken(OidcApiFactory.Token(Guid.NewGuid().ToString(), "gandalf"));
    CurrentUserDto other = await api.GetAsync<CurrentUserDto>("/api/users/me");
    Assert.NotEqual(me.Id, other.Id);
    Assert.NotEqual(me.UserName, other.UserName);
    Assert.Empty((await api.GetAsync<PagedResult<CharacterSummaryDto>>("/api/characters")).Items);
  }

  [Fact]
  public async Task TokensForOtherAudiences_AndPasswordEndpoints_AreRejected()
  {
    ApiClient api = new(factory.CreateClient());
    api.UseToken(OidcApiFactory.Token(Guid.NewGuid().ToString(), "saruman", "someone-else"));
    Assert.Equal(HttpStatusCode.Unauthorized, await ApiClient.StatusOf(() => api.GetAsync<CurrentUserDto>("/api/users/me")));

    api.UseToken(null);
    Assert.Equal(HttpStatusCode.NotFound, await ApiClient.StatusOf(() => api.PostAsync<TokenDto>("/api/auth/login", new LoginDto("saruman", ApiClient.Password))));
  }

  #endregion
}
