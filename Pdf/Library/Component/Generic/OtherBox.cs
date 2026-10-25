using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public class OtherBox: BaseComponent
{
  #region Field

  private readonly string _value;
  private readonly string _header;
  private readonly XMatrix _zRotationMatrix;

  #endregion

  #region Constructor

  public OtherBox(string value, string header, XRect rect) : base(rect)
  {
    this._value = value;
    this._header = header;
    this._zRotationMatrix = new XMatrix(-1, 0, 0, 1, 0, 0);
  }
  
  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    const double lineSize = 0.7;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    XPen line = new XPen(this.FontColor, lineSize);

    const double lowerEdgeX = 4;
    const double lowerEdgeY = 5;
    const double innerRadius = 3;
    const double outerRadius = 6;

    #region Border Curls

    XGraphicsPath borderCurl = new();
    
    borderCurl.AddBezier
    (
      new XPoint(0.834450, 18.74656).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(1.698588, 22.85814).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(4.24643, 26.64159).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(8.379339, 28.59321).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
   
    borderCurl.AddLines
    (
      [
        new XPoint(8.379339, 28.59321).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(8.379339, 48.23261).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(8.357680, 48.23261).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(13.15093, 48.23261).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      ]
    );
   
    borderCurl.AddBezier
    (
      new XPoint(13.15093, 48.23261).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(5.229192, 44.81711).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(1.469260, 39.19160).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(0.694321, 33.10684).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, borderCurl);
    
    borderCurl = new();
    
    borderCurl.AddBezier
    (
      new XPoint(0.834450, 18.74656).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(1.698588, 22.85814).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(4.24643, 26.64159).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(8.379339, 28.59321).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
   
    borderCurl.AddLines
    (
      [
        new XPoint(8.379339, 28.59321).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(8.379339, 48.23261).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(8.357680, 48.23261).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(13.15093, 48.23261).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      ]
    );
   
    borderCurl.AddBezier
    (
      new XPoint(13.15093, 48.23261).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(5.229192, 44.81711).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(1.469260, 39.19160).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(0.694321, 33.10684).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, borderCurl);

    #endregion

    #region MiddleCurve

    borderCurl = new();
    borderCurl.AddBeziers
    (
      [
        new XPoint(11.77318, 17.113232).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(12.23243, 27.539768).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(22.27807, 34.81022).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(22.56506, 48.054692).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(21.70068, 48.027044).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(21.0145, 47.940212).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(20.15408, 47.998424).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(14.76386, 39.75338).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(13.41856, 27.4754).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(18.66160, 17.113232).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      ]
    );
    gfx.DrawPath(thin, borderCurl);
    
    borderCurl = new();
    borderCurl.AddBeziers
    (
      [
        new XPoint(11.77318, 17.113232).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(12.23243, 27.539768).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(22.27807, 34.81022).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(22.56506, 48.054692).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(21.70068, 48.027044).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(21.0145, 47.940212).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(20.15408, 47.998424).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(14.76386, 39.75338).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(13.41856, 27.4754).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(18.66160, 17.113232).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      ]
    );
    gfx.DrawPath(thin, borderCurl);

    #endregion

    #region Text Line

    const double lineIndent = 5;
    gfx.DrawLine
    (
      line,
      new(this.Rect.X + lineIndent, this.Rect.Y + 17),
      new(this.Rect.X + this.Rect.Width - lineIndent, this.Rect.Y + 17)
    );

    XRect titleRect = new
    (
      this.Rect.X + lineIndent,
      this.Rect.Y + 4,
      this.Rect.Width - lineIndent * 2,
      11
    );
    gfx.DrawText(this._header, titleRect, XFontStyleEx.Bold);

    const double rectSize = 25;
    XRect valueRect = new
    (
      this.Rect.X + this.Rect.Width / 2 - rectSize / 2,
      this.Rect.Y + this.Rect.Height - 30,
      rectSize,
      rectSize
    );
    
    gfx.DrawText(this._value, valueRect, center: true);
    
    #endregion
    
    #region Border

    XGraphicsPath path = new();
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
  }

  #endregion
}