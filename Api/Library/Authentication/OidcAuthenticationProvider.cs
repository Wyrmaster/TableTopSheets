using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   Accepts access tokens of an external OpenID Connect identity provider. The first request of a subject creates its
///   <see cref="User"/> row, so characters can be linked to it
/// </summary>
public sealed class OidcAuthenticationProvider(AuthenticationOptions options) : IAuthenticationProvider
{
  #region IAuthenticationProvider

  public void ConfigureJwtBearer(JwtBearerOptions jwt)
  {
    jwt.MapInboundClaims = false;
    if (!string.IsNullOrEmpty(options.Oidc.Authority))
    {
      jwt.Authority = options.Oidc.Authority;
    }

    jwt.RequireHttpsMetadata = options.Oidc.RequireHttpsMetadata;
    if (string.IsNullOrEmpty(options.Oidc.Audience))
    {
      jwt.TokenValidationParameters.ValidateAudience = false;
    }
    else
    {
      jwt.Audience = options.Oidc.Audience;
    }

    jwt.TokenValidationParameters.NameClaimType = options.Oidc.UserNameClaim;
  }

  public void MapEndpoints(RouteGroupBuilder group)
  {
    // sign in happens at the identity provider
  }

  public async Task<User?> ResolveUserAsync(ClaimsPrincipal principal, TableTopSheetsContext context, CancellationToken cancellationToken)
  {
    Claim? subject = principal.FindFirst(JwtClaims.Subject);
    if (subject == null)
    {
      return null;
    }

    string issuer = principal.Value(JwtClaims.Issuer) ?? subject.Issuer;
    User? user = await context.Users.SingleOrDefaultAsync(entry => entry.ExternalIssuer == issuer && entry.ExternalSubject == subject.Value, cancellationToken);
    if (user != null)
    {
      return user;
    }

    user = new User
    {
      Id = Guid.NewGuid(),
      UserName = await UniqueUserNameAsync(context, principal.Value(options.Oidc.UserNameClaim) ?? principal.Value(options.Oidc.EmailClaim) ?? subject.Value, cancellationToken),
      Email = principal.Value(options.Oidc.EmailClaim),
      ExternalIssuer = issuer,
      ExternalSubject = subject.Value,
      CreatedAt = DateTime.UtcNow,
    };
    context.Users.Add(user);
    await context.SaveChangesAsync(cancellationToken);
    return user;
  }

  #endregion

  #region Helpers

  /// <summary>
  ///   The name of the identity provider, with a number appended when another user already has it
  /// </summary>
  private static async Task<string> UniqueUserNameAsync(TableTopSheetsContext context, string wanted, CancellationToken cancellationToken)
  {
    string name = wanted.Length > DatabaseAuthenticationProvider.MaximumUserNameLength - 4
      ? wanted[..(DatabaseAuthenticationProvider.MaximumUserNameLength - 4)]
      : wanted;
    string candidate = name;
    for (int suffix = 2; await context.Users.AnyAsync(entry => entry.UserName.ToLower() == candidate.ToLower(), cancellationToken); suffix++)
    {
      candidate = $"{name}-{suffix}";
    }

    return candidate;
  }

  #endregion
}
