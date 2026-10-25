using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class Feats: InformationBox
{
  #region Fields
  
  private readonly string _text;

  #endregion

  #region Constants

  private const double X = 416.85;
  private const double Y = 619.8;
  private const double Width = 165;
  private const double Height = 209.75;

  #endregion
  
  #region Constructor
  
  public Feats(string text) : base(new XRect(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Curve, CornerEnum.Edge)
  {
    this._text = text;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double columnSize = 0.3;
    const double underlineSize = 0.2;
    
    XPen line = new(this.FontColor, lineSize);
    XColor columnColor = XColor.FromArgb(156, 157, 161);
    XPen vLine = new(columnColor, lineSize * 0.75);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "FEATS";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
    
    XRect textRect = new XRect
    (
      contentRect.X + 4,
      contentRect.Y + size.Height + 4,
      contentRect.Width - 8,
      contentRect.Height - size.Height - 10
    );

    XTextFormatter tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Left;
    tf.DrawString
    (
      this._text,
      new("Roboto#", 8, XFontStyleEx.Regular),
      this.FontBrush,
      textRect
    );
  }

  #endregion
}