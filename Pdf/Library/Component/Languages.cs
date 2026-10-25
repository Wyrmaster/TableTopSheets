using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class Languages: InformationBox
{
  #region Fields

  private readonly string _languages;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 12.54043 + 107.4894 + 11.61741 * 2 + 214.9787;
  private const double Width = 178.971;
  private const double Height = 68.0766;

  #endregion

  #region Constructor
  
  public Languages(string languages) : base(new(X,Y,Width,Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._languages = languages;
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
    
    const string boxTitle = "LANGUAGES";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
    
    XRect languageRect = new(contentRect.X + 4, contentRect.Y + size.Height + 4, contentRect.Width - 8, contentRect.Height - size.Height - 10);

    XTextFormatter tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Left;
    tf.DrawString
    (
      this._languages,
      new("Roboto#", 8, XFontStyleEx.Regular),
      this.FontBrush,
      languageRect
    );
  }

  #endregion
}