using TableTopSheets.Common.Container;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;
using TableTopSheets.Common.Extension;
using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component.Generic;

public class AbilityBox : BaseComponent
{
  #region Field

  private readonly AbilityContainer _abilityContainer;
  private readonly int _proficiencyBonus;

  #endregion

  #region Constructor

  public AbilityBox(XRect rect, AbilityContainer abilityContainer, int proficiencyBonus) : base(rect)
  {
    this._abilityContainer = abilityContainer;
    this._proficiencyBonus = proficiencyBonus;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thinBlack = new XPen(this.FontColor, innerBorderSize * 2);
    XPen thick = new XPen(BaseColor, outerBorderSize);

    XFont markerFont = new("Roboto#", 8);
    XFont modifierFont = new("Roboto#", 10);

    const double lowerEdgeX = 4;
    const double lowerEdgeY = 5;
    const double outerRadius = 6;
    
    const double upperShieldX = 12;
    
    int modifier = (int)(Math.Floor((this._abilityContainer.Score - 10) / 2.0));

    XGraphicsPath path;

    #region Border

    path = new();

    path.AddArc
    (
      this.Rect.X + this.Rect.Width - outerRadius,
      this.Rect.Y - outerRadius,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      180,
      -90
    );

    path.AddLines
    (
      [
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + outerRadius),
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + this.Rect.Height - lowerEdgeY),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + lowerEdgeX, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X, this.Rect.Y + this.Rect.Height - lowerEdgeY),
        new(this.Rect.X, this.Rect.Y + outerRadius),
      ]
    );

    path.AddArc
    (
      this.Rect.X - outerRadius,
      this.Rect.Y - outerRadius,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      90,
      -90
    );

    path.CloseFigure();
    gfx.DrawPath(thick, path);

    path = new();
    path.AddLines
    (
      [
        new(this.Rect.X + this.Rect.Width, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX / 2, this.Rect.Y + this.Rect.Height - lowerEdgeY / 4),
        new(this.Rect.X + this.Rect.Width - lowerEdgeX, this.Rect.Y + this.Rect.Height),
      ]
    );
    path.CloseFigure();
    gfx.DrawPath(thick, this.BaseBrush, path);

    path = new();
    path.AddLines
    (
      [
        new(this.Rect.X, this.Rect.Y + this.Rect.Height),
        new(this.Rect.X + lowerEdgeX / 2, this.Rect.Y + this.Rect.Height - lowerEdgeY / 4),
        new(this.Rect.X + lowerEdgeX, this.Rect.Y + this.Rect.Height),
      ]
    );
    path.CloseFigure();
    gfx.DrawPath(thick, this.BaseBrush, path);

    #endregion

    #region ScoreBox

    path = new XGraphicsPath();
    
    path.AddBezier
    (
      new XPoint(upperShieldX, -Constants.OuterMargin).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(10.136936, 11.27196).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(5.1663153, 39.021335).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(0.75, 47.671).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );

    const double upperSeparator = 68.101428;
    path.AddBezier
    (
      new XPoint(0.75, 47.671).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(3.6790427, 59.373487).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(7.2015299, 63.95272).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      new XPoint(11.389377, upperSeparator).Flip() + new XVector(this.Rect.X, this.Rect.Y)
    );

    path.AddBezier
    (
      new XPoint(11.389377, upperSeparator).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(7.2015299, 63.95272).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(3.6790427, 59.373487).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(0.75, 47.671).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );

    path.AddBezier
    (
      new XPoint(0.75, 47.671).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(5.1663153, 39.021335).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(10.136936, 11.27196).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      new XPoint(upperShieldX, -Constants.OuterMargin).Flip(flipX: true) +
      new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y)
    );

    path.CloseFigure();

    gfx.DrawPath(thin, XBrushes.White, path);

    gfx.DrawLine
    (
      thin,
      new XPoint(this.Rect.X - Constants.OuterMargin, this.Rect.Y + upperSeparator),
      new XPoint(this.Rect.X + this.Rect.Width + Constants.OuterMargin, this.Rect.Y + upperSeparator)
    );

    #endregion
    
    #region OuterBorder

    path = new();
    
    path.AddArc
    (
      this.Rect.X - Constants.OuterMargin - upperShieldX - Constants.OuterMargin,
      this.Rect.Y - Constants.OuterMargin - upperShieldX - Constants.OuterMargin,
      (upperShieldX + Constants.OuterMargin) * 2,
      (upperShieldX + Constants.OuterMargin) * 2,
      0,
      90
    );
    
    path.AddLines
    (
      [
        new XPoint(-Constants.OuterMargin, upperShieldX).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(-Constants.OuterMargin, this.Rect.Height - upperShieldX).Flip() + new XVector(this.Rect.X, this.Rect.Y),
        new XPoint(upperShieldX - 4, this.Rect.Height).Flip() + new XVector(this.Rect.X, this.Rect.Y),
      ]
    );
    
    path.AddLines
    (
      [
        new XPoint(upperShieldX - 4, this.Rect.Height).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(-Constants.OuterMargin, this.Rect.Height - upperShieldX).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
        new XPoint(-Constants.OuterMargin, upperShieldX).Flip(flipX: true) + new XVector(this.Rect.X + this.Rect.Width, this.Rect.Y),
      ]
    );
    
    path.AddArc
    (
      this.Rect.X + this.Rect.Width + Constants.OuterMargin - upperShieldX - Constants.OuterMargin,
      this.Rect.Y - Constants.OuterMargin - upperShieldX - Constants.OuterMargin,
      (upperShieldX + Constants.OuterMargin) * 2,
      (upperShieldX + Constants.OuterMargin) * 2,
      90,
      90
    );
    
    gfx.DrawPath(thin, path);
    
    #endregion

    #region ScoreValue

    path = new();
    
    path.AddLine
    (
      new XPoint(this.Rect.X + 53.6201, this.Rect.Y + 22.9353),
      new XPoint(this.Rect.X + 71.7022, this.Rect.Y + 22.9353)
    );
    
    path.AddBezier
    (
      new XPoint(this.Rect.X + 71.7022, this.Rect.Y + 22.9353),
      new XPoint(this.Rect.X + 72.7394, this.Rect.Y + 25.0879),
      new XPoint(this.Rect.X + 75.2834, this.Rect.Y + 27.2601),
      new XPoint(this.Rect.X + 78.2971, this.Rect.Y + 27.3776)
    );
    
    path.AddLines
    (
      [
        new XPoint(this.Rect.X + 78.2971, this.Rect.Y + 27.3776),
        new XPoint(this.Rect.X + 83.6004, this.Rect.Y + 27.3776),
        new XPoint(this.Rect.X + 89.1581, this.Rect.Y + 50.4499),
      ]
    );
    
    gfx.DrawPath(thin, path);

    path = new();
    
    path.AddLines
    (
      [
        new XPoint(this.Rect.X + 53.6201, this.Rect.Y + 27.3776),
        new XPoint(this.Rect.X + 86.6923, this.Rect.Y + 27.3776),
        new XPoint(this.Rect.X + 92.25, this.Rect.Y + 50.4499),
        new XPoint(this.Rect.X + 53.6201, this.Rect.Y + 50.4499),
      ]
    );
    
    gfx.DrawPath(thin, path);
    
    XRect scoreBoxRect = new
    (
      this.Rect.X + 62,
      this.Rect.Y + 27.5,
      21,
      21
    );
    gfx.DrawText(this._abilityContainer.Score.ToString(), scoreBoxRect, center: true);

    const double circleSize = 20;
    
    gfx.DrawEllipse
    (
      thick,
      XBrushes.White,
      this.Rect.X + 20,
      this.Rect.Y + 18,
      circleSize + circleSize,
      circleSize + circleSize
    );
    
    const double verticalCorrection = 0.5;
    gfx.DrawBezier(
      thin,
      new XPoint(20.8121, 34.0762) + new XVector(this.Rect.X, this.Rect.Y + verticalCorrection),
      new XPoint(21.2425, 38.1857) + new XVector(this.Rect.X, this.Rect.Y + verticalCorrection),
      new XPoint(29.9705, 48.5184) + new XVector(this.Rect.X, this.Rect.Y + verticalCorrection),
      new XPoint(29.5, 53.8021) + new XVector(this.Rect.X, this.Rect.Y + verticalCorrection)
    );

    double rightCorrection = this.Rect.Width - 25;
    gfx.DrawBezier(
      thin,
      new XPoint(20.8121, 34.0762).Flip(flipX: true) + new XVector(this.Rect.X + rightCorrection, this.Rect.Y + verticalCorrection),
      new XPoint(21.2425, 38.1857).Flip(flipX: true) + new XVector(this.Rect.X + rightCorrection, this.Rect.Y + verticalCorrection),
      new XPoint(29.9705, 48.5184).Flip(flipX: true) + new XVector(this.Rect.X + rightCorrection, this.Rect.Y + verticalCorrection),
      new XPoint(29.5, 53.8021).Flip(flipX: true) + new XVector(this.Rect.X + rightCorrection, this.Rect.Y + verticalCorrection)
    );

    const double lowerRect = 6;
    gfx.DrawPolygon
    (
      thin,
      [
        new(this.Rect.X + 20 + circleSize - lowerRect / 2, this.Rect.Y + 18 + circleSize + circleSize),
        new(this.Rect.X + 20 + circleSize, this.Rect.Y + 18 + circleSize + circleSize - lowerRect),
        new(this.Rect.X + 20 + circleSize + lowerRect / 2, this.Rect.Y + 18 + circleSize + circleSize),
      ]
    );

    const double heightModifier = 3;
    const double widthModifier = 3;
    string modiferText = $"{(modifier == 0 ? string.Empty : modifier < 0 ? string.Empty : "+")}{modifier}";
    XRect valueBox = new
    (
      this.Rect.X + 20 + circleSize / 2 - widthModifier / 2,
      this.Rect.Y + 18 + circleSize / 2 - heightModifier,
      circleSize + widthModifier,
      circleSize + heightModifier
    );
    gfx.DrawText(modiferText, valueBox, XFontStyleEx.Bold);
    
    #endregion

    #region Top Text

    const double headerWidth = 70;
    XRect headerRect = new
    (
      this.Rect.X + this.Rect.Width / 2 - headerWidth / 2,
      this.Rect.Y + 1,
      headerWidth,
      15
    );
    
    gfx.DrawText(this._abilityContainer.AbilityName, headerRect, XFontStyleEx.Bold, center: true);
    
    const string modifierHeader = "MODIFIER";
    
    const double modifierWidth = 40;
    XRect modifierRect = new
    (
      this.Rect.X + 20 + circleSize - modifierWidth / 2,
      this.Rect.Y + 18 + circleSize + circleSize + 1,
      modifierWidth,
      8
    );
    gfx.DrawText(modifierHeader, modifierRect);
    
    const string score = "SCORE";
    const double scoreWidth = 35;
    XRect scoreRect = new
    (
      this.Rect.X + 90.1581 - scoreWidth,
      this.Rect.Y + 50.4,
      scoreWidth,
      8
    );
    gfx.DrawText(score, scoreRect);

    #endregion

    const double proficiencyCircleRadius = 4.5;
    const double line = 17.5;
    
    #region Saving Throw

    const double savingThrowSection = 22;
    double emptyCorrection = this._abilityContainer.Skill.Any() ? -0.5 : 2;
    XRect lowerSectionRect = new
    (
      this.Rect.X + 1,
      this.Rect.Y + 1 + upperSeparator + emptyCorrection,
      this.Rect.Width - 2,
      savingThrowSection
    );

    void DrawAbility(string abilityProficiency, string proficiencyValue, string proficiencyHeader, double verticalOffset)
    {
      gfx.DrawEllipse
      (
        thinBlack,
        this.Rect.X + 20 - savingThrowSection / 2,
        lowerSectionRect.Y + lowerSectionRect.Height / 2 - proficiencyCircleRadius + emptyCorrection + verticalOffset,
        proficiencyCircleRadius + proficiencyCircleRadius,
        proficiencyCircleRadius + proficiencyCircleRadius
      );
    
      gfx.DrawLine
      (
        thin,
        new
        (
          this.Rect.X + 22.5,
          lowerSectionRect.Y + lowerSectionRect.Height / 2 + proficiencyCircleRadius + 0.5 + emptyCorrection + verticalOffset
        ),
        new
        (
          this.Rect.X + 22.5 + line,
          lowerSectionRect.Y + lowerSectionRect.Height / 2 + proficiencyCircleRadius + 0.5 + emptyCorrection + verticalOffset
        )
      );
      
      XRect messageRect = new
      (
        this.Rect.X + 22.5 + line + 2.5,
        lowerSectionRect.Y + lowerSectionRect.Height / 2 - proficiencyCircleRadius - 2 + emptyCorrection + 1 + verticalOffset,
        55,
        proficiencyCircleRadius + proficiencyCircleRadius + 2
      );
      
      gfx.DrawText(proficiencyHeader, messageRect, XFontStyleEx.Bold, center: true, alignment: XParagraphAlignment.Left);
      
      XSize size = gfx.MeasureString(abilityProficiency, markerFont);
      gfx.DrawString
      (
        abilityProficiency,
        markerFont,
        this.FontBrush,
        this.Rect.X + 20 - savingThrowSection / 2 + proficiencyCircleRadius * 2 - size.Height / 2 + 1,
        lowerSectionRect.Y + lowerSectionRect.Height / 2 + emptyCorrection - size.Width / 2 + 2 + verticalOffset,
        XStringFormats.Center
      );

      size = gfx.MeasureString(proficiencyValue, modifierFont);
    
      gfx.DrawString
      (
        proficiencyValue,
        modifierFont,
        this.FontBrush,
        this.Rect.X + 22.5 + line / 2 - size.Width / 2,
        lowerSectionRect.Y + lowerSectionRect.Height / 2 + proficiencyCircleRadius + 0.5 + emptyCorrection - 1.5 + verticalOffset
      );
    }
    
    const string savingThrow = "Saving Throw";
    int savingThrowModifier = this._abilityContainer.SavingThrow ? (this._proficiencyBonus + modifier) : modifier;
    string modifierString = $"{(savingThrowModifier == 0 ? string.Empty : savingThrowModifier < 0 ? string.Empty : "+")}{savingThrowModifier}";
    string savingThrowText = this._abilityContainer.SavingThrow ? "P" : string.Empty;

    DrawAbility(savingThrowText, modifierString, savingThrow, 0);
    

    #endregion

    #region Skills

    if (!this._abilityContainer.Skill.Any())
    {
      return;
    }
    
    gfx.DrawLine
    (
      thin,
      new XPoint(this.Rect.X - Constants.OuterMargin, this.Rect.Y + upperSeparator + savingThrowSection),
      new XPoint(this.Rect.X + this.Rect.Width + Constants.OuterMargin, this.Rect.Y + upperSeparator + savingThrowSection)
    );

    const double boxVCorrection = 4;
    XRect skillSectionRect = new
    (
      new XPoint(this.Rect.X, this.Rect.Y + upperSeparator + savingThrowSection + boxVCorrection),
      new XPoint(this.Rect.X + this.Rect.Width, this.Rect.Y + this.Rect.Height - boxVCorrection)
    );

    double skillSectionHeight = skillSectionRect.Height / this._abilityContainer.Skill.Count();

    int iterator = 0;
    foreach (var (skillModifier, skill) in this._abilityContainer.Skill)
    {
      string abilityState = skillModifier switch
      {
        SkillModifierEnum.None => string.Empty,
        SkillModifierEnum.Half => "J",
        SkillModifierEnum.Proficient => "P",
        SkillModifierEnum.Expertise => "E",
        _ => string.Empty
      };

      int modifierValue = skillModifier.GetProficiency(this._proficiencyBonus) + modifier;
      
      DrawAbility
      (
        abilityState,
        $"{(modifierValue == 0 ? string.Empty : modifierValue < 0 ? string.Empty : "+")}{modifierValue}",
        skill,
        22.5 + skillSectionHeight * iterator++
      );
    }

    #endregion
  }

  #endregion
}