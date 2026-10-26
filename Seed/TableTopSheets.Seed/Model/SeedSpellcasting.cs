using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedSpellcasting
{
  public required string Ability { get; set; }

  public CasterProgressionEnum Progression { get; set; }

  public string? SlotTable { get; set; }

  public required string SpellList { get; set; }

  public string? Prepared { get; set; }

  public string? Cantrips { get; set; }

  public SpellPoolEnum Pool { get; set; }

  public PrepareChangeEnum PrepareChange { get; set; }

  public RitualModeEnum Ritual { get; set; }

  public string? Focus { get; set; }

  /// <summary>
  ///   Class the profile counts for (defaults to the owning class)
  /// </summary>
  public string? Class { get; set; }
}
