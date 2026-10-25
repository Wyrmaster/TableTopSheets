using System.Reflection;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component;

public class SecondPageLogo: BaseComponent
{
  #region Constants
  
  private const double X = 150;
  private const double Y = 12;
  private const double Width = 245.3377;
  private const double Height = 43;

  #endregion
  
  #region Constructor
  
  public SecondPageLogo() : base(new(X, Y, Width, Height))
  {
  }

  #endregion

  #region Override
  
  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    const double lineSize = 0.4;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    
    XPen line = new XPen(LineColor, lineSize);

    XGraphicsPath path;
    #region Curlys

    path = new();
    
    path.AddBezier
    (
      new XPoint(7.24231, 0.314883).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(7.24231, 15.8229).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(3.30627, 28.2608).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(0.393604, 30.7011).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );

    const double logoHeight = 42.8241;
    path.AddBezier
    (
      new XPoint(0.393604, 30.7011).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(4.88069, 32.8266).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(7.24231, 38.0221).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(7.24231, logoHeight).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    
    path.AddLine
    (
      new XPoint(7.24231, logoHeight).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(1.22461, logoHeight).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    
    
    path.AddBezier
    (
      new XPoint(1.22461, logoHeight).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(12.794, 38.0615).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(15.7427, 2.08706).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(16.0689, 0.255843).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, path);

    path = new();
    
    path.AddBezier
    (
      new XPoint(7.24231, 0.314883).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(7.24231, 15.8229).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(3.30627, 28.2608).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(0.393604, 30.7011).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    path.AddBezier
    (
      new XPoint(0.393604, 30.7011).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(4.88069, 32.8266).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(7.24231, 38.0221).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(7.24231, logoHeight).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    path.AddLine
    (
      new XPoint(7.24231, logoHeight).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(1.22461, logoHeight).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    path.AddBezier
    (
      new XPoint(1.22461, logoHeight).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(12.794, 38.0615).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(15.7427, 2.08706).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(16.0689, 0.255843).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    gfx.DrawPath(thin, path);
    
    gfx.DrawLine
    (
      thin,
      new XPoint(this.Rect.X, this.Rect.Y + logoHeight),
      new XPoint(this.Rect.X + this.Rect.Width, this.Rect.Y + logoHeight)
    );

    #endregion
    
    #region Images
    
    XImage image1 = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageItem1.png")!);
    XImage image2 = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageItem2.png")!);
    XImage image3 = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageItem3.png")!);
    XImage image4 = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageItem4.png")!);
    XImage image5 = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageItem5.png")!);
    XImage logo = XImage.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TableTopSheets.Pdf.Resources.SecondPageLogo.png")!);

    void DrawImage(XImage image, double xOffset, double scaleY = 1)
    {
      double size = (logoHeight + 3) * scaleY;
      double imageScale = image.PixelWidth / (double)image.PixelHeight;
      gfx.DrawImage
      (
        image,
        new XRect
        (
          this.Rect.X + xOffset,
          this.Rect.Y - 3,
          size * imageScale,
          size
        )
      );
    }
    
    DrawImage(image1, -8);
    DrawImage(image2, 34);
    DrawImage(image3, 84);
    DrawImage(image4, 170);
    
    double size = (logoHeight + 3) * 0.5;
    double imageScale = image5.PixelWidth / (double)image5.PixelHeight;
    gfx.DrawImage
    (
      image5,
      new XRect
      (
        this.Rect.X + 205,
        this.Rect.Y - 1.5 + this.Rect.Height * 0.5,
        size * imageScale,
        size
      )
    );

    size = logoHeight / 2.3;
    imageScale = logo.PixelWidth / (double)logo.PixelHeight;
    gfx.DrawImage
    (
      logo,
      new XRect
      (
        this.Rect.X + 137,
        this.Rect.Y + 5,
        size * imageScale,
        size
      )
    );
    
    gfx.DrawLine
    (
      thin,
      new(this.Rect.X, this.Rect.Y),
      new(this.Rect.X + 25, this.Rect.Y)
    );
    
    gfx.DrawLine
    (
      thin,
      new(this.Rect.X + 35, this.Rect.Y),
      new(this.Rect.X + 67, this.Rect.Y)
    );
    
    gfx.DrawLine
    (
      thin,
      new(this.Rect.X + 100, this.Rect.Y),
      new(this.Rect.X + 120, this.Rect.Y)
    );
    
    gfx.DrawLine
    (
      thin,
      new(this.Rect.X + 135, this.Rect.Y),
      new(this.Rect.X + 182, this.Rect.Y)
    );
    
    gfx.DrawLine
    (
      thin,
      new(this.Rect.X + 198, this.Rect.Y),
      new(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );
    
    
    #endregion
  }

  #endregion
}