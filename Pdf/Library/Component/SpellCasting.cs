using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class SpellCasting: InformationBox
{
  #region Constants

  private const double X = 11.61741;
  private const double Y = 12.54043;
  private const double Width = 129.883;
  private const double Height = 128.8;

  #endregion

  #region Field

  private readonly string _spellCasting;
  private readonly string _spellCastingModifier;
  private readonly string _spellSaveDc;
  private readonly string _spellAttackBonus;

  #endregion

  #region Constructor
  
  public SpellCasting(string spellCasting, string spellCastingModifier, string spellSaveDc, string spellAttackBonus) : base(new(X,Y,Width,Height), CornerEnum.Curve, CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Edge)
  {
    this._spellCasting = spellCasting;
    this._spellCastingModifier = spellCastingModifier;
    this._spellSaveDc = spellSaveDc;
    this._spellAttackBonus = spellAttackBonus;
  }

  #endregion

  #region Override
  
  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    const double lineSize = 0.4;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    XPen line = new XPen(LineColor, lineSize);

    XFont titles = new("Roboto#", 7.5, XFontStyleEx.Bold);
    XFont subTitle = new("Roboto#", 6);
    
    #region Curve

    XGraphicsPath path = new();
    
    path.AddBezier
    (
      new XPoint(2.8179901, 10.450620999999998) + new XVector(X,Y),
      new XPoint(3.015511, 19.342112) + new XVector(X,Y),
      new XPoint(5.5638004, 25.053367) + new XVector(X,Y),
      new XPoint(10.521609, 28.956535000000002) + new XVector(X,Y)
    );
  
    path.AddBezier
    (
      new XPoint(10.521609, 28.956535000000002) + new XVector(X,Y),
      new XPoint(8.3025831, 34.218818999999996) + new XVector(X,Y),
      new XPoint(4.3726937, 39.532472) + new XVector(X,Y),
      new XPoint(0.27675277, 68.757565) + new XVector(X,Y)
    );
    
    gfx.DrawPath(thin, path);

    const double sectionStart = 34;
    gfx.DrawLine
    (
      thin,
      new XPoint(X + 8.5, Y + sectionStart),
      new XPoint(X + Width - 1, Y + sectionStart)
    );
    double sectionHeight = (contentRect.Height - sectionStart) / 3.0; 
    
    gfx.DrawLine
    (
      thin,
      new XPoint(X + 1, Y + sectionStart + sectionHeight),
      new XPoint(X + Width - 1, Y + sectionStart + sectionHeight)
    );
    gfx.DrawLine
    (
      thin,
      new XPoint(X, Y + sectionStart + sectionHeight * 2.0),
      new XPoint(X + Width - 1, Y + sectionStart + sectionHeight * 2.0)
    );

    const double valueSection = 40;
    gfx.DrawLine
    (
      thick,
      new XPoint(X + valueSection, Y + sectionStart),
      new XPoint(X + valueSection, contentRect.Y + contentRect.Height)
    );
    
    #endregion

    XTextFormatter formatter = new(gfx);
    formatter.Alignment = XParagraphAlignment.Left;
    
    #region Spell Casting Modifier

    const string spellCastingModifier = "SPELLCASTING MODIFIER";
    XRect spellCastingModifierTitleRect = new
      (
        X + 8.5 + valueSection, 
        Y + sectionStart + 5,
        valueSection + 30,
        sectionHeight - 10
      ), 
      spellCastingModifierValueRect = new
      (
        X + 8.5, 
        Y + sectionStart + 2.5,
        valueSection - 15,
        sectionHeight - 10
      );
    
    formatter.DrawString(spellCastingModifier, titles, this.FontBrush, spellCastingModifierTitleRect);
    gfx.DrawText(this._spellCastingModifier, spellCastingModifierValueRect, XFontStyleEx.Bold, true);

    #endregion

    #region Spell Save DC

    const string spellSaveDc = "SPELL SAVE DC";
    XRect spellSaveDcTitleRect = new
      (
        X + 8.5 + valueSection, 
        Y + sectionStart + 10 + sectionHeight,
        60,
        sectionHeight - 10
      ), 
      spellSaveDcValueRect = new
      (
        X + 8.5, 
        Y + sectionStart + 5 + sectionHeight,
        valueSection - 15,
        sectionHeight - 10
      );
    
    formatter.DrawString(spellSaveDc, titles, this.FontBrush, spellSaveDcTitleRect);
    gfx.DrawText(this._spellSaveDc, spellSaveDcValueRect, XFontStyleEx.Bold, true);

    #endregion

    #region SpellAttack
    
    const string spellAttack = "SPELL ATTACK BONUS";
    XRect spellAttackTitleRect = new
      (
        X + 8.5 + valueSection, 
        Y + sectionStart + 5 + sectionHeight + sectionHeight,
        valueSection + 30,
        sectionHeight - 10
      ), 
      spellAttackValueRect = new
      (
        X + 8.5, 
        Y + sectionStart + 5 + sectionHeight + sectionHeight,
        valueSection - 15,
        sectionHeight - 10
      );
    
    formatter.DrawString(spellAttack, titles, this.FontBrush, spellAttackTitleRect);
    gfx.DrawText(this._spellAttackBonus, spellAttackValueRect, XFontStyleEx.Bold, true);

    #endregion

    #region Spell Casting Ability

    const string spellCastingAbility = "SPELLCASTING ABILITY";
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 12, contentRect.Y + sectionStart - 15),
      new(contentRect.X + contentRect.Width - 6, contentRect.Y + sectionStart - 15)
    );
    XSize size = gfx.MeasureString(spellCastingAbility, subTitle);
    gfx.DrawString
    (
      spellCastingAbility,
      subTitle,
      this.FontBrush,
      contentRect.X + 14, 
      contentRect.Y + sectionStart - 17 + size.Height
    );
    
    XRect castingAbilityBox = new
    (
      contentRect.X + 12,
      contentRect.Y + sectionStart - 15 - 15,
      contentRect.X + contentRect.Width - 6 - (contentRect.X + 12),
      15
    );
    gfx.DrawText(this._spellCasting.ToUpper(), castingAbilityBox, XFontStyleEx.Bold, true);
    
    #endregion
  }

  #endregion
}