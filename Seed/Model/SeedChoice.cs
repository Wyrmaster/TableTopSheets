using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedChoice
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string Count { get; set; } = "1";

  public OptionSourceEnum Source { get; set; }

  public string? Category { get; set; }

  public string? Filter { get; set; }

  public ReplacePolicyEnum Replace { get; set; }

  public bool AllowDuplicates { get; set; }

  public byte MaxPerOption { get; set; } = 1;

  public bool Optional { get; set; }

  public List<SeedOption> Options { get; set; } = [];

  public List<SeedEffect> Effects { get; set; } = [];
}
