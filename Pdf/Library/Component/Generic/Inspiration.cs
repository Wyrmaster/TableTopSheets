using System.Reflection;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public class Inspiration : BaseComponent
{
  #region Constructor

  public Inspiration(XRect rect) : base(rect)
  {
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    const double lineSize = 0.9;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    XPen line = new XPen(this.FontColor, lineSize);

    const double innerRadius = 3;
    const double outerRadius = 6;

    double topBorder = this.Rect.Height / 18 * 8;
    double borderRadius = this.Rect.Height - topBorder;

    XGraphicsPath path;

    #region Border Curly

    gfx.DrawBezier
    (
      thin,
      new XPoint(0.15655501, 22.670141).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(0.70449755, 39.264972).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(22.309089, 43.726789).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(17.690716, 60.165065).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );

    gfx.DrawBezier
    (
      thin,
      new XPoint(0.15655501, 22.670141).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(0.70449755, 39.264972).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(22.309089, 43.726789).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(17.690716, 60.165065).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );

    #endregion

    #region Line Curly

    path = new();
    
    path.AddBezier
    (
      new XPoint(35.910000000000004, 46.4373).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(32.921, 34.9137).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(21.823800000000002, 28.5).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(15.0982, 28.5).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
   
    path.AddLines
    (
      [
        new XPoint(15.0982, 28.5).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(35.822900000000004, 28.5).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(35.822900000000004, 56.2121).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      ]
    );
  
    gfx.DrawPath(thin, path);
    
    path = new();
    
    path.AddBezier
    (
      new XPoint(35.910000000000004, 46.4373).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(32.921, 34.9137).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(21.823800000000002, 28.5).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(15.0982, 28.5).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
   
    path.AddLines
    (
      [
        new XPoint(15.0982, 28.5).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(35.822900000000004, 28.5).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(35.822900000000004, 56.2121).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      ]
    );
  
    gfx.DrawPath(thin, path);

    #endregion

    #region Circle

    const double radius = 22;
    gfx.DrawArc
    (
      thick,
      this.Rect.X + this.Rect.Width / 2 - radius,
      this.Rect.Y + this.Rect.Height - radius - radius,
      radius + radius,
      radius + radius,
      -37.5,
      255
    );

    #endregion

    #region Inner Star

    const double starRadius = 10;
    const double yCorrection = 2.5;
    
    XRect starRect = new
    (
      this.Rect.X + this.Rect.Width / 2 - starRadius,
      this.Rect.Y + this.Rect.Height + yCorrection - radius - starRadius,
      starRadius + starRadius,
      starRadius + starRadius
    );

    path = new();
    
    path.AddBezier
    (
      new XPoint(starRadius, 0).Flip() + new XVector(starRect.X, starRect.Y),
      new XPoint(starRadius, starRadius /2).Flip() + new XVector(starRect.X, starRect.Y),
      new XPoint(starRadius /2, starRadius).Flip() + new XVector(starRect.X, starRect.Y),
      new XPoint(0, starRadius).Flip() + new XVector(starRect.X, starRect.Y)

    );
    path.AddBezier
    (
      new XPoint(0, starRadius).Flip(flipY: true) + new XVector(starRect.X, starRect.Y + starRect.Height),
      new XPoint(starRadius /2, starRadius).Flip(flipY: true) + new XVector(starRect.X, starRect.Y + starRect.Height),
      new XPoint(starRadius, starRadius /2).Flip(flipY: true) + new XVector(starRect.X, starRect.Y + starRect.Height),
      new XPoint(starRadius, 0).Flip(flipY: true) + new XVector(starRect.X, starRect.Y + starRect.Height)
    );
    path.AddBezier
    (
      new XPoint(starRadius, 0).Flip(true, true) + new XVector(starRect.X + starRect.Width, starRect.Y + starRect.Height),
      new XPoint(starRadius, starRadius /2).Flip(true, true) + new XVector(starRect.X + starRect.Width, starRect.Y + starRect.Height),
      new XPoint(starRadius /2, starRadius).Flip(true, true) + new XVector(starRect.X + starRect.Width, starRect.Y + starRect.Height),
      new XPoint(0, starRadius).Flip(true, true) + new XVector(starRect.X + starRect.Width, starRect.Y + starRect.Height)
    );
    path.AddBezier
    (
      new XPoint(0, starRadius).Flip(flipX: true) + new XVector(starRect.X + starRect.Width, starRect.Y),
      new XPoint(starRadius /2, starRadius).Flip(flipX: true) + new XVector(starRect.X + starRect.Width, starRect.Y),
      new XPoint(starRadius, starRadius /2).Flip(flipX: true) + new XVector(starRect.X + starRect.Width, starRect.Y),
      new XPoint(starRadius, 0).Flip(flipX: true) + new XVector(starRect.X + starRect.Width, starRect.Y)
    );
    
    gfx.DrawPath(this.BaseBrush, path);

    const double curveBoxCorrection = 1.75;
    new CurveBox(new(
      this.Rect.X + this.Rect.Width / 2 - starRadius / 2 + curveBoxCorrection,
      this.Rect.Y + this.Rect.Height + yCorrection - radius - starRadius / 2 + 0.5,
      0,
      0
    ), scale: 1.25).Draw(gfx);

    #endregion

    #region Line

    const double linePadding = 10;
    gfx.DrawLine
    (
      line,
      new (this.Rect.X + linePadding, this.Rect.Y + topBorder),
      new (this.Rect.X + this.Rect.Width - linePadding, this.Rect.Y + topBorder)
    );
    
    #endregion

    #region Border

    path = new();

    path.AddLines
    (
      [
        new(this.Rect.X + outerRadius, this.Rect.Y),
        new(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + topBorder),
      ]
    );

    path.AddArc
    (
      this.Rect.X + this.Rect.Width - borderRadius - borderRadius,
      this.Rect.Y - 7.125,
      borderRadius + borderRadius,
      borderRadius + borderRadius,
      0,
      90
    );

    path.AddArc
    (
      this.Rect.X,
      this.Rect.Y - 7.125,
      borderRadius + borderRadius,
      borderRadius + borderRadius,
      90,
      90
    );

    path.AddArc
    (
      this.Rect.X - outerRadius,
      this.Rect.Y - outerRadius,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      90,
      -90
    );

    path.CloseFigure();

    gfx.DrawPath(thick, path);

    gfx.DrawEllipse
    (
      this.BaseBrush,
      this.Rect.X - innerRadius,
      this.Rect.Y - innerRadius,
      innerRadius + innerRadius,
      innerRadius + innerRadius
    );

    #endregion

    #region Text

    const double textWidth = 75;
    XRect headerText = new
    (
      this.Rect.X + this.Rect.Width / 2 - textWidth / 2,
      this.Rect.Y + 2,
      textWidth,
      topBorder - 2
    );
    
    const string heroicInspiration = "HEROIC INSPIRATION";
    XTextFormatter formatter = new(gfx);
    formatter.Alignment = XParagraphAlignment.Center;
    formatter.DrawString(heroicInspiration, new XFont("Roboto#", 9, XFontStyleEx.Bold),this.FontBrush, headerText);

    #endregion
  }

  #endregion
}