using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record FeatureSheet(string Key, string Name, string Source, string Description, bool Spellcasting = false);
