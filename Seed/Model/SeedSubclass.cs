using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedSubclass: SeedNamed
{
  /// <summary>
  ///   Scale columns of the subclass (counted with the level of the class)
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];

  public List<SeedFeature> Features { get; set; } = [];
}
