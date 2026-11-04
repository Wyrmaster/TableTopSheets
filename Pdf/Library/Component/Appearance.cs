using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Appearance box; text shrinks to fit and the rest continues on an extra page
/// </summary>
public class Appearance: InformationBox, IOverflowComponent
{
  #region Fields

  private readonly string _appearance;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 12.54043;
  private const double Width = 178.971;
  private const double Height = 107.4894;

  #endregion

  #region Constructor

  public Appearance(string appearance) : base(new(X,Y,Width,Height), CornerEnum.Edge, CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._appearance = appearance;
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "APPEARANCE";

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;

    XPen line = new(this.FontColor, lineSize);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "APPEARANCE";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    XRect appearanceRect = new(contentRect.X + 4, contentRect.Y + size.Height + 6, contentRect.Width - 8, contentRect.Height - size.Height - 10);

    XRect[] rects = [appearanceRect];
    TextFitResult fit = TextFit.Fit(gfx, this._appearance, TextFit.Family, XFontStyleEx.Regular, rects, TextFit.BodyMaxSize, TextFit.BodyMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects);
    this.Overflow = fit.Overflow;
  }

  #endregion
}
