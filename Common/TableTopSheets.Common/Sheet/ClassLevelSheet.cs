using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record ClassLevelSheet(string Class, string? Subclass, int Level, DiceEnum HitDie);
