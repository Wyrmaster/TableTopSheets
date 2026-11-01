using TableTopSheets.Common.Enum;

namespace TableTopSheets.Common.Sheet;

/// <summary>
///   Block of a rule description (paragraph, heading, list or table), so clients can show its structure without html
/// </summary>
public sealed record TextBlock(TextBlockKindEnum Kind)
{
  /// <summary>
  ///   Text of a paragraph or heading
  /// </summary>
  public IReadOnlyList<TextSpan> Spans { get; init; } = [];

  /// <summary>
  ///   Entries of a list
  /// </summary>
  public IReadOnlyList<IReadOnlyList<TextSpan>> Items { get; init; } = [];

  /// <summary>
  ///   Rows of a table, each a list of cells
  /// </summary>
  public IReadOnlyList<IReadOnlyList<IReadOnlyList<TextSpan>>> Rows { get; init; } = [];

  /// <summary>
  ///   Leading rows of <see cref="Rows"/> that are the header of the table
  /// </summary>
  public int HeaderRows { get; init; }
}
