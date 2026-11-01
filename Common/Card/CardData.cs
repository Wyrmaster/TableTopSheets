using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Common.Card;

/// <summary>
///   Content of a printable spell or item card, already formatted for display
/// </summary>
public sealed record CardData
{
  public CardKindEnum Kind { get; init; }

  public required string Name { get; init; }

  /// <summary>
  ///   Short line under the name (<c>Level 3 Evocation</c>, <c>Wondrous Item, Rare</c>)
  /// </summary>
  public string? Subtitle { get; init; }

  /// <summary>
  ///   Rows of the metadata table, in display order
  /// </summary>
  public IReadOnlyList<CardFact> Facts { get; init; } = [];

  public IReadOnlyList<TextBlock> Description { get; init; } = [];

  /// <summary>
  ///   Picture as JPEG or PNG; null when there is none
  /// </summary>
  public byte[]? Image { get; init; }

  /// <summary>
  ///   Small muted line at the bottom (<c>Spell · Wizard, Sorcerer</c>)
  /// </summary>
  public string? Footer { get; init; }
}
