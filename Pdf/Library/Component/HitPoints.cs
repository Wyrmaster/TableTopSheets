using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component;

public class HitPoints: BaseComponent
{
  #region Field
  
  private readonly string _hitPoints;
  private readonly string _hitDice;

  #endregion

  #region Constants

  private const double X = 370.57;
  private const double Y = 8.6;
  private const double Width = 211.43;
  private const double Height = 83.05;

  #endregion
  
  #region Constructor
  
  public HitPoints(string hitPoints, string hitDice) : base(new XRect(X, Y, Width, Height))
  {
    this._hitPoints = hitPoints;
    this._hitDice = hitDice;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double ellipseSize = 9.13;
    gfx.DrawEllipse(this.BaseBrush, X + Width - ellipseSize, Y, ellipseSize, ellipseSize);

    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    XPen thin = new XPen(BaseColor, 0.5);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    XPen verticalLine = new XPen(this.FontColor, 1);
    
    #region Outer Border
    
    const double outerBoxWidth = Width;
    const double outerBottomLeftCorner = 11;
    const double outerRadius = 20.05;
    
    XGraphicsPath outerRing = new XGraphicsPath();
    
    outerRing.AddArc
    (
      X + Width - outerRadius,
      Y - outerRadius,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      -180,
      -90
    );
    outerRing.AddLine
    (
      new(X + Width, Y + outerRadius),
      new(X + Width, Y + Height)
    );
    outerRing.AddLine
    (
      new(X + Width, Y + Height),
      new (X + outerBottomLeftCorner, Y + Height)
    );
    outerRing.AddLine
    (
      new (X + outerBottomLeftCorner, Y + Height - (innerBorderSize * 1.5)),
      new (X, Y + Height - outerBottomLeftCorner)
    );
    outerRing.AddLine
    (
      new (X, Y + Height - outerBottomLeftCorner),
      new(X, Y)
    );
    
    outerRing.CloseFigure();
    
    gfx.DrawPath(thin, outerRing);
    
    
    #endregion
    
    #region Inner Border
    
    const double innerBoxWidth = outerBoxWidth - Constants.OuterMargin;
    const double innerBottomLeftCorner = 5.5;
    const double innerRadius = 12;
    
    XGraphicsPath innerRing = new XGraphicsPath();
    
    innerRing.AddArc
    (
      X + Width - Constants.OuterMargin - innerRadius,
      Y + Constants.OuterMargin - innerRadius,
      innerRadius + innerRadius,
      innerRadius + innerRadius,
      -180,
      -90
    );
    innerRing.AddLine
    (
      new(X + Width - Constants.OuterMargin, Y + Constants.OuterMargin + innerRadius),
      new(X + Width - Constants.OuterMargin, Y + Height - (innerBorderSize * 1.5))
    );
    innerRing.AddLine
    (
      new(X + Width - Constants.OuterMargin, Y + Height - (innerBorderSize * 1.5)),
      new (X + innerBottomLeftCorner, Y + Height - (innerBorderSize * 1.5))
    );
    innerRing.AddLine
    (
      new (X + innerBottomLeftCorner, Y + Height - (innerBorderSize * 1.5)),
      new (X + (innerBorderSize * 1.5), Y + Height - innerBottomLeftCorner)
    );
    innerRing.AddLine
    (
      new (X + (innerBorderSize * 1.5), Y + Height - innerBottomLeftCorner),
      new (X + (innerBorderSize * 1.5), Y + Constants.OuterMargin)
    );
    
    innerRing.CloseFigure();
    
    gfx.DrawPath(thick, innerRing);
    
    #endregion
    
    #region Small Corner

    gfx.DrawPolygon
    (
      this.BaseBrush,
      [
        new XPoint(X - 2.5, Y + Height + 0.2),
        new XPoint(X + outerBottomLeftCorner - 6, Y + Height + 0.2),
        new XPoint(X + outerBottomLeftCorner - 6, Y + Height - 2.5),
      ],
      XFillMode.Winding
    );

    #endregion

    #region Right Bezier

    XGraphicsPath rightBezier = new();
    
    rightBezier.AddBezier
    (
      new (X + 206.12, Y + 14.65),
      new (X + 205.875, Y + 24.64),
      new (X + 205.497, Y + 28.181),
      new (X + 199.085, Y + 34.244)
    );
    rightBezier.AddBezier
    (
      new (X + 199.085, Y + 34.244),
      new (X + 205.456, Y + 49.43),
      new (X + 205.845, Y + 66.686),
      new (X + 206.291, Y + 81.936)
    );
    
    gfx.DrawPath(thin, rightBezier);

    #endregion

    XFont header = new("Roboto#", 9, XFontStyleEx.Bold);
    XFont section = new("Roboto#", 7);
    
    #region Vertical Lines

    gfx.DrawLine
    (
      verticalLine,
      new(X + outerBoxWidth / 2, Y + innerBorderSize + Constants.OuterMargin + 0.5),
      new(X + outerBoxWidth / 2, Y + Height - innerBorderSize - 1.2)
    );
    
    gfx.DrawLine
    (
      verticalLine,
      new(X + outerBoxWidth * 0.75, Y + innerBorderSize + Constants.OuterMargin + 0.5),
      new(X + outerBoxWidth * 0.75, Y + Height - innerBorderSize - 1.2)
    );
    
    #endregion

    #region Texts

    const string hitPoints = "HIT POINTS";
    const string hitDice = "HIT DICE";
    const string deathSaves = "DEATH SAVES";

    XSize hpSize = gfx.MeasureString(hitPoints, header);
    gfx.DrawString
    (
      hitPoints,
      header,
      this.FontBrush,
      X + outerBoxWidth * 0.25 - hpSize.Width / 2,
      Y + innerBorderSize + Constants.OuterMargin + 0.5 + hpSize.Height
    );
    XSize size = gfx.MeasureString(hitDice, header);
    gfx.DrawString
    (
      hitDice,
      header,
      this.FontBrush,
      X + outerBoxWidth * 0.625 - size.Width / 2,
      Y + innerBorderSize + Constants.OuterMargin + 0.5 + size.Height
    );
    size = gfx.MeasureString(deathSaves, header);
    XTextFormatter formatter = new(gfx);
    formatter.Alignment = XParagraphAlignment.Left;
    formatter.DrawString
    (
      deathSaves,
      header,
      this.FontBrush,
      new XRect
      (
        X + outerBoxWidth * 0.9 - size.Width / 2,
        Y + innerBorderSize + Constants.OuterMargin + 0.5 + size.Height / 2 - 4,
        outerBoxWidth * 0.125,
        50
      ),
      XStringFormats.TopLeft
    );

    #endregion

    #region Inner Lines

    const double verticalHpLine = 1;
    
    gfx.DrawLine
    (
      new XPen(this.BaseColor, verticalHpLine),
      new (X + outerBoxWidth * 0.25 - verticalHpLine / 2, Y + outerBorderSize + innerBorderSize + Constants.OuterMargin + hpSize.Height + 3),
      new (X + outerBoxWidth * 0.25 - verticalHpLine / 2, Y + Height - 9)
    );

    const double innerLineMargin = 6;
    const double horizontalLine = 0.45;
    XPen horizontalPen = new(this.BaseColor, horizontalLine);
    
    gfx.DrawLine
    (
      horizontalPen,
      new(X + innerLineMargin, Y + Height - 18),
      new(X - innerLineMargin + outerBoxWidth * 0.25 - verticalHpLine / 2, Y + Height - 18)
    );

    const string current = "CURRENT";
    size = gfx.MeasureString(current, section);
    gfx.DrawString
    (
      current,
      section,
      this.FontBrush,
      X + innerLineMargin + 0.25, 
      Y + Height - 20 + size.Height
    );
    
    gfx.DrawLine
    (
      horizontalPen,
      new(X + innerLineMargin + outerBoxWidth * 0.25, Y + Height - 18),
      new(X - innerLineMargin + outerBoxWidth * 0.5 - verticalHpLine / 2, Y + Height - 18)
    );

    const string max = "MAX";
    size = gfx.MeasureString(max, section);
    gfx.DrawString
    (
      max,
      section,
      this.FontBrush,
      X + innerLineMargin + outerBoxWidth * 0.25 + 0.25, 
      Y + Height - 20 + size.Height
    );
    
    XRect hitPointsRect = new
    (
      X + innerLineMargin + outerBoxWidth * 0.25, 
      Y + Height - 18 - 15,
      outerBoxWidth * 0.25 - 11,
      15
    );
    gfx.DrawText(this._hitPoints, hitPointsRect);
    
    gfx.DrawLine
    (
      horizontalPen,
      new(X + innerLineMargin + outerBoxWidth * 0.5, Y + Height - 18),
      new(X - innerLineMargin + outerBoxWidth * 0.75 - verticalHpLine / 2, Y + Height - 18)
    );
    
    size = gfx.MeasureString(max, section);
    gfx.DrawString
    (
      max,
      section,
      this.FontBrush,
      X + innerLineMargin + outerBoxWidth * 0.5 + 0.25, 
      Y + Height - 20 + size.Height
    );
    gfx.DrawText(this._hitPoints, hitPointsRect);
    
    XRect hitDiceRect = new
    (
      X + innerLineMargin + outerBoxWidth * 0.5,
      Y + Height - 18 - 14,
      outerBoxWidth * 0.25 - 11,
      15
    );
    gfx.DrawText(this._hitDice, hitDiceRect);
    
    gfx.DrawLine
    (
      horizontalPen,
      new(X + innerLineMargin + outerBoxWidth * 0.25, Y + Height - 41),
      new(X - innerLineMargin + outerBoxWidth * 0.5 - verticalHpLine / 2, Y + Height - 41)
    );

    const string temp = "TEMP";
    size = gfx.MeasureString(temp, section);
    gfx.DrawString
    (
      temp,
      section,
      this.FontBrush,
      X + innerLineMargin + outerBoxWidth * 0.25 + 0.25, 
      Y + Height - 43 + size.Height
    );
    
    gfx.DrawLine
    (
      horizontalPen,
      new(X + innerLineMargin + outerBoxWidth * 0.5, Y + Height - 41),
      new(X - innerLineMargin + outerBoxWidth * 0.75 - verticalHpLine / 2, Y + Height - 41)
    );

    const string spent = "SPENT";
    size = gfx.MeasureString(spent, section);
    gfx.DrawString
    (
      spent,
      section,
      this.FontBrush,
      X + innerLineMargin + outerBoxWidth * 0.5 + 0.25, 
      Y + Height - 43 + size.Height
    );
    
    #endregion

    #region DeathSaveLines

    const double verticalLowerMultiplier = 6.8;
    const double deathSaveBackgroundLineSize = 1.5;
    XGraphicsPath path = new();
    
    path.AddLine
    (
      new(X + outerBoxWidth * 0.75 + 2, Y + Height / 9 * 4.5),
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * 4.5 - deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * 4.5 - deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * 4.5 - deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * 4.5 - deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.95, Y + Height / 9 * 4.5)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.95, Y + Height / 9 * 4.5),
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * 4.5 + deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * 4.5 + deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * 4.5 + deathSaveBackgroundLineSize / 2)
    );
    path.CloseFigure();
    
    gfx.DrawPath(this.BaseBrush, path);
    
    path = new();
    
    path.AddLine
    (
      new(X + outerBoxWidth * 0.75 + 2, Y + Height / 9 * verticalLowerMultiplier),
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * verticalLowerMultiplier - deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * verticalLowerMultiplier - deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * verticalLowerMultiplier - deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * verticalLowerMultiplier - deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.95, Y + Height / 9 * verticalLowerMultiplier)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.95, Y + Height / 9 * verticalLowerMultiplier),
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * verticalLowerMultiplier + deathSaveBackgroundLineSize / 2)
    );
    path.AddLine
    (
      new(X + outerBoxWidth * 0.9, Y + Height / 9 * verticalLowerMultiplier + deathSaveBackgroundLineSize / 2),
      new(X + outerBoxWidth * 0.75 + 8, Y + Height / 9 * verticalLowerMultiplier + deathSaveBackgroundLineSize / 2)
    );
    path.CloseFigure();
    
    gfx.DrawPath(this.BaseBrush, path);
    
    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6, Y + Height / 9 * 4.5, 0, 0),
      true
    ).Draw(gfx);
    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6 + 11, Y + Height / 9 * 4.5, 0, 0),
      true
    ).Draw(gfx);
    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6 + 22, Y + Height / 9 * 4.5, 0, 0),
      true
    ).Draw(gfx);
    const string successes = "SUCCESSES";
    size = gfx.MeasureString(successes, section);
    gfx.DrawString
    (
      successes,
      section,
      this.FontBrush,
      X + outerBoxWidth * 0.75 + 4,
      Y + Height / 9 * 4.5 + size.Height + 3
    );


    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6, Y + Height / 9 * verticalLowerMultiplier, 0, 0),
      true
    ).Draw(gfx);
    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6 + 11, Y + Height / 9 * verticalLowerMultiplier, 0, 0),
      true
    ).Draw(gfx);
    new CurveBox
    (
      new(X + outerBoxWidth * 0.75 + 6 + 22, Y + Height / 9 * verticalLowerMultiplier, 0, 0),
      true
    ).Draw(gfx);
    const string failures = "FAILURES";
    size = gfx.MeasureString(failures, section);
    gfx.DrawString
    (
      failures,
      section,
      this.FontBrush,
      X + outerBoxWidth * 0.75 + 4,
      Y + Height / 9 * verticalLowerMultiplier + size.Height + 3
    );

    #endregion
  }

  #endregion
}