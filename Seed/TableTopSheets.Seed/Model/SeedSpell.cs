using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedSpell: SeedNamed
{
  public byte? Level { get; set; }

  public SpellSchoolEnum? School { get; set; }

  public string? CastingTime { get; set; }

  public bool? Ritual { get; set; }

  public string? Range { get; set; }

  public string? Components { get; set; }

  public string? Material { get; set; }

  public bool? Concentration { get; set; }

  public string? Duration { get; set; }

  public List<string>? Lists { get; set; }

  public string? Damage { get; set; }

  public DamageTypeEnum? DamageType { get; set; }

  public bool? AttackRoll { get; set; }

  public string? SaveAbility { get; set; }

  public List<string>? CantripDamage { get; set; }
}
