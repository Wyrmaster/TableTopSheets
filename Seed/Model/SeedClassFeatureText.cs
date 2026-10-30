using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedClassFeatureText
{
  public required string Class { get; set; }

  public byte Level { get; set; }

  public required string Name { get; set; }

  public required string Description { get; set; }
}
