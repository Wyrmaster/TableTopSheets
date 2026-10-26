using System.Reflection;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public class Logo: BaseComponent
{
  #region Constants
  
  private const double X = 13;
  private const double Y = 100;
  private const double Width = 569;
  private const double Height = 28.64;

  #endregion
  
  #region Constructor
  
  public Logo() : base(new XRect(X, Y, Width, Height))
  {
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.7;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);

    const double radius = Height / 2;
    
    XImage logo = XImage.FromStream(Assembly
      .GetExecutingAssembly()
      .GetManifestResourceStream("TableTopSheets.Pdf.Resources.SheetLogo.png")!);

    double scale = (Height / 1.85 ) / logo.PixelHeight;
    gfx.DrawImage(
      logo, 
      new XRect
      (
        X + (Width / 2.0) - (logo.PixelWidth / 2.0 * scale),
        Y + (Height / 2.0) - (logo.PixelHeight / 2.0 * scale) - 8,
        logo.PixelWidth * scale,
        logo.PixelHeight * scale
      )
    );
    
    const double padding = 5.0;
    double patternWidth = (Width - (logo.PixelWidth * scale) - radius - padding) / 2.0;

    const double sideArcScale = 8;
    
    #region Left Pattern

    XGraphicsPath path = new();

    path.AddLine
    (
      new XPoint(5.98278, 8.18696).Flip() + new XVector(X, Y),
      new XPoint(103.911, 8.18696).Flip() + new XVector(X, Y)
    );
    path.AddBeziers
    (
      [
        new XPoint(103.911, 8.18696).Flip() + new XVector(X, Y),
        new XPoint(105.449, 10.0239).Flip() + new XVector(X, Y),
        new XPoint(105.683, 12.2057).Flip() + new XVector(X, Y),
        new XPoint(108.675, 12.2057).Flip() + new XVector(X, Y),
        new XPoint(108.675, 12.2057).Flip() + new XVector(X, Y),
        new XPoint(140.651, 12.2).Flip() + new XVector(X, Y),
        new XPoint(156.638, 12.2057).Flip() + new XVector(X, Y),
        new XPoint(175.527, 12.2124).Flip() + new XVector(X, Y),
        new XPoint(213.304, 12.2461).Flip() + new XVector(X, Y),
        new XPoint(213.304, 12.2461).Flip() + new XVector(X, Y)
      ]
    );
    
    path.AddLines
    (
      [
        new XPoint(213.304, 12.2461).Flip() + new XVector(X, Y),
        new XPoint(213.34, 3.479).Flip() + new XVector(X, Y),
        new XPoint(213.249, 3.479).Flip() + new XVector(X, Y)
      ]
    );

    path.AddBezier
    (
      new XPoint(213.249, 3.479).Flip() + new XVector(X, Y),
      new XPoint(212.386, 6.84667).Flip() + new XVector(X, Y),
      new XPoint(208.768, 12.1626).Flip() + new XVector(X, Y),
      new XPoint(205.845, 12.2182).Flip() + new XVector(X, Y)
    );
    gfx.DrawPath(thin, path);
    
    gfx.DrawLine
    (
      thick,
      new XPoint(5.98278, 12.2461).Flip() + new XVector(X, Y),
      new XPoint(213.65, 12.2461).Flip() + new XVector(X, Y)
    );
    
    path = new();
    path.AddArc
    (
      X - 9,
      Y,
      18,
      18,
      -90,
      180
    );
    path.CloseFigure();
    gfx.DrawPath(thin, XBrushes.White, path);
    
    gfx.DrawBezier
    (
      thin,
      new XPoint(20.5122, 8.18261).Flip() + new XVector(X, Y),
      new XPoint(19.2319, 8.98974).Flip() + new XVector(X, Y),
      new XPoint(12.4687, 9.57421).Flip() + new XVector(X, Y),
      new XPoint(12.4687, 12.0234).Flip() + new XVector(X, Y)
    );
    
    #endregion
    
    #region Right Pattern
    
    path = new();

    path.AddLine
    (
      new XPoint(5.98278, 8.18696).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(103.911, 8.18696).Flip(flipX: true) + new XVector(X + Width, Y)
    );
    path.AddBeziers
    (
      [
        new XPoint(103.911, 8.18696).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(105.449, 10.0239).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(105.683, 12.2057).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(108.675, 12.2057).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(108.675, 12.2057).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(140.651, 12.2).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(156.638, 12.2057).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(175.527, 12.2124).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(213.304, 12.2461).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(213.304, 12.2461).Flip(flipX: true) + new XVector(X + Width, Y)
      ]
    );
    
    path.AddLines
    (
      [
        new XPoint(213.304, 12.2461).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(213.34, 3.479).Flip(flipX: true) + new XVector(X + Width, Y),
        new XPoint(213.249, 3.479).Flip(flipX: true) + new XVector(X + Width, Y)
      ]
    );

    path.AddBezier
    (
      new XPoint(213.249, 3.479).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(212.386, 6.84667).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(208.768, 12.1626).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(205.845, 12.2182).Flip(flipX: true) + new XVector(X + Width, Y)
    );
    gfx.DrawPath(thin, path);
    
    gfx.DrawLine
    (
      thick,
      new XPoint(5.98278, 12.2461).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(213.65, 12.2461).Flip(flipX: true) + new XVector(X + Width, Y)
    );
    
    path = new();
    path.AddArc
    (
      X + Width - 9,
      Y,
      18,
      18,
      90,
      180
    );
    path.CloseFigure();
    gfx.DrawPath(thin, XBrushes.White, path);
    
    gfx.DrawBezier
    (
      thin,
      new XPoint(20.5122, 8.18261).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(19.2319, 8.98974).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(12.4687, 9.57421).Flip(flipX: true) + new XVector(X + Width, Y),
      new XPoint(12.4687, 12.0234).Flip(flipX: true) + new XVector(X + Width, Y)
    );
    
    #endregion
  }

  #endregion
}