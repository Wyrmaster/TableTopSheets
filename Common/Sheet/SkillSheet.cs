using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record SkillSheet(string Key, string Name, int Proficiency, int Bonus, bool Advantage, bool Disadvantage);
