using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   How users sign in; the implementation is chosen by <c>Authentication:Provider</c>. Every implementation authenticates
///   requests with bearer tokens and maps them to a <see cref="User"/> row the characters belong to
/// </summary>
public interface IAuthenticationProvider
{
  #region Methods

  /// <summary>
  ///   Validation of the bearer tokens (own signing key, or the keys of an identity provider)
  /// </summary>
  void ConfigureJwtBearer(JwtBearerOptions options);

  /// <summary>
  ///   Provider specific endpoints below <c>/api/auth</c> (e.g. register and login)
  /// </summary>
  void MapEndpoints(RouteGroupBuilder group);

  /// <summary>
  ///   User of an authenticated request (null when the token doesn't belong to a known user)
  /// </summary>
  Task<User?> ResolveUserAsync(ClaimsPrincipal principal, TableTopSheetsContext context, CancellationToken cancellationToken);

  #endregion
}
