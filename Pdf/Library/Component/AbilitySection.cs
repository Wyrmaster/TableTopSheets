using TableTopSheets.Common.Container;
using TableTopSheets.Pdf.Component.Generic;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public class AbilitySection: BaseComponent
{
  #region Constants

  private const double X = 13;
  private const double Y = 128.88;
  private const double Width = 222;
  private const double Height = 532.7;

  #endregion

  #region Field

  private readonly int _proficiencyBonus;
  private readonly AbilityContainer _strength;
  private readonly AbilityContainer _dexterity;
  private readonly AbilityContainer _constitution;
  private readonly AbilityContainer _intelligence;
  private readonly AbilityContainer _wisdom;
  private readonly AbilityContainer _charisma;

  #endregion
  
  #region Constructor
  
  public AbilitySection
  (
    int proficiencyBonus,
    AbilityContainer strength,
    AbilityContainer dexterity,
    AbilityContainer constitution,
    AbilityContainer intelligence,
    AbilityContainer wisdom,
    AbilityContainer charisma
  ) : base(new XRect(X, Y, Width, Height))
  {
    this._proficiencyBonus = proficiencyBonus;
    this._strength = strength;
    this._dexterity = dexterity;
    this._constitution = constitution;
    this._intelligence = intelligence;
    this._wisdom = wisdom;
    this._charisma = charisma;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double spacing = 12;
    const double colSize = (Width - spacing) / 2;
    const double verticalSpacing = 8.945491803;

    #region Left Column

    XRect proficiencyRect = new
    (
      X,
      Y,
      colSize,
      70.741803
    );
    new ProficiencyBox(proficiencyRect, $"+{this._proficiencyBonus}").Draw(gfx);
    
    XRect strengthRect = new
    (
      X,
      Y + verticalSpacing + proficiencyRect.Height,
      colSize,
      113.8770492
    );
    new AbilityBox(strengthRect, this._strength, this._proficiencyBonus).Draw(gfx);
    
    XRect dexterityRect = new
    (
      X,
      Y + verticalSpacing * 2 + proficiencyRect.Height + strengthRect.Height,
      colSize,
      150.1106557
    );
    new AbilityBox(dexterityRect, this._dexterity, this._proficiencyBonus).Draw(gfx);
    
    XRect constitutionRect = new
    (
      X,
      Y + verticalSpacing * 3 + proficiencyRect.Height + strengthRect.Height + dexterityRect.Height,
      colSize,
      98.34836066
    );
    new AbilityBox(constitutionRect, this._constitution, this._proficiencyBonus).Draw(gfx);
    
    XRect inspirationRect = new
    (
      X,
      Y  + verticalSpacing * 4 + proficiencyRect.Height + strengthRect.Height + dexterityRect.Height + constitutionRect.Height,
      colSize,
      63.84016393
    );
    new Inspiration(inspirationRect).Draw(gfx);

    #endregion

    #region Right Column

    XRect intelligenceRect = new
    (
      X + spacing + colSize,
      Y,
      colSize,
      176.2041
    );
    new AbilityBox(intelligenceRect, this._intelligence, this._proficiencyBonus).Draw(gfx);

    XRect wisdomRect = new
    (
      X + spacing + colSize,
      Y + verticalSpacing + intelligenceRect.Height,
      colSize,
      176.2041
    );
    new AbilityBox(wisdomRect, this._wisdom, this._proficiencyBonus).Draw(gfx);

    XRect charismaRect = new
    (
      X + spacing + colSize,
      Y + verticalSpacing + verticalSpacing + intelligenceRect.Height + wisdomRect.Height,
      colSize,
      162.4008197
    );
    new AbilityBox(charismaRect, this._charisma, this._proficiencyBonus).Draw(gfx);

    #endregion
  }

  #endregion
}