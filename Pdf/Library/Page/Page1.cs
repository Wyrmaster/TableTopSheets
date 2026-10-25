using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Page;

public class Page1: Page
{
  #region Constructor

  public Page1(PdfDocument pdfDocument) : base(pdfDocument)
  {
  }

  #endregion

  #region Override
  
  protected override void WithRender(XGraphics gfx)
  {
    throw new NotImplementedException();
  }

  #endregion
}