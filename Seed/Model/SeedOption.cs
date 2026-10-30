using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

public sealed class SeedOption
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string? Value { get; set; }

  /// <summary>
  ///   Key of an existing feature granted by the option
  /// </summary>
  public string? Grants { get; set; }

  /// <summary>
  ///   Inline feature granted by the option (created as <c>OwnerType.Option</c>)
  /// </summary>
  public SeedFeature? Feature { get; set; }

  public string? Prerequisite { get; set; }
}
