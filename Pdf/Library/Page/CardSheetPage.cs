using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TableTopSheets.Pdf.Card;
using TableTopSheets.Pdf.Component.Generic;

namespace TableTopSheets.Pdf.Page;

/// <summary>
///   A4 portrait page with cards of any size where <see cref="CardPacker"/> placed them, turned ones drawn turned, and thin
///   crop marks at the corners of every card inside the gap between the cards
/// </summary>
public class CardSheetPage: Page
{
  #region Constants

  /// <summary>
  ///   Distance of a crop mark from the corner of its card
  /// </summary>
  private static readonly double MarkOffset = XUnit.FromMillimeter(0.3).Point;

  /// <summary>
  ///   Length of a crop mark; with the offset it reaches half the gap, so the marks of two neighbours meet but don't cross
  /// </summary>
  private static readonly double MarkLength = XUnit.FromMillimeter(1.2).Point;

  private const double MarkSize = 0.3;

  #endregion

  #region Field

  private readonly IReadOnlyList<CardPlacement> _cards;

  #endregion

  #region Constructor

  /// <summary>
  ///   Page with the <paramref name="cards"/> of one page of <see cref="CardPacker.Pack"/>
  /// </summary>
  public CardSheetPage(PdfDocument pdfDocument, IReadOnlyList<CardPlacement> cards) : base(pdfDocument)
  {
    this._cards = cards;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    XPen pen = new(XColor.FromArgb(150, 150, 152), MarkSize);
    foreach (CardPlacement card in this._cards)
    {
      DrawCropMarks(gfx, pen, card.Bounds);
      if (!card.Rotated)
      {
        new SpellItemCard(card.Bounds.Location, card.Part).Draw(gfx);
        continue;
      }

      // turned clockwise around its top left, the card hangs down to the left of it: start at the top right of the bounds
      XPoint origin = new(card.Bounds.Right, card.Bounds.Top);
      XGraphicsState state = gfx.Save();
      gfx.RotateAtTransform(90, origin);
      new SpellItemCard(origin, card.Part).Draw(gfx);
      gfx.Restore(state);
    }
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   Two short lines at each corner, continuing the edges outwards
  /// </summary>
  private static void DrawCropMarks(XGraphics gfx, XPen pen, XRect bounds)
  {
    foreach (double x in new[] { bounds.Left, bounds.Right })
    {
      foreach (double y in new[] { bounds.Top, bounds.Bottom })
      {
        double outX = x == bounds.Left ? -1 : 1;
        double outY = y == bounds.Top ? -1 : 1;
        gfx.DrawLine(pen, x + outX * MarkOffset, y, x + outX * (MarkOffset + MarkLength), y);
        gfx.DrawLine(pen, x, y + outY * MarkOffset, x, y + outY * (MarkOffset + MarkLength));
      }
    }
  }

  #endregion
}
