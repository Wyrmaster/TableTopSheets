using System.Security.Claims;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   Claim names as they appear in the token (inbound claim mapping is switched off)
/// </summary>
public static class JwtClaims
{
  #region Constants

  public const string Subject = "sub";
  public const string Issuer = "iss";
  public const string Name = "name";
  public const string TokenId = "jti";

  #endregion

  #region Methods

  public static string? Value(this ClaimsPrincipal principal, string type) => principal.FindFirst(type)?.Value;

  #endregion
}
