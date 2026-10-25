using System.Text.RegularExpressions;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf;

/// <summary>
///   SVG to PDF-Sharp path converter
/// </summary>
public class Svg
{
  #region Field

  private static readonly Regex SplitPathRegex = new(@"[MmLlHhVvCcSsQqTtAaZz][0-9,\.\s-]*", RegexOptions.Compiled);
  
  private readonly XPoint _startPoint;
  private readonly XSize _viewBox;
  private readonly XSize _size;
  private readonly XColor _color;
  private readonly string _path;

  private XPoint? _movePoint;
  private XPoint? _movePointGraph;
  private XPoint? _lastSCPoint;
  private XPoint? _lastQPoint;

  #endregion

  #region Constructor

  public Svg(XPoint startPoint, XSize viewBox, XSize size, string path, XColor color)
  {
    this._startPoint = startPoint;
    this._viewBox = viewBox;
    this._size = size;
    this._path = path;
    this._color = color;
  }

  #endregion
  
  #region Public Method

  public void DrawSvg(XGraphics gfx)
  {
    XGraphicsPath path = new();

    XPen pen = new XPen(this._color, 0.1);
    
    foreach (Match segmentMatch in Svg.SplitPathRegex.Matches(this._path))
    {
      string segment = segmentMatch.Value;
      switch (segment[0])
      {
        case 'M':
        {
          path.CloseFigure();
          path.StartFigure();
          double[] segments = this.GetPoints(segment.Substring(1));
          this.MoveAbs(new(segments[0], segments[1]));
          break;
        }
        case 'm':
        {
          path.CloseFigure();
          path.StartFigure();
          double[] segments = this.GetPoints(segment.Substring(1));
          this.MoveRel(new(segments[0], segments[1]));
          break;
        }
        case 'L':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.StraightLineAbs(path, new(segments[0], segments[1]));
          break;
        }
        case 'l':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.StraightLineRel(path, new(segments[0], segments[1]));
          break;
        }
        case 'H':
        {
          this.DrawHorizontalLineAbs(path, this.GetPoints(segment.Substring(1) + ",0")[0]);
          break;
        }
        case 'h':
        {
          this.DrawHorizontalLineRel(path, this.GetPoints(segment.Substring(1) + ",0")[0]);
          break;
        }
        case 'V':
        {
          this.DrawVerticalLineAbs(path, this.GetPoints(segment.Substring(1) + ",0")[0]);
          break;
        }
        case 'v':
        {
          this.DrawVerticalLineRel(path, this.GetPoints(segment.Substring(1) + ",0")[0]);
          break;
        }
        case 'C':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawCurveAbs
          (
            path,
            new(segments[0], segments[1]), 
            new(segments[2], segments[3]), 
            new(segments[4], segments[5])
          );
          break;
        }
        case 'c':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawCurveRel
          (
            path,
            new XVector(segments[0], segments[1]), 
            new(segments[2], segments[3]), 
            new(segments[4], segments[5])
          );
          break;
        }
        case 'S':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawCurveAbs
          (
            path,
            this._lastSCPoint!.Value,
            new(segments[0], segments[1]),
            new(segments[2], segments[3])
          );
          break;
        }
        case 's':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawCurveRel
          (
            path,
            this._lastSCPoint!.Value,
            new(segments[0], segments[1]),
            new(segments[2], segments[3])
          );
          break;
        }
        case 'Q':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawQuadraticCurvePoint
          (
            path,
            new XPoint(segments[0], segments[1]),
            new XPoint(segments[2], segments[3])
          );
          break;
        }
        case 'q':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawQuadraticCurvePoint
          (
            path,
            new XVector(segments[0], segments[1]),
            new XVector(segments[2], segments[3])
          );
          break;
        }
        case 'T':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawQuadraticCurvePoint
          (
            path,
            this._lastQPoint!.Value,
            new XPoint(segments[0], segments[1])
          );
          break;
        }
        case 't':
        {
          double[] segments = this.GetPoints(segment.Substring(1));
          this.DrawQuadraticCurvePoint
          (
            path,
            this._lastQPoint!.Value,
            new XVector(segments[0], segments[1])
          );
          break;
        }
        case 'A':
        {
          break;
        }
        case 'a':
        {
          break;
        }
        case 'Z':
        case 'z':
        {
          path.CloseFigure();
          break;
        }
      }
    }
    gfx.DrawPath(pen, new XSolidBrush(this._color), path);
    
  }

  #endregion

  #region Private Methods

  private double[] GetPoints(string numberPartSegment)
  {
    string[] valueStrings = numberPartSegment.Split([',',' ']);
    double[] coordinates = new double[valueStrings.Length];
    
    for (var i = 0; i < valueStrings.Length; i++)
    {
      coordinates[i] = double.Parse(valueStrings[i]);
    }

    return coordinates;
  }

  private double CalculateX(double input) => input * (this._size.Width / this._viewBox.Width);

  private double CalculateY(double input) => input * (this._size.Height / this._viewBox.Height);

  private XPoint GetAbsPoint(XPoint svgPoint) =>
    new
    (
      this._startPoint.X + this.CalculateX(svgPoint.X),
      this._startPoint.Y + this.CalculateY(svgPoint.Y)
    );

  private XPoint GetRelPoint(XPoint svgPoint) =>
    new
    (
      this._movePoint!.Value.X + this.CalculateX(svgPoint.X),
      this._movePoint!.Value.Y + this.CalculateY(svgPoint.Y)
    );

  private void MoveAbs(XPoint movePoint)
  {
    this._movePoint = this.GetAbsPoint(movePoint);
    this._movePointGraph = movePoint;
  }

  private void MoveRel(XVector moveVector)
  {
    this._movePoint = this.GetAbsPoint(this._movePoint!.Value + moveVector);
    this._movePointGraph = this._movePoint!.Value + moveVector;
  }

  private void StraightLineAbs(XGraphicsPath path, XPoint targetPoint)
  {
    XPoint absTargetPoint = this.GetAbsPoint(targetPoint);
    path.AddLine(this._movePoint!.Value, absTargetPoint);
    this._movePoint = absTargetPoint;
    this._movePointGraph = targetPoint;
  }
  
  private void StraightLineRel(XGraphicsPath path, XVector moveVector)
  {
    XPoint relTargetPoint = this.GetAbsPoint(this._movePoint!.Value + moveVector);
    path.AddLine(this._movePoint!.Value, relTargetPoint);
    this._movePoint = relTargetPoint;
    this._movePointGraph = this._movePoint!.Value + moveVector;
  }
  
  private void DrawHorizontalLineAbs(XGraphicsPath path, double targetX)
  {
    XPoint absTargetPoint = new(this._startPoint.X + this.CalculateX(targetX), this._movePoint!.Value.Y);
    path.AddLine(this._movePoint!.Value, absTargetPoint);
    this._movePoint = absTargetPoint;
    this._movePointGraph = this._movePointGraph!.Value with { X = targetX};
  }
  
  private void DrawHorizontalLineRel(XGraphicsPath path, double targetX)
  {
    XPoint relTargetPoint = new(this._movePoint!.Value.X + this.CalculateX(targetX), this._movePoint!.Value.Y);
    path.AddLine(this._movePoint!.Value, relTargetPoint);
    this._movePoint = relTargetPoint;
    this._movePointGraph += new XVector(targetX, 0);
  }
  
  private void DrawVerticalLineAbs(XGraphicsPath path, double targetY)
  {
    XPoint absTargetPoint = new(this._movePoint!.Value.X, this._startPoint.Y + this.CalculateX(targetY));
    path.AddLine(this._movePoint!.Value, absTargetPoint);
    this._movePoint = absTargetPoint;
    this._movePointGraph = this._movePointGraph!.Value with { Y = targetY };
  }
  
  private void DrawVerticalLineRel(XGraphicsPath path, double targetY)
  {
    XPoint relTargetPoint = new(this._movePoint!.Value.X, this._movePoint!.Value.Y + this.CalculateX(targetY));
    path.AddLine(this._movePoint!.Value, relTargetPoint);
    this._movePoint = relTargetPoint;
    this._movePointGraph += new XVector(0, targetY);
  }

  private void DrawCurveAbs(XGraphicsPath path, XPoint p1, XPoint p2, XPoint p3)
  {
    XPoint finalPoint = this.GetAbsPoint(p3);
    path.AddBezier(this._movePoint!.Value, this.GetAbsPoint(p1), this.GetAbsPoint(p2), finalPoint);
    this._movePoint = finalPoint;
    this._movePointGraph = p3;

    this._lastSCPoint = p3 - (p2 - p3);
  }
  
  private void DrawCurveRel(XGraphicsPath path, XVector v1, XVector v2, XVector v3)
  {
    XPoint p0 = this._movePointGraph!.Value;
    XPoint p1 = p0 + v1;
    XPoint p2 = p1 + v2;
    XPoint p3 = p2 + v3;
    XPoint finalPoint = this.GetAbsPoint(p3);
    
    path.AddBezier
    (
      this.GetAbsPoint(p0),
      this.GetAbsPoint(p1),
      this.GetAbsPoint(p2),
      finalPoint
    );
    this._movePoint = finalPoint;
    this._movePointGraph = p3;
    
    this._lastSCPoint = p3 - (p2 - p3);
  }
  
  private void DrawCurveRel(XGraphicsPath path, XPoint p1, XVector v2, XVector v3)
  {
    XPoint p0 = this._movePointGraph!.Value;
    XPoint p2 = p0 + v2;
    XPoint p3 = p2 + v3;
    XPoint finalPoint = this.GetAbsPoint(p3);
    
    path.AddBezier
    (
      this.GetAbsPoint(p0),
      this.GetAbsPoint(p1),
      this.GetAbsPoint(p2),
      finalPoint
    );
    this._movePoint = finalPoint;
    this._movePointGraph = p3;
    
    this._lastSCPoint = p3 - (p2 - p3);
  }

  private void DrawQuadraticCurvePoint(XGraphicsPath path, XPoint p1, XPoint p2)
  {
    XPoint finalPoint = this.GetAbsPoint(p2);
    path.AddBezier
    (
      this._movePoint!.Value,
      this.GetAbsPoint(p1),
      this.GetAbsPoint(p1),
      finalPoint
    );
    
    this._movePoint = finalPoint;
    this._movePointGraph = p2;

    this._lastQPoint = p2 + (p2 - p1);
  }
  
  private void DrawQuadraticCurvePoint(XGraphicsPath path, XVector v1, XVector v2)
  {
    XPoint absP1 = this._movePointGraph!.Value + v1;
    XPoint absP2 = absP1 + v2;
    
    XPoint finalPoint = this.GetAbsPoint(absP2);
    path.AddBezier
    (
      this._movePoint!.Value,
      this.GetAbsPoint(absP1),
      this.GetAbsPoint(absP1),
      finalPoint
    );
    
    this._movePoint = finalPoint;
    this._movePointGraph = absP2;

    this._lastQPoint = absP2 + (absP2 - absP1);
  }
  
  private void DrawQuadraticCurvePoint(XGraphicsPath path, XPoint p1, XVector v2)
  {
    XPoint absP2 = this._movePointGraph!.Value + v2;
    
    XPoint finalPoint = this.GetAbsPoint(absP2);
    path.AddBezier
    (
      this._movePoint!.Value,
      this.GetAbsPoint(p1),
      this.GetAbsPoint(p1),
      finalPoint
    );
    
    this._movePoint = finalPoint;
    this._movePointGraph = absP2;

    this._lastQPoint = absP2 + (absP2 - p1);
  }
  
  #endregion
}