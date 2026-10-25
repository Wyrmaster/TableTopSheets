namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Base for all named rule entities that are seeded from a rule book
/// </summary>
public abstract class RuleEntity: BaseEntity
{
  #region Properties

  /// <summary>
  ///   Stable unique key (e.g. <c>wizard</c>, <c>fireball</c>) used by seed data and expressions
  /// </summary>
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string DescriptionHtml { get; set; } = string.Empty;

  public Guid? SourceId { get; set; }

  public Source? Source { get; set; }

  #endregion
}
