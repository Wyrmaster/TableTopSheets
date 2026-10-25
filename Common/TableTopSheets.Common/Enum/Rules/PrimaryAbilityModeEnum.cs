namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   How the primary abilities of a class are checked for the multiclass prerequisite
/// </summary>
public enum PrimaryAbilityModeEnum
{
  /// <summary>
  ///   Every primary ability must be 13+ (Monk: Dexterity and Wisdom)
  /// </summary>
  All = 0,
  /// <summary>
  ///   One of the primary abilities must be 13+ (Fighter: Strength or Dexterity)
  /// </summary>
  Any = 1,
}
