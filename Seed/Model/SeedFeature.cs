using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedFeature: SeedNamed
{
  public byte? Level { get; set; }

  public GrantModeEnum? GrantMode { get; set; }

  public OwnerTypeEnum? OwnerType { get; set; }

  public string? Category { get; set; }

  public SheetSectionEnum? SheetSection { get; set; }

  public string? Prerequisite { get; set; }

  public bool? Toggleable { get; set; }

  /// <summary>
  ///   Spell the feature belongs to (<c>OwnerType.Spell</c>)
  /// </summary>
  public string? Spell { get; set; }

  public List<SeedEffect> Effects { get; set; } = [];

  public List<SeedChoice> Choices { get; set; } = [];

  public List<SeedResource> Resources { get; set; } = [];

  public SeedSpellcasting? Spellcasting { get; set; }
}
