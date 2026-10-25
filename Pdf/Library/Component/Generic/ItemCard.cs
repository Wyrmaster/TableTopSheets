using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component.Generic;

public class ItemCard: Card
{
  #region Field
  
  private readonly string _header;
  private readonly string _body;
  private static readonly XFont HeaderFont = new("Roboto#", 12, XFontStyleEx.Bold);
  private static readonly XFont BodyFont = new("Roboto#", 10, XFontStyleEx.Regular);

  /// <summary>
  ///   Space around the header and body inside the card
  /// </summary>
  private const double Padding = 30;

  /// <summary>
  ///   Indent of the body on the left and right
  /// </summary>
  private const double BodyIndent = 8;

  #endregion

  #region Constructor
  
  public ItemCard(XRect rect, CornerEnum tl, CornerEnum tr, CornerEnum br, CornerEnum bl, string header, string body)
    : base(rect, tl, tr, br, bl)
  {
    this._header = header;
    this._body = body;
  }

  #endregion
  
  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    // same widths as the content drawn in WithRender(gfx, contentRect), see InformationBox for the margins
    double contentWidth = this.Rect.Width - 2;
    this.Rect = new(
      this.Rect.X,
      this.Rect.Y,
      this.Rect.Width,
      this.GetSize(gfx, this._header, HeaderFont, contentWidth).Height + this.GetSize(gfx, this._body, BodyFont, contentWidth - BodyIndent * 2).Height + Padding
    );
    base.WithRender(gfx);
  }

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    
    XPen line = new(this.FontColor, lineSize);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    
    XSize headerSize = this.GetSize(gfx, this._header, HeaderFont, contentRect.Width);
    List<string> headerLines = TextFit.Wrap(gfx, this._header, HeaderFont, contentRect.Width);
    for (int index = 0; index < headerLines.Count; index++)
    {
      double y = contentRect.Y + index * TextFit.LineHeight(HeaderFont);
      TextFit.DrawLine(gfx, headerLines[index], HeaderFont, titleBrush, contentRect with { Y = y }, XParagraphAlignment.Center);
    }
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + headerSize.Height + 5),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + headerSize.Height + 5)
    );

    double bodyWidth = contentRect.Width - BodyIndent * 2;
    List<string> bodyLines = TextFit.Wrap(gfx, this._body, BodyFont, bodyWidth);
    for (int index = 0; index < bodyLines.Count; index++)
    {
      double y = contentRect.Y + headerSize.Height + 10 + index * TextFit.LineHeight(BodyFont);
      TextFit.DrawLine(gfx, bodyLines[index], BodyFont, titleBrush, new XRect(contentRect.X + BodyIndent, y, bodyWidth, TextFit.LineHeight(BodyFont)));
    }
  }

  #endregion
}
