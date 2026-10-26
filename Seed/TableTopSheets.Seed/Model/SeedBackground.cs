using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedBackground: SeedNamed
{
  public List<string>? Abilities { get; set; }

  public string? OriginFeat { get; set; }

  public List<SeedFeature> Features { get; set; } = [];

  public List<SeedStartingEquipment>? StartingEquipment { get; set; }
}
