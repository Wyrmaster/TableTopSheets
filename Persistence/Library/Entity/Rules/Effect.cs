using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Declarative modification of a stat. Belongs either to a <see cref="Feature"/> (applied once while the feature is active)
///   or to a <see cref="Choice"/> (applied once per selection, <c>$option</c> and <c>$value</c> are substituted in <see cref="Target"/>)
/// </summary>
public class Effect: BaseEntity
{
  #region Properties

  public Guid? FeatureId { get; set; }

  public Feature? Feature { get; set; }

  public Guid? ChoiceId { get; set; }

  public Choice? Choice { get; set; }

  /// <summary>
  ///   Stat key (e.g. <c>ability.str.score</c>, <c>skill.stealth.prof</c>, <c>ac.base</c>)
  /// </summary>
  public required string Target { get; set; }

  public EffectOperationEnum Operation { get; set; }

  /// <summary>
  ///   Expression for the value (e.g. <c>2</c>, <c>proficient</c>, <c>10 + mod('dex') + mod('wis')</c>)
  /// </summary>
  public string ValueExpr { get; set; } = "1";

  /// <summary>
  ///   Expression that has to be true for the effect to apply (e.g. <c>!wearing('armor') &amp;&amp; !wielding('shield')</c>)
  /// </summary>
  public string? ConditionExpr { get; set; }

  /// <summary>
  ///   Effects with the same group on the same target don't stack, only the highest value applies
  /// </summary>
  public string? StackGroup { get; set; }

  /// <summary>
  ///   Text shown in the source breakdown of a stat
  /// </summary>
  public string? Label { get; set; }

  public int Order { get; set; }

  #endregion
}
