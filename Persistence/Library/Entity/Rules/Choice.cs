using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Decision a player has to make while a feature is active (skills, spells, invocations, feats ...)
/// </summary>
public class Choice: BaseEntity
{
  #region Properties

  public required string Key { get; set; }

  public required string Name { get; set; }

  public Guid FeatureId { get; set; }

  public Feature? Feature { get; set; }

  /// <summary>
  ///   Total number of selections allowed (e.g. <c>2</c>, <c>scale('warlock.invocations')</c>); evaluated against the current character
  /// </summary>
  public string CountExpr { get; set; } = "1";

  public OptionSourceEnum OptionSource { get; set; }

  /// <summary>
  ///   Feature category for <see cref="OptionSourceEnum.FeatureCategory"/>
  /// </summary>
  public string? Category { get; set; }

  /// <summary>
  ///   Expression evaluated per option (e.g. <c>onList('wizard') &amp;&amp; option.level == 0</c>)
  /// </summary>
  public string? FilterExpr { get; set; }

  public ReplacePolicyEnum ReplacePolicy { get; set; }

  public bool AllowDuplicates { get; set; }

  /// <summary>
  ///   Maximum number of times the same option can be selected when duplicates are allowed
  /// </summary>
  public byte MaxPerOption { get; set; } = 1;

  public bool Optional { get; set; }

  public ICollection<ChoiceOption> Options { get; set; } = new List<ChoiceOption>();

  /// <summary>
  ///   Effects applied once per selection
  /// </summary>
  public ICollection<Effect> Effects { get; set; } = new List<Effect>();

  #endregion
}
