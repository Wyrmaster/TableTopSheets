namespace TableTopSheets.Seed.Migration;

/// <summary>
///   <c>Migrator</c> section of the configuration
/// </summary>
public sealed class RulesetMigratorOptions
{
  #region Constants

  public const string Section = "Migrator";

  #endregion

  #region Properties

  /// <summary>
  ///   Folder with <c>Extracted</c> and <c>Rules</c>; defaults to the <c>Data</c> folder next to the migrator
  /// </summary>
  public string? DataDirectory { get; set; }

  #endregion
}
