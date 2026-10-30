using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedResource
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string Max { get; set; } = "1";

  public string? Die { get; set; }

  public PeriodEnum Recharge { get; set; } = PeriodEnum.LongRest;

  // condition from which on the resource recharges on a Short Rest instead (Font of Inspiration)
  public string? ShortRest { get; set; }
}
