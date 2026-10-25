using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using TableTopSheets.Pdf.Component;
using TableTopSheets.Pdf.Interface;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf.Test.Pdf;

public class SectionPage: Page.Page
{
  #region Field

  private readonly IComponent[] _components;

  #endregion

  #region Constructor
  
  public SectionPage(PdfDocument pdfDocument, params IComponent[] components) : base(pdfDocument)
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