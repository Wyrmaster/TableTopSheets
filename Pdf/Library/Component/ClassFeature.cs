using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class ClassFeature: InformationBox
{
  #region Fields
  
  private readonly string _column1;
  private readonly string _column2;

  #endregion

  #region Constants

  private const double X = 245.04;
  private const double Y = 360.86;
  private const double Width = 336.96;
  private const double Height = 252.03;

  #endregion
  
  #region Constructor
  
  public ClassFeature(string column1, string column2) : base(new XRect(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._column1 = column1;
    this._column2 = column2;
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
    XFont boxTextFont = new("Roboto#", 8, XFontStyleEx.Regular);
    
    const string boxTitle = "CLASS FEATURES";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
    
    gfx.DrawLine
    (
      vLine,
      new(contentRect.X + contentRect.Width / 2 - lineSize * 0.75 / 2, contentRect.Y + size.Height + 8),
      new(contentRect.X + contentRect.Width / 2 - lineSize * 0.75 / 2, contentRect.Y + contentRect.Height - 6)
    );

    XRect col1Rect = new XRect
    (
      contentRect.X + 4,
      contentRect.Y + size.Height + 4,
      contentRect.Width / 2 - 8,
      contentRect.Height - size.Height - 10
    );

    XTextFormatter tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Left;
    tf.DrawString
    (
      this._column1,
      boxTextFont,
      this.FontBrush,
      col1Rect
    );

    XRect col2Rect = new XRect
    (
      contentRect.X + contentRect.Width / 2 + 4,
      contentRect.Y + size.Height + 4,
      contentRect.Width / 2 - 8,
      contentRect.Height - size.Height - 10
    );
    
    tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Left;
    tf.DrawString
    (
      this._column2,
      boxTextFont,
      this.FontBrush,
      col2Rect
    );
    
  }

  #endregion
}