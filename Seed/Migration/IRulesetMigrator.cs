using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Migration;

/// <summary>
///   Brings a database up to date: schema (EF migrations) first, then the rules seed. Rules are upserted, nothing that
///   is in the database already disappears (see <see cref="SeedWriter"/>)
/// </summary>
public interface IRulesetMigrator
{
  /// <summary>
  ///   Apply pending EF migrations and write the rules unless the same ruleset was written last
  /// </summary>
  /// <param name="force">write the rules even when they are unchanged</param>
  /// <returns>the history entry of this run, null when the rules were up to date</returns>
  Task<RulesetMigration?> MigrateAsync(bool force = false, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Build and validate the rules without touching a database
  /// </summary>
  /// <returns>every problem found; empty when the rules are valid</returns>
  IReadOnlyList<string> Validate();

  /// <summary>
  ///   Pending EF migrations and the last ruleset written, compared with this migrator's ruleset
  /// </summary>
  Task<RulesetStatus> StatusAsync(CancellationToken cancellationToken = default);
}
