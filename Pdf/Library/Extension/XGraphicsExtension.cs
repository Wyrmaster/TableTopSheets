using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Extension;

public static class XGraphicsExtension
{
  #region Extension Methods

  /// <summary>
  ///   Generate the XGraphics from a page
  /// </summary>
  /// <param name="self"></param>
  /// <returns></returns>
  public static XGraphics CreateGraphics(this PdfPage self) => XGraphics.FromPdfPage(self);

  /// <summary>
  ///   Try to fit a text within a Box
  /// </summary>
  /// <returns></returns>
  public static void DrawText(this XGraphics gfx, string text, XRect rect, XFontStyleEx style = XFontStyleEx.Regular, bool center = false, XParagraphAlignment alignment = XParagraphAlignment.Center)
  {
    double fontsize = 30;
    XSize size;
    do
    {
      fontsize -= 0.1;
      size = gfx.MeasureString(text, new XFont("Roboto#", fontsize, style));
    } while (size.Width > rect.Width || size.Height > rect.Height);
    
    XTextFormatter formatter = new XTextFormatter(gfx);
    formatter.Alignment = alignment;

    if (center)
    {
      formatter.DrawString
      (
        text,
        new XFont("Roboto#", fontsize),
        XBrushes.Black,
        rect with { Y = rect.Y + (rect.Height - size.Height) / 2 },
        XStringFormats.TopLeft
      );
    }
    else
    {
      formatter.DrawString
      (
        text,
        new XFont("Roboto#", fontsize),
        XBrushes.Black,
        rect with { Y = rect.Y + (rect.Height - size.Height)},
        XStringFormats.TopLeft
      );
    }
  }
  
  #endregion
}