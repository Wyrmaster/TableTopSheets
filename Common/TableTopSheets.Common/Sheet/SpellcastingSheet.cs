using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record SpellcastingSheet(string Class, string Ability, int Modifier, int SaveDc, int AttackBonus, int PreparedLimit, int CantripLimit);
