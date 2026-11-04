using System.Globalization;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using SheetCoins = TableTopSheets.Common.Sheet.Coins;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Coins box: one labelled box per coin (CP, SP, EP, GP, PP) with the amount; empty boxes for coins the character has
///   none of
/// </summary>
public class Coins : InformationBox
{
  #region Fields

  private readonly SheetCoins _coins;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 762.14238;
  private const double Width = 178.971;
  private const double Height = 68;

  private const double CoinGap = 4;

  /// <summary>
  ///   Radius of the concave bottom corners of a coin slot
  /// </summary>
  private const double NotchRadius = 3;

  /// <summary>
  ///   Size of the chevrons on the top line and below a coin slot
  /// </summary>
  private const double ChevronWidth = 4;
  private const double ChevronHeight = 3;

  #endregion

  #region Constructor

  public Coins(SheetCoins coins) : base(new(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Curve, CornerEnum.Edge)
  {
    this._coins = coins;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double topLineSize = 1.4;
    const double sideLineSize = 1.2;

    XPen line = new(this.FontColor, lineSize);
    XPen topLine = new(this.FontColor, topLineSize);
    XPen sideLine = new(XColor.FromArgb(176, 177, 181), sideLineSize);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

    XFont labelFont = new("Roboto#", 7, XFontStyleEx.Regular);
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

    (string Label, int Amount)[] coins =
    [
      ("CP", this._coins.Copper),
      ("SP", this._coins.Silver),
      ("EP", this._coins.Electrum),
      ("GP", this._coins.Gold),
      ("PP", this._coins.Platinum),
    ];

    double left = contentRect.X + 4;
    double coinWidth = (contentRect.Width - 8 - CoinGap * (coins.Length - 1)) / coins.Length;
    double labelTop = contentRect.Y + size.Height + 4;
    double labelHeight = TextFit.LineHeight(labelFont);
    // room for the chevron above the top line
    double boxTop = labelTop + labelHeight + ChevronHeight + 1;
    // room for the chevron below the box and the curved corner at the bottom right
    double boxHeight = contentRect.Bottom - boxTop - ChevronHeight - 10;

    for (int index = 0; index < coins.Length; index++)
    {
      (string label, int amount) = coins[index];
      XRect labelRect = new(left + index * (coinWidth + CoinGap), labelTop, coinWidth, labelHeight);
      XRect boxRect = labelRect with { Y = boxTop, Height = boxHeight };

      TextFit.DrawLine(gfx, label, labelFont, titleBrush, labelRect, XParagraphAlignment.Center);
      this.DrawCoinBox(gfx, boxRect, topLine, sideLine);
      if (amount == 0)
      {
        continue;
      }

      (XFont font, string text, _) = TextFit.FitSingleLine
      (
        gfx,
        amount.ToString(CultureInfo.InvariantCulture),
        TextFit.Family,
        XFontStyleEx.Bold,
        10,
        TextFit.BodyMinSize,
        boxRect.Width - 4
      );
      double height = TextFit.LineHeight(font);
      TextFit.DrawLine(gfx, text, font, this.FontBrush, boxRect with { Y = boxRect.Y + (boxRect.Height - height) / 2, Height = height }, XParagraphAlignment.Center);
    }
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   One coin slot like on the official sheet: a dark top line with a chevron pointing up, grey sides and bottom with
  ///   concave corners at the bottom, and a chevron pointing down below it
  /// </summary>
  private void DrawCoinBox(XGraphics gfx, XRect rect, XPen topLine, XPen sideLine)
  {
    double centerX = rect.X + rect.Width / 2;

    // sides and bottom; the bottom corners are quarter circles around the corner points, cut into the box
    XGraphicsPath outline = new();
    outline.AddLine(rect.Left, rect.Top, rect.Left, rect.Bottom - NotchRadius);
    outline.AddArc(rect.Left - NotchRadius, rect.Bottom - NotchRadius, NotchRadius * 2, NotchRadius * 2, 270, 90);
    outline.AddLine(rect.Left + NotchRadius, rect.Bottom, rect.Right - NotchRadius, rect.Bottom);
    outline.AddArc(rect.Right - NotchRadius, rect.Bottom - NotchRadius, NotchRadius * 2, NotchRadius * 2, 180, 90);
    outline.AddLine(rect.Right, rect.Bottom - NotchRadius, rect.Right, rect.Top);
    gfx.DrawPath(sideLine, outline);

    gfx.DrawLine(topLine, rect.Left - 0.5, rect.Top, rect.Right + 0.5, rect.Top);
    gfx.DrawLines(topLine, [
      new XPoint(centerX - ChevronWidth / 2, rect.Top),
      new XPoint(centerX, rect.Top - ChevronHeight),
      new XPoint(centerX + ChevronWidth / 2, rect.Top),
    ]);
    gfx.DrawLines(topLine, [
      new XPoint(centerX - ChevronWidth / 2, rect.Bottom + 0.5),
      new XPoint(centerX, rect.Bottom + ChevronHeight + 0.5),
      new XPoint(centerX + ChevronWidth / 2, rect.Bottom + 0.5),
    ]);
  }

  #endregion
}
