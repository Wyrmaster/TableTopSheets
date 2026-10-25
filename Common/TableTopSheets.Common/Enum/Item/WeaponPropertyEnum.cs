namespace TableTopSheets.Common.Enum.Item;

[Flags]
public enum WeaponPropertyEnum
{
  None = 0,
  Ammunition = (1 << 1),
  Finesse = (1 << 2),
  Heavy = (1 << 3),
  Light = (1 << 4),
  Loading = (1 << 5),
  Range = (1 << 6),
  Reach = (1 << 7),
  Thrown = (1 << 8),
  TwoHanded = (1 << 9),
  Versatile = (1 << 10),
  Melee = (1 << 11),
  Unarmed = (1 << 12),
}