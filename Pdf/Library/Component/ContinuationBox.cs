using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Full width section on an extra page that continues the text of a box that didn't fit, e.g. <c>CLASS FEATURES (CONTINUED)</c>
/// </summary>
public class ContinuationBox: InformationBox, IOverflowComponent
{
  #region Constants

  /// <summary>
  ///   Fixed text size of continuation sections; the text doesn't shrink
  /// </summary>
  public const double TextSize = TextFit.BodyMaxSize;

  /// <summary>
  ///   Suffix appended to the title of the continued box
  /// </summary>
  public const string TitleSuffix = " (CONTINUED)";

  private const double TitleSize = 9;

  /// <summary>
  ///   Margin and padding of <see cref="InformationBox"/> plus the space around the title and below the text
  /// </summary>
  private const double Chrome = 1 + 2 + 4 + 6;

  #endregion

  #region Field

  private readonly string _title;
  private readonly string _text;

  #endregion

  #region Constructor

  /// <param name="title">Title of the continued box without <see cref="TitleSuffix"/>, e.g. <c>CLASS FEATURES</c></param>
  /// <param name="text">Text to continue</param>
  /// <param name="rect">Area of the section</param>
  public ContinuationBox(string title, string text, XRect rect) : base(rect, CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._title = title;
    this._text = text;
  }

  #endregion

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => this._title;

  #endregion

  #region Public Methods

  /// <summary>
  ///   Height a section of <paramref name="width"/> needs to show the whole <paramref name="text"/>
  /// </summary>
  public static double NeededHeight(XGraphics gfx, string text, double width)
  {
    XFont font = TextFont();
    int lines = Math.Max(1, TextFit.Wrap(gfx, text, font, TextWidth(width)).Count);
    return TitleHeight(gfx) + Chrome + lines * TextFit.LineHeight(font) + 0.5;
  }

  /// <summary>
  ///   Smallest useful section height: the title and one line of text
  /// </summary>
  public static double MinimumHeight(XGraphics gfx) => TitleHeight(gfx) + Chrome + TextFit.LineHeight(TextFont()) + 0.5;

  /// <summary>
  ///   Fit <paramref name="text"/> into a section placed at <paramref name="rect"/>; the overflow continues in the next section
  /// </summary>
  public static TextFitResult Layout(XGraphics gfx, string text, XRect rect)
    => TextFit.Fit(gfx, text, TextFit.Family, XFontStyleEx.Regular, [TextRect(gfx, rect)], TextSize, TextSize);

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;

    XPen line = new(this.FontColor, lineSize);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    XFont boxTitleFont = TitleFont();

    string boxTitle = this._title + TitleSuffix;
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);

    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    XRect[] rects = [TextRectFromContent(contentRect, size.Height)];
    TextFitResult fit = TextFit.Fit(gfx, this._text, TextFit.Family, XFontStyleEx.Regular, rects, TextSize, TextSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects);
    this.Overflow = fit.Overflow;
  }

  #endregion

  #region Private Methods

  private static XFont TitleFont() => new(TextFit.Family, TitleSize, XFontStyleEx.Bold);

  private static XFont TextFont() => new(TextFit.Family, TextSize, XFontStyleEx.Regular);

  private static double TitleHeight(XGraphics gfx) => gfx.MeasureString("X", TitleFont()).Height;

  /// <summary>
  ///   Text width of a section of <paramref name="width"/>: margin 1, padding 2, text inset 4 left and 4 right
  /// </summary>
  private static double TextWidth(double width) => width - 1 - 2 - 8;

  /// <summary>
  ///   Text area of a section placed at <paramref name="rect"/>, mirroring the margins of <see cref="InformationBox"/>
  /// </summary>
  private static XRect TextRect(XGraphics gfx, XRect rect)
  {
    XRect contentRect = new(rect.X - 0.5 + 2, rect.Y - 0.5 + 2, rect.Width - 1 - 2, rect.Height - 1 - 2);
    return TextRectFromContent(contentRect, TitleHeight(gfx));
  }

  private static XRect TextRectFromContent(XRect contentRect, double titleHeight) => new
  (
    contentRect.X + 4,
    contentRect.Y + titleHeight + 4,
    contentRect.Width - 8,
    contentRect.Height - titleHeight - 10
  );

  #endregion
}
