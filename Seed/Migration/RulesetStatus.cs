using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Migration;

/// <summary>
///   State of a database compared to the migrator
/// </summary>
/// <param name="PendingSchemaMigrations">EF migrations the database doesn't have yet</param>
/// <param name="LastApplied">latest ruleset written into the database (null: never, or the table doesn't exist yet)</param>
/// <param name="ContentHash">hash of the ruleset this migrator would write</param>
public sealed record RulesetStatus(IReadOnlyList<string> PendingSchemaMigrations, RulesetMigration? LastApplied, string ContentHash)
{
  /// <summary>
  ///   Schema and rules are both current
  /// </summary>
  public bool UpToDate => this.PendingSchemaMigrations.Count == 0 && this.LastApplied?.ContentHash == this.ContentHash;
}
