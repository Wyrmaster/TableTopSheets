namespace TableTopSheets.Common.Enum.Magic;

[Flags]
public enum ComponentsEnum
{
  Verbal = (1 << 1),
  Somantic = (1 << 2),
  Material = (1 << 3),
}