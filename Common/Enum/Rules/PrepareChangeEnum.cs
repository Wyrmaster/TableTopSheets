namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   When the list of prepared spells may be changed
/// </summary>
public enum PrepareChangeEnum
{
  /// <summary>
  ///   Prepared spells are a mutable loadout (Cleric, Druid, Paladin, Ranger, Wizard)
  /// </summary>
  LongRest = 0,
  /// <summary>
  ///   Prepared spells are journaled choices that may be swapped on level up (Bard, Sorcerer, Warlock)
  /// </summary>
  LevelUp = 1,
}
