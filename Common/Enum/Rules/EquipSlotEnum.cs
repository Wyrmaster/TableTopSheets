namespace TableTopSheets.Common.Enum.Rules;

public enum EquipSlotEnum
{
  None = 0,
  Armor = 1,
  MainHand = 2,
  OffHand = 3,
  /// <summary>
  ///   Worn items such as rings, cloaks or amulets
  /// </summary>
  Worn = 4,
  /// <summary>
  ///   Weapon that is equipped and ready (on the belt, on the back) but not held; any number of weapons can be ready
  /// </summary>
  Ready = 5,
}
