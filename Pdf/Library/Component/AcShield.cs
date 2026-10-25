using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component;

public class AcShield: BaseComponent
{
  #region Constants

  private const double X = 309.5;
  private const double Y = 8.6;
  private const double Width = 57.41;
  private const double Height = 83.05;

  #endregion

  #region Field

  private readonly string _armorClass;
  private readonly bool _equippedShield;

  #endregion
  
  #region Constructor
  
  public AcShield(string armorClass, bool equippedShield) : base(new XRect(X, Y, Width, Height))
  {
    this._armorClass = armorClass;
    this._equippedShield = equippedShield;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double innerPadding = 5.22;
    
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    XPen thin = new XPen(BaseColor, 0.5);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    
    XFont font = new XFont("Roboto#", 6.5);
    XFont fontB = new XFont("Roboto#", 8.5, XFontStyleEx.Bold);

    const string armor = "ARMOR";
    const string @class = "CLASS";
    XSize size = gfx.MeasureString(armor, fontB);
    XRect rect = new XRect(X + Width / 2 - size.Width / 2, Y + 7, size.Width, size.Height);
    gfx.DrawString(armor, fontB, this.FontBrush, rect, XStringFormats.Center);
    size = gfx.MeasureString(@class, fontB);
    rect = new XRect(X + Width / 2 - size.Width / 2, Y + rect.Height + 5, size.Width, size.Height);
    gfx.DrawString(@class, fontB, this.FontBrush, rect, XStringFormats.Center);

    XGraphicsPath primaryBorder = new();
    
    primaryBorder.AddLine
    (
      new(X + 5.32, Y + 3.34),
      new(X + 5.32, Y + 52.92)
    );
    primaryBorder.AddBezier
    (
      new(X + 5.32, Y + 52.92),
      new(X + 11.59, Y + 64.50),
      new(X + 18.58, Y + 72.64),
      new(X + 28.7, Y + 80.57)
    );
    primaryBorder.AddBezier
    (
      new(X + 28.7, Y + 80.57),
      new(X + 39.33, Y + 72.08),
      new(X + 44.75, Y + 64.46),
      new(X + 51.75, Y + 52.92)
    );
    primaryBorder.AddLine
    (
      new(X + 51.75, Y + 52.92),
      new(X + 51.75, Y + 3.34)
    );
    primaryBorder.CloseFigure();
    gfx.DrawPath(thick, primaryBorder);
    
    gfx.DrawBezier
    (
      thin,
      new(X + 5.32, Y + 34.76),
      new(X + 6.89, Y + 49.47),
      new(X + 12.52, Y + 55.52),
      new(X + 15.66, Y + 58.45)
    );
    gfx.DrawBezier
    (
      thin,
      new(X + 51.56, Y + 34.76),
      new(X + 50, Y + 49.52),
      new(X + 44.36, Y + 55.58),
      new(X + 41.23, Y + 58.50)
    );
    
    XGraphicsPath leftPeak = new();
    
    gfx.DrawPolygon
    (
      this.BaseBrush,
      [
        new(X + 1.15, Y + 2.4),
        new(X + 5.84, Y + 2.4),
        new(X + 5.84, Y + 6.78),
        new(X + 1.15, Y + 2.4),
      ],
      XFillMode.Winding
    );
    gfx.DrawPolygon
    (
      this.BaseBrush,
      [
        new(X + 56.11, Y + 2.4),
        new(X + 51.41, Y + 2.4),
        new(X + 51.41, Y + 6.79),
        new(X + 56.11, Y + 2.4),
      ],
      XFillMode.Winding
    );

    new CurveBox
    (
      new
      (
        this.Rect.X + this.Rect.Width / 2 - CurveBox.Width / 2 + 1.6,
        this.Rect.Y + this.Rect.Height - CurveBox.Height / 2 - 8,
        CurveBox.Height,
        CurveBox.Height
      ),
      true,
      this._equippedShield
    ).Draw(gfx);
    
    const string shield = "SHIELD";
    size = gfx.MeasureString(shield, font);
    rect = new XRect(X + Width / 2 - size.Width / 2,  this.Rect.Y + this.Rect.Height - size.Height * 2 - CurveBox.Height / 2 - 4.5, size.Width, size.Height);
    gfx.DrawString(shield, font, this.FontBrush, rect, XStringFormats.Center);

    const double acRectSize = 20;
    XRect acRect = new
    (
      X + Width / 2 - acRectSize * 1.25 / 2,
      Y + Height/ 2 - acRectSize / 2,
      acRectSize * 1.25,
      acRectSize
    );
    
    gfx.DrawText(this._armorClass, acRect);
  }

  #endregion
}