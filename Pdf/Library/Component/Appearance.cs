using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public class Appearance: InformationBox
{
  #region Constants

  private const double X = 405.0396;
  private const double Y = 12.54043;
  private const double Width = 178.971;
  private const double Height = 107.4894;

  #endregion
  public Appearance() : base(new(X,Y,Width,Height), CornerEnum.Edge, CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant)
  {
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

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "APPPEARANCE";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
  }
}