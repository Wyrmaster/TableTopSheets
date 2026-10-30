using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedClass: SeedNamed
{
  public DiceEnum? HitDie { get; set; }

  public List<string>? PrimaryAbilities { get; set; }

  public PrimaryAbilityModeEnum? PrimaryAbilityMode { get; set; }

  public byte? SubclassLevel { get; set; }

  /// <summary>
  ///   Class table columns: key → 20 comma separated values for level 1..20 (<c>3,3,3,4,...</c>)
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];

  public List<SeedFeature> Features { get; set; } = [];

  public List<SeedSubclass> Subclasses { get; set; } = [];

  /// <summary>
  ///   Starting equipment alternatives of a character whose first class this is
  /// </summary>
  public List<SeedStartingEquipment>? StartingEquipment { get; set; }
}
