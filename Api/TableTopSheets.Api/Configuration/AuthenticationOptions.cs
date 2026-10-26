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

/// <summary>
///   <c>Authentication</c> section of the configuration
/// </summary>
public sealed class AuthenticationOptions
{
  #region Constants

  public const string Section = "Authentication";

  #endregion

  #region Properties

  public AuthenticationProvider Provider { get; set; } = AuthenticationProvider.Database;

  public JwtOptions Jwt { get; set; } = new();

  public PasswordOptions Database { get; set; } = new();

  public OidcOptions Oidc { get; set; } = new();

  #endregion
}

/// <summary>
///   Tokens issued by the API (database authentication)
/// </summary>
public sealed class JwtOptions
{
  #region Properties

  public string Issuer { get; set; } = "TableTopSheets";

  public string Audience { get; set; } = "TableTopSheets";

  /// <summary>
  ///   Symmetric key (at least 32 characters); generated per start in Development when empty, required otherwise
  /// </summary>
  public string SigningKey { get; set; } = string.Empty;

  public TimeSpan Lifetime { get; set; } = TimeSpan.FromHours(8);

  #endregion
}

/// <summary>
///   Rules for passwords of the database authentication
/// </summary>
public sealed class PasswordOptions
{
  #region Properties

  public int MinimumPasswordLength { get; set; } = 8;

  /// <summary>
  ///   Allow <c>POST /api/auth/register</c> (otherwise users have to be created in the database)
  /// </summary>
  public bool AllowRegistration { get; set; } = true;

  #endregion
}

/// <summary>
///   External OpenID Connect identity provider
/// </summary>
public sealed class OidcOptions
{
  #region Properties

  /// <summary>
  ///   Issuer url; signing keys are read from its discovery document
  /// </summary>
  public string Authority { get; set; } = string.Empty;

  /// <summary>
  ///   Expected <c>aud</c> of the tokens (not checked when empty)
  /// </summary>
  public string Audience { get; set; } = string.Empty;

  public bool RequireHttpsMetadata { get; set; } = true;

  /// <summary>
  ///   Claim used as user name of new users
  /// </summary>
  public string UserNameClaim { get; set; } = "preferred_username";

  public string EmailClaim { get; set; } = "email";

  #endregion
}
