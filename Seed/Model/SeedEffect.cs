using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedEffect
{
  public required string Target { get; set; }

  public EffectOperationEnum Op { get; set; }

  public string Value { get; set; } = "1";

  public string? Condition { get; set; }

  public string? StackGroup { get; set; }

  public string? Label { get; set; }
}
