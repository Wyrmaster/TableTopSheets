using TableTopSheets.Pdf.Enum;
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

  public XSize GetSize(XGraphics gfx, string text, XFont font, double width)
  {
    XSize size = gfx.MeasureString(text, font);
    return new XSize()
    {
      Width = width,
      Height = size.Width / width * size.Height,
    };
  }

  #endregion
}