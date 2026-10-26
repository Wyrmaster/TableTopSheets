namespace TableTopSheets.Api.Configuration;

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
