namespace TableTopSheets.Common.Enum.Item;

[Flags]
public enum ArmorEnum
{
  Light = (1 << 1),
  Medium = (1 << 2),
  Heavy = (1 << 3),
  Shield = (1 << 4),
}