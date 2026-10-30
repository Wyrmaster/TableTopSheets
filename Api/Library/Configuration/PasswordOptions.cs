namespace TableTopSheets.Api.Configuration;

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
