using PdfSharp.Drawing;

namespace TableTopSheets.Pdf;

/// <summary>
///   Pictures from bytes stored in the database
/// </summary>
public static class Images
{
  #region Public Methods

  /// <summary>
  ///   The picture, or null when there are no bytes or PDFsharp can't read them (WebP for example)
  /// </summary>
  public static XImage? Load(byte[]? bytes)
  {
    if (bytes is not { Length: > 0 })
    {
      return null;
    }

    try
    {
      // PDFsharp reads the buffer of the stream directly
      return XImage.FromStream(new MemoryStream(bytes, 0, bytes.Length, writable: false, publiclyVisible: true));
    }
    catch (Exception)
    {
      return null;
    }
  }

  #endregion
}
