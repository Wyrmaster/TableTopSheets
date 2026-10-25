using TableTopSheets.Common.Enum;
using TableTopSheets.Pdf.Common;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class WeaponDamageBox: InformationBox
{
  private readonly Attack[] _attacks;

  #region Constants

  private const double X = 245.04;
  private const double Y = 184.5;
  private const double Width = 336.96;
  private const double Height = 168.97;

  #endregion
  
  #region Constructor
  
  public WeaponDamageBox(params Attack[] attacks) : base(new XRect(X, Y, Width, Height), CornerEnum.Slant, CornerEnum.Edge, CornerEnum.Edge, CornerEnum.Slant)
  {
    this._attacks = attacks;
  }

  #endregion

  #region Override

  public override void WithRender(XGraphics gfx, XRect contentRect)
  {
    const double lineSize = 1.2;
    const double columnSize = 0.3;
    const double underlineSize = 0.2;
    
    XPen line = new(this.FontColor, lineSize);
    XColor columnColor = XColor.FromArgb(210, 211, 213);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    XPen column = new(columnColor, columnSize);
    XPen underline = new(this.BaseColor, underlineSize);

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);
    XFont titleFont = new("Roboto#", 8);
    
    
    const string boxTitle = "WEAPON & DAMAGE CANTRIPS";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
    
    const string name = "Name", atkBonus = "Atk Bonus / DC", damageType = "Damage & Type", notes = "Notes";
    XSize nameSize = gfx.MeasureString(name, titleFont);
    XSize atkBonusSize = gfx.MeasureString(atkBonus, titleFont);
    XSize damageTypeSize = gfx.MeasureString(damageType, titleFont);
    XSize notesSize = gfx.MeasureString(notes, titleFont);

    double width = contentRect.Width - 4 - 2;

    double atkBonusWidth = atkBonusSize.Width + 4;
    double nameNotesWidth = (width / 25) * 8.0;
    double damageTypeNotesWidth = width - nameNotesWidth * 2 - atkBonusWidth;
    
    gfx.DrawString(name, titleFont, this.BaseBrush, contentRect.X + 4, contentRect.Y + size.Height + 5 + nameSize.Height);
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + 4, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + 4 + nameSize.Width, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    
    gfx.DrawLine
    (
      column,
      new(contentRect.X + nameNotesWidth, contentRect.Y + size.Height + 5),
      new(contentRect.X + nameNotesWidth, contentRect.Y + contentRect.Height - 7)
    );
    gfx.DrawString(atkBonus, titleFont, this.BaseBrush, contentRect.X + nameNotesWidth + 2, contentRect.Y + size.Height + 5 + nameSize.Height);
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusSize.Width + 2, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    
    gfx.DrawLine
    (
      column,
      new(contentRect.X + nameNotesWidth + atkBonusWidth, contentRect.Y + size.Height + 5),
      new(contentRect.X + nameNotesWidth + atkBonusWidth, contentRect.Y + contentRect.Height - 7)
    );
    gfx.DrawString(damageType, titleFont, this.BaseBrush, contentRect.X + nameNotesWidth + atkBonusWidth + 2, contentRect.Y + size.Height + 5 + nameSize.Height);
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 20 + 2, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 24 + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 41 + 2, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 44 + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 45.3 + 2, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + 47 + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeSize.Width + 2, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    
    gfx.DrawLine
    (
      column,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth, contentRect.Y + size.Height + 5),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth, contentRect.Y + contentRect.Height - 7)
    );
    gfx.DrawString(notes, titleFont, this.BaseBrush, contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + 2, contentRect.Y + size.Height + 5 + nameSize.Height);
    gfx.DrawLine
    (
      underline,
      new(contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + 2, contentRect.Y + size.Height + 6 + nameSize.Height),
      new(contentRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + 2 + notesSize.Width, contentRect.Y + size.Height + 6 + nameSize.Height)
    );
    double contentHeight = size.Height + 6 + nameSize.Height;
    XRect dataRect = new(contentRect.X, contentRect.Y + contentHeight + 1, contentRect.Width, contentRect.Height - contentHeight - 8.5);
    double itemHeight = dataRect.Height / 6;
    double linePadding = 1.5;
    for (int i = 0; i < 6; i++)
    {
      Attack? attack = null;
      if (this._attacks.Length > i)
      {
        attack = this._attacks[i];
      }
      XRect nameRect = new
      (
        dataRect.X + 4,
        dataRect.Y + i * itemHeight + 5,
        nameNotesWidth - 6, 
        itemHeight - 1.5
      );

      if (attack != null)
      {
        XTextFormatter tf = new(gfx);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString
        (
          attack.Name,
          new XFont("Roboto#", 8, XFontStyleEx.Bold),
          this.FontBrush,
          nameRect
        );
      }
      
      gfx.DrawLine
      (
        column, 
        new(dataRect.X + 4, dataRect.Y + itemHeight * (i + 1)),
        new(dataRect.X + nameNotesWidth - linePadding, dataRect.Y + itemHeight * (i + 1))
      );
      
      XRect atkBonusRec = new
      (
        dataRect.X + nameNotesWidth + linePadding,
        dataRect.Y + i * itemHeight + 5,
        atkBonusWidth - linePadding * 2, 
        itemHeight - 1.5
      );
      
      if (attack != null)
      {
        XTextFormatter tf = new(gfx);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString
        (
          $"+{attack.AttackRollModifier}",
          new XFont("Roboto#", 8, XFontStyleEx.Bold),
          this.FontBrush,
          atkBonusRec
        );
      }
      
      gfx.DrawLine
      (
        column, 
        new(dataRect.X + nameNotesWidth + linePadding, dataRect.Y + itemHeight * (i + 1)),
        new(dataRect.X + nameNotesWidth + atkBonusWidth - linePadding, dataRect.Y + itemHeight * (i + 1))
      );
      
      XRect damageTypeNotesRect = new
      (
        dataRect.X + nameNotesWidth + atkBonusWidth + linePadding,
        dataRect.Y + i * itemHeight + 5,
        damageTypeNotesWidth - linePadding * 2, 
        itemHeight - 1.5
      );
      
      if (attack != null)
      {
        string damageRoll = string.Join
        (
          "",
          attack
            .DamageRoll
            .Where(entry => entry != DiceEnum.D1)
            .GroupBy(entry => entry)
            .Select(entry => $"{entry.Count()}{entry.Key}")
        );
        if (attack.DamageRoll.Any(entry => entry == DiceEnum.D1))
        {
          damageRoll += $" +{attack.DamageRoll.Count(entry => entry == DiceEnum.D1)}";
        }
        XTextFormatter tf = new(gfx);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString
        (
          $"{damageRoll} {attack.DamageType}",
          new XFont("Roboto#", 8, XFontStyleEx.Bold),
          this.FontBrush,
          damageTypeNotesRect
        );
      }
      
      gfx.DrawLine
      (
        column, 
        new(dataRect.X + nameNotesWidth + atkBonusWidth + linePadding, dataRect.Y + itemHeight * (i + 1)),
        new(dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth - linePadding, dataRect.Y + itemHeight * (i + 1))
      );
      
      XRect noteRect = new
      (
        dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + linePadding,
        dataRect.Y + i * itemHeight + 5,
        nameNotesWidth - linePadding, 
        itemHeight - 1.5
      );
      if (attack != null)
      {
        XTextFormatter tf = new(gfx);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString
        (
          attack.Notes,
          new XFont("Roboto#", 8, XFontStyleEx.Bold),
          this.FontBrush,
          noteRect
        );
      }
      
      gfx.DrawLine
      (
        column, 
        new(dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + linePadding, dataRect.Y + itemHeight * (i + 1)),
        new(dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + nameNotesWidth, dataRect.Y + itemHeight * (i + 1))
      );
    }
  }

  #endregion
}