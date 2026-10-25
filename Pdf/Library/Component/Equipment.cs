using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Equipment list and magic item attunement; the list shrinks to fit and the rest continues on an extra page
/// </summary>
public class Equipment: InformationBox, IOverflowComponent
{
  #region Fields
  
  private readonly string _equipment;
  private readonly string? _attuned1;
  private readonly string? _attuned2;
  private readonly string? _attuned3;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 437.93736;
  private const double Width = 178.971;
  private const double Height = 312.58761;

  #endregion

  #region Constructor
  
  public Equipment(string equipment, string? attuned1 = null, string? attuned2 = null, string? attuned3 = null) : base(new(X,Y,Width,Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._equipment = equipment;
    this._attuned1 = attuned1;
    this._attuned2 = attuned2;
    this._attuned3 = attuned3;
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "EQUIPMENT";

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

    XFont textFont = new("Roboto#", 8, XFontStyleEx.Regular);
    XFont textTitleFont = new("Roboto#", 8, XFontStyleEx.Bold);

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "EQUIPMENT";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    const double equipmentContent = 0.7d;
    XRect dataRect = new XRect
    (
      contentRect.X + 4,
      contentRect.Y + size.Height + 6,
      contentRect.Width - 8,
      contentRect.Height * equipmentContent - size.Height - 10
    );
    
    XRect[] rects = [dataRect];
    TextFitResult fit = TextFit.Fit(gfx, this._equipment, TextFit.Family, XFontStyleEx.Regular, rects, TextFit.BodyMaxSize, TextFit.BodyMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects);
    this.Overflow = fit.Overflow;
    
    dataRect = dataRect with { Y = contentRect.Y + contentRect.Height * equipmentContent, Height = 20};
    
    gfx.DrawString("Magic Item Attunement", textTitleFont, this.FontBrush, new XPoint(dataRect.X, dataRect.Y + 15));
    
    dataRect = dataRect with { Y = contentRect.Y + contentRect.Height * equipmentContent, Height = 20};
    
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 28, 20, 20), deg45: true, fill: this._attuned1 != null).Draw(gfx);
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 48, 20, 20), deg45: true, fill: this._attuned2 != null).Draw(gfx);
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 68, 20, 20), deg45: true, fill: this._attuned3 != null).Draw(gfx);
    
    dataRect = dataRect with { X = dataRect.X + 16, Y = dataRect.Y + 20, Height = 15, Width = dataRect.Width - 18 };
    this.DrawAttuned(gfx, this._attuned1, dataRect);
    
    dataRect = dataRect with { Y = dataRect.Y + 20};
    this.DrawAttuned(gfx, this._attuned2, dataRect);
    
    dataRect = dataRect with { Y = dataRect.Y + 20};
    this.DrawAttuned(gfx, this._attuned3, dataRect);
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   Attuned item name on one line, bottom aligned in its slot; long names shrink, then end with "…"
  /// </summary>
  private void DrawAttuned(XGraphics gfx, string? item, XRect rect)
  {
    if (item == null)
    {
      return;
    }

    (XFont font, string text, _) = TextFit.FitSingleLine(gfx, item, TextFit.Family, XFontStyleEx.Regular, TextFit.BodyMaxSize, TextFit.CellMinSize, rect.Width);
    double height = TextFit.LineHeight(font);
    TextFit.DrawLine(gfx, text, font, this.FontBrush, rect with { Y = rect.Y + rect.Height - height, Height = height });
  }

  #endregion
}