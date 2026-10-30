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

/// <summary>
///   API configured for an OIDC identity provider; the provider is replaced by a local signing key
/// </summary>
public sealed class OidcApiFactory: ApiFactory
{
  #region Constants

  public const string Issuer = "https://idp.test/realms/tabletop";
  public const string Audience = "tabletopsheets";

  public static readonly SymmetricSecurityKey Key = new(Encoding.UTF8.GetBytes("oidc-tests-signing-key-with-at-least-32-characters"));

  #endregion

  #region Overrides

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);
    builder.UseSetting("Authentication:Provider", "Oidc");
    builder.UseSetting("Authentication:Oidc:Audience", Audience);
    builder.ConfigureTestServices(services => services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
    {
      options.TokenValidationParameters.ValidIssuer = Issuer;
      options.TokenValidationParameters.IssuerSigningKey = Key;
    }));
  }

  #endregion

  #region Methods

  /// <summary>
  ///   Access token as the identity provider would issue it
  /// </summary>
  public static string Token(string subject, string userName, string? audience = Audience)
    => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
      Issuer = Issuer,
      Audience = audience,
      Expires = DateTime.UtcNow.AddMinutes(5),
      SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256),
      Claims = new Dictionary<string, object>
      {
        ["sub"] = subject,
        ["preferred_username"] = userName,
        ["email"] = $"{userName}@idp.test",
      },
    });

  #endregion
}
