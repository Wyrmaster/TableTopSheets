using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using TableTopSheets.Pdf.Card;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Pdf.Component.Generic;

/// <summary>
///   Printable spell or item card: name, rule, picture with the metadata table, rich description in one or more columns
///   with thin rules between them, and footer, all at the positions of its <see cref="CardGeometry"/>
/// </summary>
public class SpellItemCard: InformationBox
{
  #region Constants

  private const double ImageCorner = 4;
  private const double TableLineSize = 0.4;
  private const double DividerSize = 0.35;

  #endregion

  #region Field

  private readonly XPoint _origin;
  private readonly CardPart _part;
  private readonly XBrush _mutedBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

  #endregion

  #region Constructor

  /// <summary>
  ///   Card <paramref name="part"/> with the top left of its trimmed area at <paramref name="origin"/>
  /// </summary>
  public SpellItemCard(XPoint origin, CardPart part)
    : base(Offset(part.Geometry.Frame, origin), CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge)
  {
    this._origin = origin;
    this._part = part;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    CardGeometry geometry = this._part.Geometry;
    XPen tablePen = new(this.LineColor, TableLineSize);

    TextFit.DrawLine(gfx, geometry.Title, geometry.TitleFont, this.FontBrush, this.At(geometry.TitleRect), XParagraphAlignment.Center);
    if (geometry is { Subtitle: not null, SubtitleFont: not null })
    {
      TextFit.DrawLine(gfx, geometry.Subtitle, geometry.SubtitleFont, this._mutedBrush, this.At(geometry.SubtitleRect), XParagraphAlignment.Center);
    }

    XPoint rule = this.At(geometry.Rule);
    double ruleY = rule.Y + CardGeometry.RuleSize / 2;
    gfx.DrawLine(new XPen(this.FontColor, CardGeometry.RuleSize), rule.X, ruleY, rule.X + geometry.RuleWidth, ruleY);

    if (geometry.ImageRect is { } imageRect && this._part.Image is { } image)
    {
      this.DrawImage(gfx, image, this.At(imageRect));
    }

    geometry.Facts?.Draw(gfx, this.At(geometry.FactsOrigin), this.FontBrush, tablePen);

    XPen dividerPen = new(this.LineColor, DividerSize);
    for (int index = 0; index < geometry.Columns.Count; index++)
    {
      XRect column = this.At(geometry.Columns[index]);
      RichTextSlice slice = this._part.Columns[index];
      slice.Draw(gfx, column.Location, this.FontBrush, tablePen);
      if (index > 0 && slice.Lines.Count > 0)
      {
        // a thin rule in the middle of the gap, as tall as the column
        double x = (this.At(geometry.Columns[index - 1]).Right + column.Left) / 2;
        gfx.DrawLine(dividerPen, x, column.Top, x, column.Bottom);
      }
    }

    XRect footer = this.At(geometry.FooterRect);
    if (!string.IsNullOrWhiteSpace(this._part.Data.Footer))
    {
      TextFit.DrawLine(gfx, this._part.Data.Footer, geometry.FooterFont, this._mutedBrush, footer);
    }

    if (this._part.PartLabel.Length > 0)
    {
      // clear of the slanted corner
      TextFit.DrawLine(gfx, this._part.PartLabel, geometry.FooterFont, this._mutedBrush, footer with { Width = footer.Width - 6 },
        XParagraphAlignment.Right);
    }
  }

  #endregion

  #region Private Methods

  private static XRect Offset(XRect rect, XPoint origin) => rect with { X = rect.X + origin.X, Y = rect.Y + origin.Y };

  private XRect At(XRect rect) => Offset(rect, this._origin);

  private XPoint At(XPoint point) => new(point.X + this._origin.X, point.Y + this._origin.Y);

  /// <summary>
  ///   Fill the rectangle (a square or a banner) with the picture, cropped to its middle, inside rounded corners
  /// </summary>
  private void DrawImage(XGraphics gfx, XImage image, XRect area)
  {
    XGraphicsPath clip = new();
    clip.AddRoundedRectangle(area.X, area.Y, area.Width, area.Height, ImageCorner * 2, ImageCorner * 2);

    double pixelWidth = Math.Max(1, image.PixelWidth);
    double pixelHeight = Math.Max(1, image.PixelHeight);
    double scale = Math.Max(area.Width / pixelWidth, area.Height / pixelHeight);
    double width = pixelWidth * scale;
    double height = pixelHeight * scale;
    XRect target = new(area.X - (width - area.Width) / 2, area.Y - (height - area.Height) / 2, width, height);

    XGraphicsState state = gfx.Save();
    gfx.IntersectClip(clip);
    gfx.DrawImage(image, target);
    gfx.Restore(state);
    gfx.DrawPath(new XPen(this.BaseColor, 0.75), clip);
  }

  #endregion
}
