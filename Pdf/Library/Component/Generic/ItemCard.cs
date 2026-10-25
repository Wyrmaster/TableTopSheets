using TableTopSheets.Pdf.Enum;
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
    this.Rect = new(
      this.Rect.X,
      this.Rect.Y,
      this.Rect.Width,
      this.GetSize(gfx, this._header,HeaderFont, this.Rect.Width).Height + this.GetSize(gfx, this._body, BodyFont, this.Rect.Width).Height + 50
    );
    base.WithRender(gfx);
  }

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double columnSize = 0.3;
    const double underlineSize = 0.2;
    
    XPen line = new(this.FontColor, lineSize);
    XColor columnColor = XColor.FromArgb(156, 157, 161);
    XPen vLine = new(columnColor, lineSize * 0.75);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    
    XSize headerSize = this.GetSize(gfx, this._header, HeaderFont, contentRect.Width);

    XTextFormatter tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Center;
    tf.DrawString(this._header, HeaderFont, titleBrush, new(
      contentRect.X,
      contentRect.Y,
      headerSize.Width,
      headerSize.Height
    ));
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + headerSize.Height + 15),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + headerSize.Height + 15)
    );

    tf.Alignment = XParagraphAlignment.Justify;
    tf.DrawString(this._body, BodyFont, titleBrush, new(
      contentRect.X + 8, 
      contentRect.Y + headerSize.Height + 20,
      contentRect.Width - 20,
      contentRect.Height - 6 - headerSize.Height
    ));

  }

  #endregion
}