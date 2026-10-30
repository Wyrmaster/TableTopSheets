namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Option of a <see cref="Choice"/> with <c>OptionSource.Static</c>
/// </summary>
public class ChoiceOption: BaseEntity
{
  #region Properties

  public Guid ChoiceId { get; set; }

  public Choice? Choice { get; set; }

  public required string Key { get; set; }

  public required string Name { get; set; }

  /// <summary>
  ///   Value substituted for <c>$value</c> in the effects of the choice (e.g. draconic ancestry <c>red</c> → <c>fire</c>)
  /// </summary>
  public string? Value { get; set; }

  /// <summary>
  ///   Feature that becomes active when this option is selected
  /// </summary>
  public Guid? GrantsFeatureId { get; set; }

  public Feature? GrantsFeature { get; set; }

  public string? PrerequisiteExpr { get; set; }

  #endregion
}
