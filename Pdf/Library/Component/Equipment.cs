using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component;

public class Equipment: InformationBox
{
  #region Fields
  
  private readonly string _equipment;
  private readonly string? _attuned1;
  private readonly string? _attuned2;
  private readonly string? _attuned3;

  #endregion

  #region Constants

  private const double X = 405.0396;
  private const double Y = 437.93736;
  private const double Width = 178.971;
  private const double Height = 312.58761;

  #endregion

  #region Constructor
  
  public Equipment(string equipment, string? attuned1 = null, string? attuned2 = null, string? attuned3 = null) : base(new(X,Y,Width,Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._equipment = equipment;
    this._attuned1 = attuned1;
    this._attuned2 = attuned2;
    this._attuned3 = attuned3;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double columnSize = 0.3;
    const double underlineSize = 0.2;
    
    XPen line = new(this.FontColor, lineSize);
    XColor columnColor = XColor.FromArgb(156, 157, 161);
    XPen vLine = new(columnColor, lineSize * 0.75);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));

    XFont textFont = new("Roboto#", 8, XFontStyleEx.Regular);
    XFont textTitleFont = new("Roboto#", 8, XFontStyleEx.Bold);

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    
    const string boxTitle = "EQUIPMENT";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    const double equipmentContent = 0.7d;
    XRect dataRect = new XRect
    (
      contentRect.X + 4,
      contentRect.Y + size.Height + 6,
      contentRect.Width - 8,
      contentRect.Height * equipmentContent - size.Height - 10
    );
    
    XTextFormatter tf = new(gfx);
    tf.Alignment = XParagraphAlignment.Left;
    tf.DrawString
    (
      this._equipment,
      textFont,
      this.FontBrush,
      dataRect
    );
    
    dataRect = dataRect with { Y = contentRect.Y + contentRect.Height * equipmentContent, Height = 20};
    
    gfx.DrawString("Magic Item Attunement", textTitleFont, this.FontBrush, new XPoint(dataRect.X, dataRect.Y + 15));
    
    dataRect = dataRect with { Y = contentRect.Y + contentRect.Height * equipmentContent, Height = 20};
    
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 28, 20, 20), deg45: true, fill: this._attuned1 != null).Draw(gfx);
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 48, 20, 20), deg45: true, fill: this._attuned1 != null).Draw(gfx);
    new CurveBox(new XRect(dataRect.X + 4, dataRect.Y + 68, 20, 20), deg45: true, fill: this._attuned1 != null).Draw(gfx);
    
    dataRect = dataRect with { X = dataRect.X + 16, Y = dataRect.Y + 20, Height = 15, Width = dataRect.Width - 18 };
    if (this._attuned1 != null)
    {
      gfx.DrawText(this._attuned1, dataRect, alignment: XParagraphAlignment.Left);
    }
    
    dataRect = dataRect with { Y = dataRect.Y + 20};
    if (this._attuned2 != null)
    {
      gfx.DrawText(this._attuned2, dataRect, alignment: XParagraphAlignment.Left);
    }
    
    dataRect = dataRect with { Y = dataRect.Y + 20};
    if (this._attuned3 != null)
    {
      gfx.DrawText(this._attuned3, dataRect, alignment: XParagraphAlignment.Left);
    }
  }

  #endregion
}