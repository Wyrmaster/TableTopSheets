namespace TableTopSheets.Common.Enum.Rules;

public enum RitualModeEnum
{
  None = 0,
  /// <summary>
  ///   Ritual spells can be cast as rituals when prepared
  /// </summary>
  Prepared = 1,
  /// <summary>
  ///   Ritual spells can be cast from the spellbook without being prepared (Wizard)
  /// </summary>
  FromSpellbook = 2,
}
