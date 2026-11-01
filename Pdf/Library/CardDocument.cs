using PdfSharp.Drawing;
using TableTopSheets.Common.Card;
using TableTopSheets.Pdf.Card;
using TableTopSheets.Pdf.Page;

namespace TableTopSheets.Pdf;

/// <summary>
///   Printable spell and item cards on A4 pages. Each card gets the smallest size its description fits (see
///   <see cref="CardLayout"/>), then <see cref="CardPacker"/> packs the cards of all sizes together onto as few pages as
///   possible.
/// </summary>
public class CardDocument: Document
{
  #region Field

  private readonly IReadOnlyList<CardData> _cards;

  #endregion

  #region Constructor

  public CardDocument(IReadOnlyList<CardData> cards)
  {
    this._cards = cards;
    this.PdfDocument.Info.Title = "Cards";
  }

  #endregion

  #region Override

  protected override void Generate()
  {
    IReadOnlyList<CardPart> parts;
    using (XGraphics measure = XGraphics.CreateMeasureContext(new XSize(842, 842), XGraphicsUnit.Point, XPageDirection.Downwards))
    {
      parts = CardLayout.Plan(measure, this._cards);
    }

    foreach (IReadOnlyList<CardPlacement> page in CardPacker.Pack(parts))
    {
      new CardSheetPage(this.PdfDocument, page).Draw();
    }

    if (this.PdfDocument.PageCount == 0)
    {
      // a PDF needs a page
      this.PdfDocument.AddPage();
    }
  }

  #endregion
}
