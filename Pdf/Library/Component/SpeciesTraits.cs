using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Species Traits box; text shrinks to fit and the rest continues on an extra page
/// </summary>
public class SpeciesTraits: InformationBox, IOverflowComponent
{
  #region Fields
  
  private readonly string _text;

  #endregion

  #region Constants

  private const double X = 245.04;
  private const double Y = 619.8;
  private const double Width = 165;
  private const double Height = 209.75;

  #endregion
  
  #region Constructor
  
  public SpeciesTraits(string text) : base(new XRect(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge)
  {
    this._text = text;
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "SPECIES TRAITS";

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
    
    const string boxTitle = "SPECIES TRAITS";
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

    XRect[] rects = [textRect];
    TextFitResult fit = TextFit.Fit(gfx, this._text, TextFit.Family, XFontStyleEx.Regular, rects, TextFit.BodyMaxSize, TextFit.BodyMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects);
    this.Overflow = fit.Overflow;
  }

  #endregion
}