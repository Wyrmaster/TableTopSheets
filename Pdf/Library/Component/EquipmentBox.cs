using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class EquipmentBox: InformationBox
{
  #region Fields

  private readonly ArmorEnum _armorProficiency;
  private readonly string _weapons;
  private readonly string _tools;

  #endregion

  #region Constants

  private const double X = 13;
  private const double Y = 669;
  private const double Width = 222;
  private const double Height = 160.5;

  #endregion
  
  #region Constructor
  
  public EquipmentBox(ArmorEnum armorProficiency,string weapons, string tools) : base(new XRect(X, Y, Width, Height), CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Curve)
  {
    this._armorProficiency = armorProficiency;
    this._weapons = weapons;
    this._tools = tools;
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

    const string boxTitle = "EQUIPMENT TRAINING & PROFICIENCIES";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );

    #endregion

    #region Armor
    
    const double armorTrainingWidth = 45, armorTrainingHeight = 25;
    XRect armorTrainingRect = new
    (
      new XPoint(contentRect.X + 2, contentRect.Y + size.Height + 6),
      new XPoint(contentRect.X + armorTrainingWidth, contentRect.Y + size.Height + 4 + armorTrainingHeight)
    );
    
    const string armorTrainingTitle = "ARMOR TRAINING";
    XTextFormatter armorTrainingFormatter = new(gfx);
    armorTrainingFormatter.Alignment = XParagraphAlignment.Left;
    armorTrainingFormatter.DrawString
    (
      armorTrainingTitle,
      new XFont("Roboto#", 8, XFontStyleEx.Bold),
      this.FontBrush,
      armorTrainingRect
    );
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 4 + armorTrainingHeight + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 4 + armorTrainingHeight + 2)
    );

    XFont armorProficiencyFont = new("Roboto#", 7.5, XFontStyleEx.Bold);
    XRect lightRect = new
    (
      armorTrainingRect.X + armorTrainingRect.Width + 2,
      contentRect.Y + size.Height + 3.5 + armorTrainingHeight / 2,
      CurveBox.Height,
      CurveBox.Width
    );
    const string lightArmorString = "Light",
      mediumArmorString = "Medium",
      heavyArmorString = "Heavy",
      shieldArmorString = "Shields";
    size = gfx.MeasureString(lightArmorString, armorProficiencyFont);
    gfx.DrawString
    (
      lightArmorString,
      armorProficiencyFont,
      this.FontBrush,
      lightRect.X + lightRect.Width + 3,
      lightRect.Y + size.Height / 2 - 2.25
    );
    XRect mediumRect = lightRect with { X = lightRect.X + lightRect.Width + lightRect.Width + 3 + size.Width};
    size = gfx.MeasureString(mediumArmorString, armorProficiencyFont);
    gfx.DrawString
    (
      mediumArmorString,
      armorProficiencyFont,
      this.FontBrush,
      mediumRect.X + mediumRect.Width + 3,
      mediumRect.Y + size.Height / 2 - 2.25
    );
    XRect heavyRect = mediumRect with { X = mediumRect.X + mediumRect.Width + CurveBox.Width + 3 + size.Width};
    size = gfx.MeasureString(heavyArmorString, armorProficiencyFont);
    gfx.DrawString
    (
      heavyArmorString,
      armorProficiencyFont,
      this.FontBrush,
      heavyRect.X + heavyRect.Width + 3,
      heavyRect.Y + size.Height / 2 - 2.25
    );
    XRect shieldRect = heavyRect with { X = heavyRect.X + heavyRect.Width + CurveBox.Width + 3 + size.Width};
    size = gfx.MeasureString(shieldArmorString, armorProficiencyFont);
    gfx.DrawString
    (
      shieldArmorString,
      armorProficiencyFont,
      this.FontBrush,
      shieldRect.X + shieldRect.Width + 3,
      shieldRect.Y + size.Height / 2 - 2.25
    );
    
    new CurveBox(lightRect, deg45: true, fill: this._armorProficiency.HasFlag(ArmorEnum.Light)).Draw(gfx);
    new CurveBox(mediumRect, deg45: true, fill: this._armorProficiency.HasFlag(ArmorEnum.Medium)).Draw(gfx);
    new CurveBox(heavyRect, deg45: true, fill: this._armorProficiency.HasFlag(ArmorEnum.Heavy)).Draw(gfx);
    new CurveBox(shieldRect, deg45: true, fill: this._armorProficiency.HasFlag(ArmorEnum.Shield)).Draw(gfx);

    #endregion

    double lowerHeight = contentRect.Height - armorTrainingHeight - 17.75;
    double weaponHeight = lowerHeight / 7 * 4;
    double toolHeight = lowerHeight / 7 * 3;
    XRect lowerRect = contentRect with { Height = lowerHeight, Y = contentRect.Y + contentRect.Height - lowerHeight };
    XRect weaponRect = lowerRect with { Height = weaponHeight };
    XRect toolRect = lowerRect with { Height = toolHeight, Y = lowerRect.Y + weaponHeight };

    XFont titleFont = new("Roboto#", 8, XFontStyleEx.Bold);
    
    #region Weapons

    const string weaponString = "WEAPONS";
    size = gfx.MeasureString(weaponString, titleFont);
    
    gfx.DrawString
    (
      weaponString,
      titleFont,
      this.FontBrush,
      contentRect.X + 2,
      weaponRect.Y - 1 + size.Height
    );
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, weaponRect.Y + weaponRect.Height),
      new(contentRect.X + contentRect.Width - 4, weaponRect.Y + weaponRect.Height)
    );

    XRect weaponContentRect = new
    (
      weaponRect.X + 2,
      weaponRect.Y + size.Height,
      weaponRect.Width - 6,
      weaponRect.Height - size.Height - 2
    );

    XTextFormatter weaponFormatter = new(gfx);
    weaponFormatter.Alignment = XParagraphAlignment.Left;
    weaponFormatter.DrawString
    (
      this._weapons,
      new XFont("Roboto#", 8, XFontStyleEx.Bold),
      this.FontBrush,
      weaponContentRect
    );

    #endregion

    #region Tools

    const string toolsString = "TOOLS";
    size = gfx.MeasureString(toolsString, titleFont);
    
    gfx.DrawString
    (
      toolsString,
      titleFont,
      this.FontBrush,
      contentRect.X + 2,
      toolRect.Y - 1 + size.Height
    );

    XRect toolContentRect = new
    (
      toolRect.X + 2 + 10,
      toolRect.Y + size.Height,
      toolRect.Width - 6 - 10,
      toolRect.Height - size.Height - 2
    );
    
    XTextFormatter toolFormatter = new(gfx);
    toolFormatter.Alignment = XParagraphAlignment.Left;
    toolFormatter.DrawString
    (
      this._tools,
      new XFont("Roboto#", 8, XFontStyleEx.Bold),
      this.FontBrush,
      toolContentRect
    );

    #endregion
  }

  #endregion
}