using TableTopSheets.Pdf.Extension;
using TableTopSheets.Pdf.Interface;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Page;

/// <summary>
///   Base Class for a Page
/// </summary>
public abstract class Page: IPage
{
  #region Constructor

  protected Page(PdfDocument pdfDocument)
  {
    this.PdfPage = pdfDocument.AddPage();
    this.PdfPage.Size = PageSize.A4;
  }

  #endregion

  #region Property

  public PdfPage PdfPage { get; protected set; }

  #endregion
  
  #region Public Methods

  /// <summary>
  ///   Draw the content of a Page
  /// </summary>
  public void Draw()
  {
    using XGraphics gfx = this.PdfPage.CreateGraphics();
    this.WithRender(gfx);
    gfx.Save();
  }

  #endregion

  #region Abstract Methods

  /// <summary>
  ///   Override that defines the content of a Page
  /// </summary>
  /// <param name="gfx"><see cref="XGraphics"/> object allowing renderings</param>
  protected abstract void WithRender(XGraphics gfx);

  #endregion
}