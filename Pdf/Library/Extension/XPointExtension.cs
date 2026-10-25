using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Extension;

/// <summary>
///   Extension Methods for Points
/// </summary>
public static class XPointExtension
{
  #region Static Field

  private static readonly XMatrix NoFlip = new(1, 0, 0, 1, 0, 0);
  private static readonly XMatrix XFlip = new(-1, 0, 0, 1, 0, 0);
  private static readonly XMatrix YFlip = new(1, 0, 0, -1, 0, 0);
  private static readonly XMatrix XYFlip = new(-1, 0, 0, -1, 0, 0);

  #endregion
  
  #region Extension Methods
  
  /// <summary>
  ///   Flip the point
  /// </summary>
  /// <param name="self">Point</param>
  /// <param name="flipX">flip vertically</param>
  /// <param name="flipY">flip horizontally</param>
  /// <returns></returns>
  public static XPoint Flip(this XPoint self, bool flipX = false, bool flipY = false)
  {
    XMatrix matrix = NoFlip;
    if (flipX && flipY)
    {
      matrix = XYFlip;
    }
    else if (flipX)
    {
      matrix = XFlip;
    }
    else if (flipY)
    {
      matrix = YFlip;
    }

    return self * matrix;
  }

  #endregion
}