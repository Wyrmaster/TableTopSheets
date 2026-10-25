using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Page;

public class CardPage<T>: Page where T: Card
{
  #region Constants

  private const double Margin = 20d;

  #endregion
  
  #region Field

  private readonly IEnumerable<(string header, string body)> _cards;
  private (string header, string body) _nextCardToDraw;

  #endregion
  
  #region Constructor
  
  public CardPage(PdfDocument pdfDocument, IEnumerable<(string header, string body)> cards) : base(pdfDocument)
  {
    this._cards = cards;
  }
  
  #endregion
  
  #region Override
  
  protected override void WithRender(XGraphics gfx)
  {
    double x = 10, y = 10;
    foreach (var (header, body) in this._cards)
    {
      ItemCard card = new(new(x,y,this.PdfPage.Width / 2  - Margin, 10), CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge, header, body);
      card.Draw(gfx);
      if (y + card.Rect.Height + 10 > this.PdfPage.Height.Point)
      {
        x = this.PdfPage.Width / 2 + 10;
        y = 10d;
        new ItemCard(new(x,y,this.PdfPage.Width / 2  - Margin, 10), CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge, header, body).Draw(gfx);
        y = 10d + card.Rect.Height + 10;
      }
      else
      {
        y += card.Rect.Height + 10;
      }
    }
  }
  
  #endregion

  #region Public Methods
  

  #endregion
}