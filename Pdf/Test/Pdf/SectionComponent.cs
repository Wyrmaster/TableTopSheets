using TableTopSheets.Pdf.Interface;

namespace TableTopSheets.Pdf.Test.Pdf;

public class SectionComponent: Document
{
  #region Field

  private readonly IComponent[][] _components;

  #endregion

  #region Constructor

  public SectionComponent(params IComponent[][] components)
  {
    this._components = components;
  }

  #endregion
  
  #region Override
  
  protected override void Generate()
  {
    foreach (IComponent[] components in this._components)
    {
      new SectionPage(this.PdfDocument, components).Draw();
    }
  }

  #endregion
}