namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   Where the selectable options of a <c>Choice</c> come from
/// </summary>
public enum OptionSourceEnum
{
  /// <summary>
  ///   Options are listed in <c>ChoiceOption</c>
  /// </summary>
  Static = 0,
  Ability = 1,
  Skill = 2,
  Tool = 3,
  Language = 4,
  Spell = 5,
  Feat = 6,
  /// <summary>
  ///   Features of <c>OwnerType.Option</c> with the category stored in <c>Choice.Category</c>
  /// </summary>
  FeatureCategory = 7,
  Item = 8,
  Weapon = 9,
}
