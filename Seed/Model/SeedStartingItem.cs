using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

/// <summary>
///   A fixed item (<c>item</c>) or a pick from item groups (<c>choose</c>: <c>holy-symbol</c>, <c>artisan</c> ...)
/// </summary>
public sealed class SeedStartingItem
{
  public string? Item { get; set; }

  /// <summary>
  ///   Item groups (tool groups or gear categories) to pick the item from
  /// </summary>
  public List<string>? Choose { get; set; }

  /// <summary>
  ///   Name of a pick (<c>Holy Symbol</c>)
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  ///   Choice whose selection is the pick ("same as above")
  /// </summary>
  public string? SameAs { get; set; }

  public int Quantity { get; set; } = 1;

  public string? Note { get; set; }
}
