using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Interface;

/// <summary>
///   Page Interface
/// </summary>
public interface IPage
{
  #region Property

  PdfPage PdfPage { get; }

  #endregion
}