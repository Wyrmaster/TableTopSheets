using PdfSharp.Drawing;
using PdfSharp.Pdf;
using TableTopSheets.Pdf.Interface;

namespace TableTopSheets.Pdf.Page;

/// <summary>
///   Page that draws a fixed set of components
/// </summary>
public class ComponentPage: Page
{
  #region Field

  private readonly IComponent[] _components;

  #endregion

  #region Constructor

  public ComponentPage(PdfDocument pdfDocument, params IComponent[] components) : base(pdfDocument)
  {
    this._components = components;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    foreach (IComponent component in this._components)
    {
      component.Draw(gfx);
    }
  }

  #endregion
}
