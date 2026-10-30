namespace TableTopSheets.Common.Enum.Magic;

[Flags]
public enum CastingTimeEnum
{
  Duration = (1 << 0),
  Action = (1 << 1),
  Ritual = (1 << 2),
  Reaction = (1 << 3),
  BonusAction = (1 << 4),
}