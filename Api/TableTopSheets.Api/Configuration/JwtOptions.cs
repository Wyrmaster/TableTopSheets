namespace TableTopSheets.Api.Configuration;

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
