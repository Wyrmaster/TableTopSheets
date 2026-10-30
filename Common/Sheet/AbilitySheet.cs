using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record AbilitySheet(string Key, string Name, int Score, int Modifier, int SaveProficiency, int SaveBonus, IReadOnlyList<SkillSheet> Skills);
