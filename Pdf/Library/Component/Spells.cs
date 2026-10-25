using TableTopSheets.Pdf.Common;
using TableTopSheets.Pdf.Component.Generic;
using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Component;

public class Spells: InformationBox
{
  #region Fields
  
  private readonly Spell[] _spells;

  #endregion

  #region Constants

  private const double X = 11.61741;
  private const double Y = 152;
  private const double Width = 383.8;
  private const double Height = 680;

  #endregion

  #region Constructor
  
  public Spells(params Spell[] spells) : base(new(X,Y,Width,Height), CornerEnum.Edge, CornerEnum.Slant, CornerEnum.Slant, CornerEnum.Edge)
  {
    this._spells = spells;
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
    XPen gridLine = new(columnColor, 0.5);
    XBrush titleBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    XPen underscore= new(XColor.FromArgb(179, 180, 182), 0.2);
    XPen hline= new(XColor.FromArgb(179, 180, 182), 0.7);

    XFont boxTitleFont = new("Roboto#", 9, XFontStyleEx.Bold);

    XFont textFont = new("Roboto#", 8, XFontStyleEx.Regular);

    #region Header

    const string boxTitle = "CANTRIP & PREPARED SPELLS";
    XSize size = gfx.MeasureString(boxTitle, boxTitleFont);
    gfx.DrawString(boxTitle, boxTitleFont, titleBrush, contentRect.X + contentRect.Width / 2 - size.Width / 2, contentRect.Y + size.Height - 2.5);
    
    gfx.DrawLine
    (
      line,
      new(contentRect.X + 2, contentRect.Y + size.Height + 2),
      new(contentRect.X + contentRect.Width - 4, contentRect.Y + size.Height + 2)
    );
    
    #endregion

    const double nameColumn = 28.33,
      castingTimeColumn = 140.75,
      rangeColumn = 174.45,
      spellTypeColumn = 220.1,
      notesColumn = 290.64;

    const double padding = 3;

    XRect subContentRect = new
    (
      contentRect.X + 2, 
      contentRect.Y + size.Height + 6,
      contentRect.Width - 6,
      contentRect.Height - size.Height - 12
    );

    #region Grid
    
    gfx.DrawLine
    (
      gridLine,
      new XPoint(X + nameColumn, subContentRect.Y),
      new XPoint(X + nameColumn, subContentRect.Y + subContentRect.Height)
    );
    gfx.DrawLine
    (
      gridLine,
      new XPoint(X + castingTimeColumn, subContentRect.Y),
      new XPoint(X + castingTimeColumn, subContentRect.Y + subContentRect.Height)
    );
    gfx.DrawLine
    (
      gridLine,
      new XPoint(X + rangeColumn, subContentRect.Y),
      new XPoint(X + rangeColumn, subContentRect.Y + subContentRect.Height)
    );
    gfx.DrawLine
    (
      gridLine,
      new XPoint(X + spellTypeColumn, subContentRect.Y),
      new XPoint(X + spellTypeColumn, subContentRect.Y + subContentRect.Height)
    );
    gfx.DrawLine
    (
      gridLine,
      new XPoint(X + notesColumn, subContentRect.Y),
      new XPoint(X + notesColumn, subContentRect.Y + subContentRect.Height)
    );

    #endregion

    XFont headerFont = new("Roboto#", 7, XFontStyleEx.Bold);
    XFont markerFont = new("Roboto#", 10, XFontStyleEx.Bold);
    XBrush headerBrush = new XSolidBrush(XColor.FromArgb(110, 110, 112));
    
    #region Headers
    
    const string levelHeader = "Level",
      nameHeader = "Name",
      castingTimeHeader = "Casting Time",
      rangeHeader = "Range",
      notesHeader = "Notes",
      spellTypeHeader = "Concentration, Ritual & Required Material";

    const double underScoreOffset = 18;
    
    XTextFormatter formatter = new(gfx);
    formatter.Alignment = XParagraphAlignment.Left;
    
    gfx.DrawString
    (
      levelHeader,
      headerFont,
      headerBrush,
      new XPoint
      (
        contentRect.X + 2,
        subContentRect.Y + underScoreOffset - 1
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(contentRect.X + 2, subContentRect.Y + underScoreOffset),
      new(contentRect.X + 18.5, subContentRect.Y + underScoreOffset)
    );
    
    gfx.DrawString
    (
      nameHeader,
      headerFont,
      headerBrush,
      new XPoint
      (
        X + nameColumn + 2,
        subContentRect.Y + underScoreOffset - 1
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(X + nameColumn + 2, subContentRect.Y + underScoreOffset),
      new(X + nameColumn + 20, subContentRect.Y + underScoreOffset)
    );
    
    formatter.DrawString
    (
      castingTimeHeader,
      headerFont,
      headerBrush,
      new
      (
        X + castingTimeColumn + 2,
        subContentRect.Y,
        rangeColumn - castingTimeColumn - 4,
        20
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(X + castingTimeColumn + 2, subContentRect.Y + underScoreOffset),
      new(X + castingTimeColumn + 25, subContentRect.Y + underScoreOffset)
    );
    
    gfx.DrawString
    (
      rangeHeader,
      headerFont,
      headerBrush,
      new XPoint
      (
        X + rangeColumn + 2,
        subContentRect.Y + underScoreOffset - 1
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(X + rangeColumn + 2, subContentRect.Y + underScoreOffset),
      new(X + rangeColumn + 13.5, subContentRect.Y + underScoreOffset)
    );
    gfx.DrawLine
    (
      underscore,
      new(X + rangeColumn + 17, subContentRect.Y + underScoreOffset),
      new(X + rangeColumn + 21, subContentRect.Y + underScoreOffset)
    );
    
    formatter.DrawString
    (
      spellTypeHeader,
      headerFont,
      headerBrush,
      new
      (
        X + spellTypeColumn + 2,
        subContentRect.Y,
        notesColumn - spellTypeColumn - 4,
        20
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(X + spellTypeColumn + 2, subContentRect.Y + underScoreOffset),
      new(X + spellTypeColumn + 18, subContentRect.Y + underScoreOffset)
    );
    gfx.DrawLine
    (
      underscore,
      new(X + spellTypeColumn + 20, subContentRect.Y + underScoreOffset),
      new(X + spellTypeColumn + 67.5, subContentRect.Y + underScoreOffset)
    );
    
    gfx.DrawString
    (
      notesHeader,
      headerFont,
      headerBrush,
      new XPoint
      (
        X + notesColumn + 2,
        subContentRect.Y + underScoreOffset - 1
      )
    );
    gfx.DrawLine
    (
      underscore,
      new(X + notesColumn + 2, subContentRect.Y + underScoreOffset),
      new(X + notesColumn + 20, subContentRect.Y + underScoreOffset)
    );
    
    #endregion

    #region Data
    
    double dataHeight = subContentRect.Height - underScoreOffset;
    double rowHeight = dataHeight / 30;

    for (int i = 0; i < 30; i++)
    {
      Spell? spell = null;
      if (this._spells.Length > i)
      {
        spell = this._spells[i];
        XRect dataRect = new XRect
        (
          subContentRect.X,
          subContentRect.Y + underScoreOffset + i * rowHeight + 6,
          nameColumn - 6,
          rowHeight - 4
        );
        XTextFormatter tf = new(gfx);
        tf.Alignment = XParagraphAlignment.Center;
        tf.DrawString(spell.Level, textFont, this.FontBrush, dataRect);
      
        dataRect = new XRect
        (
          subContentRect.X + nameColumn,
          subContentRect.Y + underScoreOffset + i * rowHeight + 6,
          castingTimeColumn - nameColumn - 6,
          rowHeight - 4
        );
        tf.DrawString(spell.Name, textFont, this.FontBrush, dataRect);
      
        dataRect = new XRect
        (
          subContentRect.X + castingTimeColumn,
          subContentRect.Y + underScoreOffset + i * rowHeight + 6,
          rangeColumn - castingTimeColumn - 6,
          rowHeight - 4
        );
        tf.DrawString(spell.CastingTime, textFont, this.FontBrush, dataRect);
      
        dataRect = new XRect
        (
          subContentRect.X + rangeColumn,
          subContentRect.Y + underScoreOffset + i * rowHeight + 6,
          spellTypeColumn - rangeColumn - 6,
          rowHeight - 4
        );
        tf.DrawString(spell.Range, textFont, this.FontBrush, dataRect);
      
        dataRect = new XRect
        (
          subContentRect.X + notesColumn,
          subContentRect.Y + underScoreOffset + i * rowHeight + 6,
          contentRect.Width - notesColumn - 6,
          rowHeight - 4
        );
        tf.DrawString(spell.Notes, textFont, this.FontBrush, dataRect);
      }
      
      gfx.DrawLine
      (
        hline,
        new (subContentRect.X, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight),
        new (X + nameColumn - padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight)
      );
      
      gfx.DrawLine
      (
        hline,
        new (X + nameColumn + padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight),
        new (X + castingTimeColumn - padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight)
      );
      
      gfx.DrawLine
      (
        hline,
        new (X + castingTimeColumn + padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight),
        new (X + rangeColumn - padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight)
      );
      
      gfx.DrawLine
      (
        hline,
        new (X + rangeColumn + padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight),
        new (X + spellTypeColumn - padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight)
      );
      
      new CurveBox
      (
        new(
          X + spellTypeColumn + 7,
          subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - CurveBox.Height,
          CurveBox.Height,
          CurveBox.Width
        ),
        true,
        fill: spell?.Concentration ?? false
      ).Draw(gfx);
      gfx.DrawString
      (
        "C",
        markerFont,
        titleBrush,
        X + spellTypeColumn + 16,
        subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - 4
      );
      
      new CurveBox
      (
        new(
          X + spellTypeColumn + 7 + 20,
          subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - CurveBox.Height,
          CurveBox.Height,
          CurveBox.Width
        ),
        true,
        fill: spell?.Ritual ?? false
      ).Draw(gfx);
      gfx.DrawString
      (
        "R",
        markerFont,
        titleBrush,
        X + spellTypeColumn + 16 + 20,
        subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - 4
      );
      
      new CurveBox
      (
        new(
          X + spellTypeColumn + 7 + 40,
          subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - CurveBox.Height,
          CurveBox.Height,
          CurveBox.Width
        ),
        true,
        fill: spell?.Material ?? false
      ).Draw(gfx);
      gfx.DrawString
      (
        "M",
        markerFont,
        titleBrush,
        X + spellTypeColumn + 16 + 40,
        subContentRect.Y + underScoreOffset + (i + 1) * rowHeight - 4
      );
      
      gfx.DrawLine
      (
        hline,
        new (X + notesColumn + padding, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight),
        new (subContentRect.X + subContentRect.Width, subContentRect.Y + underScoreOffset + (i + 1) * rowHeight)
      );
    }

    #endregion

  }

  #endregion
}