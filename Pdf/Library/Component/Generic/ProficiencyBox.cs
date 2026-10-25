using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public class ProficiencyBox : BaseComponent
{
  #region Field

  private readonly string _proficiencyBonus;

  #endregion

  #region Constructor

  public ProficiencyBox(XRect rect, string proficiencyBonus) : base(rect)
  {
    this._proficiencyBonus = proficiencyBonus;
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

    const double lowerEdgeX = 4;
    const double lowerEdgeY = 5;
    const double innerRadius = 3;
    const double outerRadius = 6;

    XGraphicsPath path;

    #region Left Curlys

    path = new();

    path.AddBezier
    (
      new XPoint(0.31311, 24.5009).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(0.469665, 29.2758).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(11.5851, 42.1868).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(12.5244, 48.9234).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );

    path.AddLines
    (
      [
        new XPoint(12.5244, 48.9234).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(12.5244, 70.4498).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(8.9, 70.4498).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(8.9, 32.6417).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      ]
    );

    gfx.DrawPath(thin, path);

    #endregion

    #region Right Curlys

    path = new();

    path.AddBezier
    (
      new XPoint(0.31311, 24.5009).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(0.469665, 29.2758).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(11.5851, 42.1868).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(12.5244, 48.9234).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );

    path.AddLines
    (
      [
        new XPoint(12.5244, 48.9234).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(12.5244, 70.4498).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(8.9, 70.4498).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(8.9, 32.6417).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      ]
    );

    gfx.DrawPath(thin, path);

    #endregion

    const double middleBar = 34.44736842;
    const double upperLine = 20.70492;
    const double middleBarPadding = 8;

    #region MiddleBox

    path = new();

    path.AddLine
    (
      new XPoint(this.Rect.X + this.Rect.Width / 2 - middleBar / 2, this.Rect.Y + upperLine),
      new XPoint(this.Rect.X + this.Rect.Width / 2 - middleBar / 2, this.Rect.Y + this.Rect.Height - 12.1)
    );

    path.AddArc
    (
      this.Rect.X + this.Rect.Width / 2 - middleBar / 2 - outerRadius,
      this.Rect.Y + this.Rect.Height - 12.1,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      -90,
      90
    );

    path.AddLine
    (
      new XPoint(this.Rect.X + this.Rect.Width / 2 - middleBar / 2 + outerRadius,
        this.Rect.Y + this.Rect.Height - 12.1 + outerRadius),
      new XPoint(this.Rect.X + this.Rect.Width / 2 - middleBar / 2 + middleBar - outerRadius - outerRadius,
        this.Rect.Y + this.Rect.Height - 12.1 + outerRadius)
    );

    path.AddArc
    (
      this.Rect.X + this.Rect.Width / 2 - middleBar / 2 + middleBar - outerRadius,
      this.Rect.Y + this.Rect.Height - 12.1,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      180,
      90
    );

    path.AddLine
    (
      new XPoint(this.Rect.X + this.Rect.Width / 2 + middleBar / 2, this.Rect.Y + upperLine),
      new XPoint(this.Rect.X + this.Rect.Width / 2 + middleBar / 2, this.Rect.Y + this.Rect.Height - 12.1)
    );

    gfx.DrawPath(thick, path);

    #endregion

    #region LeftMiddleCurls

    path = new();
    
    path.AddBezier
    (
      new XPoint(18.5645, 20.7259).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(29.2885, 22.3698).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(32.910129999999995, 33.305828).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(35, 38.103547).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    path.AddLine
    (
      new XPoint(35, 38.103547).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(35, 52.584908).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    path.AddBezier
    (
      new XPoint(35, 52.584908).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(38.510367, 47.34068).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(40.482178, 34.033093).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(40.1691, 20.7259).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
   
    gfx.DrawPath(thin, path);
   
    path = new();
   
    path.AddLine
    (
      new XPoint(35.472437, 31.919614).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(28.975404, 31.919614).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
   
    path.AddBezier
    (
      new XPoint(28.975404, 31.919614).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(25.068417, 23.622199).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(16.764113000000002, 20.882486).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(12.615405, 20.647653).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, path);
    
    #endregion
    
    #region RightMiddleCurls

    path = new();
    
    path.AddBezier
    (
      new XPoint(18.5645, 20.7259).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(29.2885, 22.3698).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(32.910129999999995, 33.305828).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(35, 38.103547).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    path.AddLine
    (
      new XPoint(35, 38.103547).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(35, 52.584908).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    path.AddBezier
    (
      new XPoint(35, 52.584908).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(38.510367, 47.34068).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(40.482178, 34.033093).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(40.1691, 20.7259).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
   
    gfx.DrawPath(thin, path);
   
    path = new();
    
    path.AddLine
    (
      new XPoint(35.472437, 31.919614).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(28.975404, 31.919614).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    path.AddBezier
    (
      new XPoint(28.975404, 31.919614).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(25.068417, 23.622199).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(16.764113000000002, 20.882486).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(12.615405, 20.647653).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, path);
    
    #endregion

    #region Topline

    gfx.DrawLine
    (
      line,
      new XPoint(this.Rect.X + middleBarPadding, this.Rect.Y + upperLine),
      new XPoint(this.Rect.X + this.Rect.Width - middleBarPadding, this.Rect.Y + upperLine)
    );
    
    #endregion

    #region Border

    path = new();
    path.AddLines
    (
      [
        new(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + this.Rect.Height - lowerEdgeY),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + lowerEdgeX, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X, this.Rect.Y + this.Rect.Height - lowerEdgeY),
        new(this.Rect.X, this.Rect.Y + outerRadius),
      ]
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

    path = new();
    path.AddLines
    (
      [
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX / 2, this.Rect.Y + this.Rect.Height - lowerEdgeY / 4),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX, this.Rect.Y + this.Rect.Height),
      ]
    );
    path.CloseFigure();
    gfx.DrawPath(thick, this.BaseBrush, path);

    path = new();
    path.AddLines
    (
      [
        new(this.Rect.X, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + lowerEdgeX / 2, this.Rect.Y + this.Rect.Height - lowerEdgeY / 4),
        new(this.Rect.X + lowerEdgeX, this.Rect.Y + this.Rect.Height),
      ]
    );
    path.CloseFigure();
    gfx.DrawPath(thick, this.BaseBrush, path);

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

    const string proficiencyBonus = "PROFICIENCY BONUS";

    double headerWidth = this.Rect.Width - middleBarPadding - middleBarPadding;
    XRect headerRect = new
    (
      this.Rect.X + this.Rect.Width / 2 - headerWidth / 2, 
      this.Rect.Y + 2, 
      headerWidth,
      upperLine - 3
    );
    gfx.DrawText(proficiencyBonus, headerRect, XFontStyleEx.Bold);

    const double valueBoxWidth = middleBar - 14;
    
    XRect valueRect = new
    (
      this.Rect.X + this.Rect.Width / 2 - valueBoxWidth / 2,
      this.Rect.Y + upperLine + 4,
      valueBoxWidth,
      30
    );
    
    gfx.DrawText(this._proficiencyBonus, valueRect, center: true);

    #endregion
  }

  #endregion
}