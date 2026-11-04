using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Portrait of the character in the top left corner of the first page; the picture fills the box, cropped to its middle
/// </summary>
public class Portrait: InformationBox
{
  #region Constants

  public const double X = 13;
  public const double Y = 8.6;
  public const double Width = 88;
  public const double Height = 110;

  private const double ImageCorner = 4;

  #endregion

  #region Fields

  private readonly XImage _image;

  #endregion

  #region Constructor

  public Portrait(XImage image) : base(new(X, Y, Width, Height), CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge)
  {
    this._image = image;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double inset = 2;
    XRect area = new(contentRect.X + inset, contentRect.Y + inset, contentRect.Width - inset * 3, contentRect.Height - inset * 3);

    XGraphicsPath clip = new();
    clip.AddRoundedRectangle(area.X, area.Y, area.Width, area.Height, ImageCorner * 2, ImageCorner * 2);

    double pixelWidth = Math.Max(1, this._image.PixelWidth);
    double pixelHeight = Math.Max(1, this._image.PixelHeight);
    double scale = Math.Max(area.Width / pixelWidth, area.Height / pixelHeight);
    double width = pixelWidth * scale;
    double height = pixelHeight * scale;
    XRect target = new(area.X - (width - area.Width) / 2, area.Y - (height - area.Height) / 2, width, height);

    XGraphicsState state = gfx.Save();
    gfx.IntersectClip(clip);
    gfx.DrawImage(this._image, target);
    gfx.Restore(state);
    gfx.DrawPath(new XPen(this.BaseColor, 0.75), clip);
  }

  #endregion
}
