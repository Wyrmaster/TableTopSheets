using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record ResourceSheet(string Key, string Name, int Maximum, int Used, string? Die, string Recharge);
