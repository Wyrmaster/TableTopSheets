using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public class Coins : InformationBox
{
  #region Constants

  private const double X = 405.0396;
  private const double Y = 762.14238;
  private const double Width = 178.971;
  private const double Height = 68;

  #endregion

  #region Constructor

  public Coins() : base(new(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Curve, CornerEnum.Edge)
  {
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
    
    const string boxTitle = "COINS";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
  }

  #endregion
}