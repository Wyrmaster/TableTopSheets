using PdfSharp.Drawing;
using TableTopSheets.Common.Card;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   Chooses the size of each card: the smallest <see cref="CardFormat"/> whose description fits at
///   <see cref="MinBodySize"/>. The body then grows in steps of <see cref="Step"/> while it still fits, up to the
///   <see cref="CardFormat.MaxBodySize"/> of the format, so the text fills the card. A description that doesn't fit a
///   large card continues on further large cards.
/// </summary>
public static class CardLayout
{
  #region Constants

  /// <summary>
  ///   Smallest body size before a larger card is taken
  /// </summary>
  public const double MinBodySize = 6;

  /// <summary>
  ///   Body size step
  /// </summary>
  public const double Step = 0.25;

  private const double Epsilon = 0.0001;

  #endregion

  #region Public Methods

  /// <summary>
  ///   Cards of every entry, in input order
  /// </summary>
  public static IReadOnlyList<CardPart> Plan(XGraphics gfx, IReadOnlyList<CardData> cards) => cards
    .SelectMany(card => Plan(gfx, card))
    .ToList();

  /// <summary>
  ///   Cards of one entry: one card, or several large cards when the description is too long
  /// </summary>
  public static IReadOnlyList<CardPart> Plan(XGraphics gfx, CardData card)
  {
    // an unreadable picture leaves the header row to the table
    XImage? image = Images.Load(card.Image);
    bool hasImage = image != null;
    foreach (CardFormat format in CardFormat.All)
    {
      if (TryFit(gfx, card, format, MinBodySize, hasImage) is not { } fit)
      {
        continue;
      }

      // fill the card: grow the body while everything still fits
      for (double size = MinBodySize + Step; size <= format.MaxBodySize + Epsilon; size += Step)
      {
        if (TryFit(gfx, card, format, size, hasImage) is not { } larger)
        {
          break;
        }

        fit = larger;
      }

      return [new CardPart(card, fit.Geometry, fit.Columns, image, 1, 1)];
    }

    // too long for one card: the largest body size that needs the fewest large cards
    List<(CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns)>? best = null;
    for (double size = CardFormat.Large.MaxBodySize; size >= MinBodySize - Epsilon; size -= Step)
    {
      List<(CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns)> parts = Split(gfx, card, CardFormat.Large, size, hasImage);
      if (best == null || parts.Count < best.Count)
      {
        best = parts;
      }
    }

    return best!
      .Select((part, index) => new CardPart(card, part.Geometry, part.Columns, image, index + 1, best!.Count))
      .ToList();
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   The whole description on one card of <paramref name="format"/> at <paramref name="size"/>; <c>null</c> when it
  ///   doesn't fit
  /// </summary>
  private static (CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns)? TryFit(XGraphics gfx, CardData card, CardFormat format,
    double size, bool hasImage)
  {
    CardGeometry geometry = CardGeometry.Create(gfx, card, format, size, hasImage, continuation: false);
    RichTextLayout layout = RichTextLayout.Create(gfx, card.Description, geometry.ColumnWidth, size);
    IReadOnlyList<RichTextSlice> columns = layout.Flow(0, Heights(geometry));
    bool complete = columns.Count == 0 ? layout.Lines.Count == 0 : columns[^1].End >= layout.Lines.Count;
    bool inside = columns
      .Zip(geometry.Columns)
      .All(pair => pair.First.Height <= pair.Second.Height + Epsilon);
    return complete && inside ? (geometry, columns) : null;
  }

  private static List<(CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns)> Split(XGraphics gfx, CardData card, CardFormat format,
    double size, bool hasImage)
  {
    CardGeometry first = CardGeometry.Create(gfx, card, format, size, hasImage, continuation: false);
    CardGeometry next = CardGeometry.Create(gfx, card, format, size, hasImage, continuation: true);
    RichTextLayout layout = RichTextLayout.Create(gfx, card.Description, first.ColumnWidth, size);

    List<(CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns)> parts = [];
    int start = 0;
    CardGeometry geometry = first;
    do
    {
      IReadOnlyList<RichTextSlice> columns = layout.Flow(start, Heights(geometry));
      parts.Add((geometry, columns));
      start = columns[^1].End;
      geometry = next;
    }
    while (start < layout.Lines.Count);

    return parts;
  }

  private static List<double> Heights(CardGeometry geometry) => geometry.Columns
    .Select(column => column.Height)
    .ToList();

  #endregion
}
