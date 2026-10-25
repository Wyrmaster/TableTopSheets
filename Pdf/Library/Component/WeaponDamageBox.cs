using TableTopSheets.Common.Enum;
using TableTopSheets.Pdf.Common;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using TableTopSheets.Pdf.Interface;
using TableTopSheets.Pdf.Text;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Attack table with six rows; attacks that don't fit (or whose cells had to be cut) continue on an extra page
/// </summary>
public class WeaponDamageBox: InformationBox, IOverflowComponent
{
  #region Fields

  private const int Rows = 6;

  private readonly Attack[] _attacks;

  #endregion

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

  #region Property

  /// <inheritdoc />
  public string? Overflow { get; private set; }

  /// <inheritdoc />
  public string ContinuationTitle => "WEAPON & DAMAGE CANTRIPS";

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
    double itemHeight = dataRect.Height / Rows;
    double linePadding = 1.5;
    List<Attack> cut = [];
    for (int i = 0; i < Rows; i++)
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

      bool truncated = false;
      if (attack != null)
      {
        truncated |= this.DrawCell(gfx, attack.Name, nameRect);
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
        truncated |= this.DrawSingleLineCell(gfx, AttackRoll(attack), atkBonusRec);
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
        truncated |= this.DrawSingleLineCell(gfx, Damage(attack), damageTypeNotesRect);
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
        truncated |= this.DrawCell(gfx, attack.Notes, noteRect);
        if (truncated)
        {
          cut.Add(attack);
        }
      }
      
      gfx.DrawLine
      (
        column, 
        new(dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + linePadding, dataRect.Y + itemHeight * (i + 1)),
        new(dataRect.X + nameNotesWidth + atkBonusWidth + damageTypeNotesWidth + nameNotesWidth, dataRect.Y + itemHeight * (i + 1))
      );
    }

    // attacks with a cut cell are repeated in full, attacks without a row follow
    List<Attack> overflow = cut.Concat(this._attacks.Skip(Rows)).ToList();
    this.Overflow = overflow.Count == 0 ? null : string.Join("\n", overflow.Select(Describe));
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   Cell text that may wrap inside the row; returns <c>true</c> when it had to be cut
  /// </summary>
  private bool DrawCell(XGraphics gfx, string text, XRect rect)
  {
    XRect[] rects = [rect];
    TextFitResult fit = TextFit.Fit(gfx, text, TextFit.Family, XFontStyleEx.Bold, rects, TextFit.BodyMaxSize, TextFit.CellMinSize);
    TextFit.Draw(gfx, fit, this.FontBrush, rects, XParagraphAlignment.Center);
    return fit.Overflow != null;
  }

  /// <summary>
  ///   One line cell text; returns <c>true</c> when it had to be cut
  /// </summary>
  private bool DrawSingleLineCell(XGraphics gfx, string text, XRect rect)
  {
    (XFont font, string line, bool truncated) = TextFit.FitSingleLine(gfx, text, TextFit.Family, XFontStyleEx.Bold, TextFit.BodyMaxSize, TextFit.CellMinSize, rect.Width);
    TextFit.DrawLine(gfx, line, font, this.FontBrush, rect, XParagraphAlignment.Center);
    return truncated;
  }

  private static string AttackRoll(Attack attack)
    => attack.AttackRollText ?? (attack.AttackRollModifier >= 0 ? $"+{attack.AttackRollModifier}" : $"{attack.AttackRollModifier}");

  private static string Damage(Attack attack)
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

    return attack.DamageText ?? $"{damageRoll} {attack.DamageType}";
  }

  /// <summary>
  ///   One line of the continuation section, e.g. <c>Fire Bolt — +6 — 2d10 Fire — 120 feet</c>
  /// </summary>
  private static string Describe(Attack attack)
    => string.Join(" — ", new[] { attack.Name, AttackRoll(attack), Damage(attack), attack.Notes }.Where(entry => !string.IsNullOrWhiteSpace(entry)));

  #endregion
}