namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   How class levels contribute to the multiclass spellcaster level
/// </summary>
public enum CasterProgressionEnum
{
  None = 0,
  /// <summary>
  ///   All levels count (Bard, Cleric, Druid, Sorcerer, Wizard)
  /// </summary>
  Full = 1,
  /// <summary>
  ///   Half the levels rounded up (Paladin, Ranger)
  /// </summary>
  Half = 2,
  /// <summary>
  ///   A third of the levels rounded down (Eldritch Knight, Arcane Trickster)
  /// </summary>
  Third = 3,
  /// <summary>
  ///   Separate Pact Magic slots (Warlock)
  /// </summary>
  Pact = 4,
}
