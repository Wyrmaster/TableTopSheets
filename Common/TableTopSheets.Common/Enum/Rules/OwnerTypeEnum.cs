namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   Kind of rule entity a <c>Feature</c> belongs to
/// </summary>
public enum OwnerTypeEnum
{
  Class = 0,
  Subclass = 1,
  Species = 2,
  Background = 3,
  Feat = 4,
  Item = 5,
  /// <summary>
  ///   Selectable option (Invocation, Metamagic, Fighting Style, Lineage ...) only active when chosen
  /// </summary>
  Option = 6,
  /// <summary>
  ///   Effects of a spell that can be toggled on (Mage Armor, Shield ...)
  /// </summary>
  Spell = 7,
}
