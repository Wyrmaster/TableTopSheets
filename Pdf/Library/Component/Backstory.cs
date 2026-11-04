using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Backstory & personality box with the alignment on its last line; the backstory shrinks to fit and the rest
///   continues on an extra page
/// </summary>
public class Backstory: InformationBox, IOverflowComponent
{
  #region Fields

  private readonly string _backstory;
  private readonly string _alignment;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 12.54043 + 107.4894 + 11.61741;
  private const double Width = 178.971;
  private const double Height = 214.9787;

  /// <summary>
  ///   Height of the alignment line at the bottom of the box
  /// </summary>
  private const double AlignmentHeight = 24;

  /// <summary>
  ///   Space between the alignment line and the bottom border of the box
  /// </summary>
  private const double AlignmentMargin = 4;

  #endregion

  #region Constructor
  
  public Backstory(string backstory, string alignment) : base(new(X,Y,Width,Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._backstory = backstory;
    this._alignment = alignment;
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "BACKSTORY & PERSONALITY";

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double underlineSize = 0.2;
    
    XPen line = new(this.FontColor, lineSize);
    XPen underline = new(this.FontColor, underlineSize);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

    XFont textTitleFont = new("Sarabun#", 8, XFontStyleEx.Bold);
    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "BACKSTORY & PERSONALITY";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    XRect backstoryRect = new
    (
      contentRect.X + 4,
      contentRect.Y + size.Height + 6,
      contentRect.Width - 8,
      contentRect.Height - size.Height - 10 - AlignmentHeight - AlignmentMargin
    );

    XRect[] rects = [backstoryRect];
    TextFitResult fit = TextFit.Fit(gfx, this._backstory, TextFit.Family, XFontStyleEx.Regular, rects, TextFit.BodyMaxSize, TextFit.BodyMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects);
    this.Overflow = fit.Overflow;

    // "Alignment" label above an underlined value, like the attunement slots of the equipment box
    double top = contentRect.Y + contentRect.Height - AlignmentHeight - AlignmentMargin;
    gfx.DrawString("Alignment", textTitleFont, this.FontBrush, new XPoint(contentRect.X + 4, top + 8));
    XRect valueRect = new(contentRect.X + 4, top + 9, contentRect.Width - 10, 13);
    gfx.DrawLine(underline, new(valueRect.X, valueRect.Bottom), new(valueRect.Right, valueRect.Bottom));
    if (string.IsNullOrWhiteSpace(this._alignment))
    {
      return;
    }

    (XFont font, string text, _) = TextFit.FitSingleLine(gfx, this._alignment, TextFit.Family, XFontStyleEx.Regular, TextFit.BodyMaxSize, TextFit.CellMinSize, valueRect.Width);
    double height = TextFit.LineHeight(font);
    TextFit.DrawLine(gfx, text, font, this.FontBrush, valueRect with { Y = valueRect.Bottom - height, Height = height });
  }

  #endregion
}
