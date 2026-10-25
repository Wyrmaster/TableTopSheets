using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public abstract class Card: InformationBox
{
  #region Constructor
  
  public Card(XRect rect, CornerEnum tl, CornerEnum tr, CornerEnum br, CornerEnum bl) : base(rect, tl, tr, br, bl)
  {
  }

  #endregion
  
  #region Properties

  public static double HeaderSize { get; set; } = 12d;
  
  public static double BodySize { get; set; } = 10d;

  #endregion
  
  #region Public Methods

  /// <summary>
  ///   Size of a text word wrapped to <paramref name="width"/>
  /// </summary>
  public XSize GetSize(XGraphics gfx, string text, XFont font, double width)
  {
    int lines = TextFit.Wrap(gfx, text, font, width).Count;
    return new XSize()
    {
      Width = width,
      Height = lines * TextFit.LineHeight(font),
    };
  }

  #endregion
}