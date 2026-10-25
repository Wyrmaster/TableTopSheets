namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   How an <c>Effect</c> modifies its target stat
/// </summary>
public enum EffectOperationEnum
{
  /// <summary>
  ///   Adds the value; effects sharing a stack group only contribute their highest value
  /// </summary>
  Add = 0,
  /// <summary>
  ///   Replaces the default value of the stat (highest Set wins)
  /// </summary>
  Set = 1,
  /// <summary>
  ///   Raises the stat to at least the value after all additions (e.g. ability maximum, Belt of Giant Strength)
  /// </summary>
  Max = 2,
  /// <summary>
  ///   Lowers the stat to at most the value after all additions
  /// </summary>
  Min = 3,
  /// <summary>
  ///   Proficiency level (<see cref="TableTopSheets.Common.Enum.Character.SkillModifierEnum"/>), the highest one wins
  /// </summary>
  Proficiency = 4,
  /// <summary>
  ///   Flag (training, resistance, language, advantage ...), value is ignored
  /// </summary>
  Grant = 5,
  /// <summary>
  ///   Alternative formula for the stat (Armor Class calculations); the best one or the selected one is used
  /// </summary>
  Candidate = 6,
}
