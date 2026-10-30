using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (dnddata/Rules) or generated from the rule book (dnddata/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

/// <summary>
///   One alternative of the starting equipment ("(A) Spear, 2 Daggers ..., 28 GP")
/// </summary>
public sealed class SeedStartingEquipment
{
  /// <summary>
  ///   Letter of the alternative (<c>a</c>, <c>b</c> ...)
  /// </summary>
  public required string Key { get; set; }

  public List<SeedStartingItem> Items { get; set; } = [];

  /// <summary>
  ///   Gold pieces of the alternative
  /// </summary>
  public int Gold { get; set; }
}
