using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedFeat: SeedNamed
{
  public FeatCategoryEnum? Category { get; set; }

  public bool? Repeatable { get; set; }

  public string? Prerequisite { get; set; }

  public List<SeedFeature> Features { get; set; } = [];
}
