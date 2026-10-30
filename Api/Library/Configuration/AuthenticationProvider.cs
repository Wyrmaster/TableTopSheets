namespace TableTopSheets.Api.Configuration;

public enum AuthenticationProvider
{
  /// <summary>
  ///   Users register and sign in with a password stored as a salted hash; the API issues the tokens
  /// </summary>
  Database,

  /// <summary>
  ///   Tokens of an external OpenID Connect identity provider; users are created on their first request
  /// </summary>
  Oidc,
}
