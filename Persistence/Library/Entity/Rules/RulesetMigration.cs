namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   One run of the ruleset migrator that wrote the rules seed into the database (history, newest last)
/// </summary>
public class RulesetMigration: BaseEntity
{
  #region Properties

  /// <summary>
  ///   SHA-256 (hex) over the seed files that were written; an unchanged hash means the rules are up to date
  /// </summary>
  public required string ContentHash { get; set; }

  /// <summary>
  ///   Version of the migrator that wrote the rules
  /// </summary>
  public required string ToolVersion { get; set; }

  public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

  /// <summary>
  ///   Human readable counts of what was written (classes, spells, inserted and updated rows ...)
  /// </summary>
  public string Summary { get; set; } = string.Empty;

  #endregion
}
