using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class SpellSlots: InformationBox
{
  #region Constants

  private const double X = 152;
  private const double Y = 63.45;
  private const double Width = 243.3377;
  private const double Height = 78;

  #endregion

  #region Field

  private readonly int _level1Slots;
  private readonly int _level2Slots;
  private readonly int _level3Slots;
  private readonly int _level4Slots;
  private readonly int _level5Slots;
  private readonly int _level6Slots;
  private readonly int _level7Slots;
  private readonly int _level8Slots;
  private readonly int _level9Slots;

  #endregion

  #region Constructor
  
  public SpellSlots(SpellSlot[] spellSlots) : base(new(X,Y,Width,Height), CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Slant, CornerEnum.Edge)
  {
    this._level1Slots = spellSlots.Count(entry => entry.Level == 1);
    this._level2Slots = spellSlots.Count(entry => entry.Level == 2);
    this._level3Slots = spellSlots.Count(entry => entry.Level == 3);
    this._level4Slots = spellSlots.Count(entry => entry.Level == 4);
    this._level5Slots = spellSlots.Count(entry => entry.Level == 5);
    this._level6Slots = spellSlots.Count(entry => entry.Level == 6);
    this._level7Slots = spellSlots.Count(entry => entry.Level == 7);
    this._level8Slots = spellSlots.Count(entry => entry.Level == 8);
    this._level9Slots = spellSlots.Count(entry => entry.Level == 9);
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

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);

    #region Header

    const string boxTitle = "SPELL SLOTS";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    #endregion

    XRect slotInformationRect = new
    (
      new XPoint(contentRect.X + 3, contentRect.Y + size.Height + 3),
      new XPoint(contentRect.X + contentRect.Width - 5, contentRect.Y + contentRect.Height - 3)
    );
    double colWidth = slotInformationRect.Width / 3;
    colWidth = (slotInformationRect.Width - colWidth * 0.25) / 3;
    double padding = 3;
    
    gfx.DrawLine
    (
      vLine,
      new(slotInformationRect.X + colWidth * 1.125, slotInformationRect.Y + padding * 2.5),
      new(slotInformationRect.X + colWidth * 1.125, slotInformationRect.Y + slotInformationRect.Height - padding)
    );
    gfx.DrawLine
    (
      vLine,
      new(slotInformationRect.X + colWidth * 2.25, slotInformationRect.Y + padding * 2.5),
      new(slotInformationRect.X + colWidth * 2.25 , slotInformationRect.Y + slotInformationRect.Height - padding)
    );

    #region SpellSlots

    double slotPadding = 1.5;
    double verticalOffset = 18;
    double scale = 0.85;
    double rowHeight = 15;
    
    const string totalString = "Total";
    const string expendedString = "Expended";
    XFont headersFont = new("Roboto#", 7);
    size = gfx.MeasureString(expendedString, headersFont);
    XBrush headerBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    XPen underscore= new(XColor.FromArgb(179, 180, 182), 0.2);
    
    double col2 = 3, col3 = 1.5;
    
    gfx.DrawString
    (
      expendedString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 0.625 - 2.5,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 0.625 - 2.5,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 0.625 + 4,
      slotInformationRect.Y + size.Height + 1
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 0.625 + 6,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 0.625 - 2.5 + size.Width,
      slotInformationRect.Y + size.Height + 1
    );
    
    gfx.DrawString
    (
      expendedString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 1.875 - 2.5 - col2,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 1.875 - 2.5 - col2,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 1.875 + 4 - col2,
      slotInformationRect.Y + size.Height + 1
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 1.875 + 6 - col2,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 1.875 - 2.5 + size.Width - col2,
      slotInformationRect.Y + size.Height + 1
    );
    
    gfx.DrawString
    (
      expendedString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 2.875 - 2.5 + 2,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 2.875 - 2.5 + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 2.875 + 4 + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 2.875 + 6 + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 2.875 - 2.5 + size.Width + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1
    );
    
    size = gfx.MeasureString(totalString, headersFont);
    gfx.DrawString
    (
      totalString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 0.625 - 5 - size.Width,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 0.625 - 5 - size.Width,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 0.625 - 5,
      slotInformationRect.Y + size.Height + 1
    );
    
    gfx.DrawString
    (
      totalString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 1.875 - 5 - size.Width - col2,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 1.875 - 5 - size.Width - col2,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 1.875 - 5 - col2,
      slotInformationRect.Y + size.Height + 1
    );
    
    gfx.DrawString
    (
      totalString,
      headersFont,
      headerBrush, 
      slotInformationRect.X + colWidth * 2.875 - 5 - size.Width + 2,
      slotInformationRect.Y + size.Height
    );
    gfx.DrawLine
    (
      underscore,
      slotInformationRect.X + colWidth * 2.875 - 5 - size.Width + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1,
      slotInformationRect.X + colWidth * 2.875 - 5 + col3 * 1.5,
      slotInformationRect.Y + size.Height + 1
    );
    
    string level;
    XPen linePen = new(XColor.FromArgb(179, 180, 182), 0.4d);

    XTextFormatter value = new XTextFormatter(gfx);
    value.Alignment = XParagraphAlignment.Center;
    XRect valueRect;
      
    #region Level 1

    valueRect = new
    (
      slotInformationRect.X + colWidth * 0.625 - 17,
      slotInformationRect.Y + verticalOffset - rowHeight / 2 + 2,
      12,
      11
    );
    value.DrawString($"{this._level1Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 0.625 - 17, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2),
      new (slotInformationRect.X + colWidth * 0.625 - 5, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2)
    );
    
    level = "LEVEL 1";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X, slotInformationRect.Y + verticalOffset + size.Height / 4);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.625,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.75,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.875,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 1,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    
    

    #endregion
    
    #region Level 2
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 0.625 - 17,
      slotInformationRect.Y + verticalOffset + rowHeight * 0.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level2Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 0.625 - 17, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2),
      new (slotInformationRect.X + colWidth * 0.625 - 5, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2)
    );
    
    level = "LEVEL 2";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.625,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.75,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.875,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 3
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 0.625 - 17,
      slotInformationRect.Y + verticalOffset + rowHeight * 1.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level3Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 0.625 - 17, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2),
      new (slotInformationRect.X + colWidth * 0.625 - 5, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2)
    );
    
    level = "LEVEL 3";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight * 2);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.625,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.75,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 0.875,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 4
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 1.875 - 17 - col2,
      slotInformationRect.Y + verticalOffset - rowHeight / 2 + 2,
      12,
      11
    );
    value.DrawString($"{this._level4Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 1.875 - 17 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2),
      new (slotInformationRect.X + colWidth * 1.875 - 5 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2)
    );
    
    level = "LEVEL 4";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 1.125 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 1.875 - col2,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 2 - col2,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 2.125 - col2,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 5
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 1.875 - 17 - col2,
      slotInformationRect.Y + verticalOffset + rowHeight * 0.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level5Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 1.875 - 17 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2),
      new (slotInformationRect.X + colWidth * 1.875 - 5 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2)
    );
    
    level = "LEVEL 5";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 1.125 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 1.875 - col2,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 2 - col2,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 2.125 - col2,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 6
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 1.875 - 17 - col2,
      slotInformationRect.Y + verticalOffset + rowHeight * 1.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level6Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 1.875 - 17 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2),
      new (slotInformationRect.X + colWidth * 1.875 - 5 - col2, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2)
    );
    
    level = "LEVEL 6";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 1.125 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight * 2);

    new CurveBox(new(
        slotInformationRect.X + colWidth * 1.875 - col2,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 2 - col2,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion

    #region Level 7
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 3 - 17 - col2 - col3,
      slotInformationRect.Y + verticalOffset - rowHeight * 0.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level7Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 3 - 17 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2),
      new (slotInformationRect.X + colWidth * 3 - 5 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 0.5 - 2)
    );
    
    level = "LEVEL 7";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 2.25 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4);
    
    new CurveBox(new(
        slotInformationRect.X + colWidth * 3 - col2 - col3,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);
    new CurveBox(new(
        slotInformationRect.X + colWidth * 3.125 - col2 - col3,
        slotInformationRect.Y + verticalOffset,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 8
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 3 - 17 - col2 - col3,
      slotInformationRect.Y + verticalOffset + rowHeight * 0.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level8Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 3 - 17 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2),
      new (slotInformationRect.X + colWidth * 3 - 5 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 1.5 - 2)
    );
    
    level = "LEVEL 8";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 2.25 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight);

    new CurveBox(new(
        slotInformationRect.X + colWidth * 3 - col2 - col3,
        slotInformationRect.Y + verticalOffset + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #region Level 9
    
    valueRect = new
    (
      slotInformationRect.X + colWidth * 3 - 17 - col2 - col3,
      slotInformationRect.Y + verticalOffset + rowHeight * 1.5 + 2,
      12,
      11
    );
    value.DrawString($"{this._level9Slots}", headersFont, this.FontBrush, valueRect);
    
    gfx.DrawLine
    (
      linePen,
      new (slotInformationRect.X + colWidth * 3 - 17 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2),
      new (slotInformationRect.X + colWidth * 3 - 5 - col2 - col3, slotInformationRect.Y + verticalOffset + rowHeight * 2.5 - 2)
    );
    
    level = "LEVEL 9";
    size = gfx.MeasureString(level, headersFont);
    gfx.DrawString(level, headersFont, this.FontBrush, slotInformationRect.X + colWidth * 2.25 + 5, slotInformationRect.Y + verticalOffset + size.Height / 4 + rowHeight * 2);

    new CurveBox(new(
        slotInformationRect.X + colWidth * 3 - col2 - col3,
        slotInformationRect.Y + verticalOffset + rowHeight + rowHeight,
        CurveBox.Width,
        CurveBox.Height
      ), true, scale: scale)
      .Draw(gfx);

    #endregion
    
    #endregion

  }

  #endregion
}