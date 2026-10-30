namespace TableTopSheets.Api.Configuration;

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
