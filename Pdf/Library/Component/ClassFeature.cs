using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Class features box; one text flows from the first into the second column
/// </summary>
public class ClassFeature: InformationBox, IOverflowComponent
{
  #region Fields
  
  private readonly string _text;

  #endregion

  #region Constants

  private const double X = 245.04;
  private const double Y = 360.86;
  private const double Width = 336.96;
  private const double Height = 252.03;

  #endregion
  
  #region Constructor
  
  /// <summary>
  ///   Class features flowing through both columns
  /// </summary>
  public ClassFeature(string text) : base(new XRect(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._text = text;
  }

  /// <summary>
  ///   Compatibility constructor; both columns are joined into one flowing text
  /// </summary>
  public ClassFeature(string column1, string column2)
    : this(string.Join("\n\n", new[] { column1, column2 }.Where(entry => !string.IsNullOrWhiteSpace(entry))))
  {
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "CLASS FEATURES";

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

    XRect col2Rect = new XRect
    (
      contentRect.X + contentRect.Width / 2 + 4,
      contentRect.Y + size.Height + 4,
      contentRect.Width / 2 - 8,
      contentRect.Height - size.Height - 10
    );
    
    XRect[] columns = [col1Rect, col2Rect];
    TextFitResult fit = TextFit.Fit(gfx, this._text, TextFit.Family, XFontStyleEx.Regular, columns, TextFit.BodyMaxSize, TextFit.BodyMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, columns);
    this.Overflow = fit.Overflow;
  }

  #endregion
}