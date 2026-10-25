using TableTopSheets.Pdf.FontResolver;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace TableTopSheets.Pdf;

public abstract class Document
{
  #region Field
  
  protected readonly PdfDocument PdfDocument;

  #endregion

  #region Constructor

  static Document()
  {
    GlobalFontSettings.FontResolver = new RobotoFontResolver();
  }

  protected Document()
  {
    this.PdfDocument = new();
    this.PdfDocument.Language = "EN";
  }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Generate the File into a Stream Object
  /// </summary>
  /// <returns></returns>
  public Stream GenerateCharacterSheetStream()
  {
    this.Generate();

    MemoryStream stream = new();
    PdfDocument.Save(stream);

    return stream;
  }

  /// <summary>
  ///   Generate save file into a specific path
  /// </summary>
  public void GenerateCharacterSheetFile(string path)
  {
    this.Generate();
    PdfDocument.Save(path);
  }

  #endregion

  #region Protected Methods

  /// <summary>
  /// Method to generate content of the PDF Document
  /// </summary>
  protected abstract void Generate();

  #endregion
}