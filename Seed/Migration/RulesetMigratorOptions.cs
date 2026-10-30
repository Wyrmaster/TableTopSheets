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
  ///   Folder with <c>Extracted</c>, <c>Rules</c> and <c>Images</c> (<c>Migrator__DataDirectory</c>; the container mounts
  ///   it at <c>/seed</c>); defaults to the repository's <c>dnddata</c> folder (<see cref="SeedLoader.DefaultDirectory"/>)
  /// </summary>
  public string? DataDirectory { get; set; }

  #endregion
}
