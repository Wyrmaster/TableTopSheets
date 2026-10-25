using System.Numerics;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public class CurveBox: BaseComponent
{
  #region Constants

  public const double Height = 7.55;
  public const double Width = 7.55;

  #endregion

  #region Field
  
  private readonly XMatrix? _translationMatrix;
  private readonly XMatrix _scaleTransformation;
  private readonly bool _fill;

  #endregion

  #region Constructor

  public CurveBox(XRect rect, bool deg45 = false, bool fill = false, double scale = 1d) : base(rect)
  {
    this._fill = fill;
    this._scaleTransformation = new()
    {
      M11 = scale,
      M12 = 0,
      M21 = 0,
      M22 = scale,
      OffsetX = -Width / 4 + 0.33,
    };
    this._translationMatrix = deg45
        ? new()
        {
          M11 = Math.Cos(45 * Math.PI / 180.0),
          M12 = -Math.Sin(45 * Math.PI / 180.0),
          M21 = Math.Sin(45 * Math.PI / 180.0),
          M22 = Math.Cos(45 * Math.PI / 180.0),
          OffsetX = -Width / 4 + 0.33,
        }
        : null
      ;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 0.1;
    XPen thick = new XPen(XColors.Black, outerBorderSize);
    XGraphicsPath path = new();
    
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.517644, 0.0351318)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0641358, 0.507775)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0775306, 0.513516)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0641358, 0.52691)),
    path.AddLines
    (
      [
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.517644, 0.0351318)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0641358, 0.507775)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0775306, 0.513516)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0641358, 0.52691)),
      ]
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.733458, 1.47683)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.85746, 5.5971)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0545682, 6.96596)),
    path.AddBezier
    (
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0641358, 0.52691)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.733458, 1.47683)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.85746, 5.5971)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0545682, 6.96596))
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.527211, 7.41946)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.529125, 7.41563)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.561655, 7.44816)),
    path.AddLines
    (
      [
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.0545682, 6.96596)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.527211, 7.41946)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.529125, 7.41563)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.561655, 7.44816)),
      ]
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.51158, 6.77885)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.63376, 6.65484)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.00262, 7.45774)),
    path.AddBezier
    (
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.561655, 7.44816)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.51158, 6.77885)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.63376, 6.65484)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.00262, 7.45774))
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.45612, 6.98509)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.44273, 6.97935)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.45612, 6.96595)),
    path.AddLines
    (
      [
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.00262, 7.45774)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.45612, 6.98509)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.44273, 6.97935)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.45612, 6.96595)),
      ]
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.7868, 6.01603)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.6628, 1.89577)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.46569, 0.52691)),
    path.AddBezier
    (
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.45612, 6.96595)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.7868, 6.01603)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.6628, 1.89577)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.46569, 0.52691))
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.99305, 0.0734025)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.99114, 0.0772296)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.95861, 0.0446995)),
    path.AddLines
    (
      [
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(7.46569, 0.52691)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.99305, 0.0734025)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.99114, 0.0772296)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.95861, 0.0446995))
      ]
    );
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.00868, 0.714022)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.8865, 0.838024)),
    // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.517644, 0.0351318)),
    path.AddBezier
    (
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.95861, 0.0446995)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.00868, 0.714022)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.8865, 0.838024)),
      new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.517644, 0.0351318))
    );
    path.CloseFigure();

    if (!this._fill)
    {
      path.StartFigure();
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.56824, 0.93832)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5338, 1.01103)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.10464, 2.83792)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.56824, 0.93832)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5338, 1.01103)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.10464, 2.83792)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116)), 
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.54337, 6.53924)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.51658, 6.52584)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.54337, 6.53924)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.51658, 6.52584)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.01516, 6.52202)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.952016, 6.55455)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.01516, 6.52202)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.952016, 6.55455)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.31836, 2.34171)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.990287, 0.980418)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.31836, 2.34171)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.990287, 0.980418)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.83057, 1.39618)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.1438, 1.29892)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5051, 0.970851)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.83057, 1.39618)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.1438, 1.29892)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5051, 0.970851))
      );
      
      path.CloseFigure();
    }
    
    
    gfx.DrawPath(XBrushes.Black, path);

    if (!this._fill)
    {
      path = new();
    
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.56824, 0.93832)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5338, 1.01103)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.10464, 2.83792)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.56824, 0.93832)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5338, 1.01103)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.10464, 2.83792)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.2019, 5.15116)), 
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.54337, 6.53924)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.51658, 6.52584)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.52997, 6.51245)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.54337, 6.53924)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.51658, 6.52584)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(4.68969, 6.09669)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.01516, 6.52202)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.952016, 6.55455)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.37645, 6.19394)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.01516, 6.52202)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.952016, 6.55455)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.98646, 6.48183)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.31836, 2.34171)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.990287, 0.980418)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.41561, 4.65495)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.31836, 2.34171)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.990287, 0.980418)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023)),
      path.AddLine
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(0.976892, 0.953629)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023))
      );
      
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.83057, 1.39618)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.1438, 1.29892)),
      // new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5051, 0.970851)),
      path.AddBezier
      (
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(1.00368, 0.967023)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(2.83057, 1.39618)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(5.1438, 1.29892)),
        new XVector(this.Rect.X, this.Rect.Y) + this.TransformPoint(new XPoint(6.5051, 0.970851))
      );
      
      path.CloseFigure();
      gfx.DrawPath(XBrushes.White, path);
    }
  }
  
  #endregion

  #region Private Method

  private XPoint TransformPoint(XPoint point)
  {
    if (this._translationMatrix == null)
    {
      return point * this._scaleTransformation;
    }

    return point * this._translationMatrix.Value * this._scaleTransformation;
  }

  #endregion
}