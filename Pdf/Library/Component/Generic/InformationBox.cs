using TableTopSheets.Pdf.Enum;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component.Generic;

public abstract class InformationBox: BaseComponent
{
  #region Constants

  private const double Margin = 1;

  #endregion
  
  #region Field
  
  private XRect _rect;
  private readonly CornerEnum _tr;
  private readonly CornerEnum _tl;
  private readonly CornerEnum _bl;
  private readonly CornerEnum _br;

  #endregion

  #region Constructor
  
  protected InformationBox(XRect rect, CornerEnum tl, CornerEnum tr, CornerEnum br, CornerEnum bl)
    : base(rect with{X = rect.X - Margin / 2, Y = rect.Y - Margin / 2, Width = rect.Width - Margin, Height = rect.Height - Margin})
  {
    this._tr = tr;
    this._tl = tl;
    this._bl = bl;
    this._br = br;
    
    if (this.EdgeValidation(tr, tl, br)
        || this.EdgeValidation(tl, tr, bl)
        || this.EdgeValidation(br, tr, bl)
        || this.EdgeValidation(bl, tl, br)
       )
    {
      throw new ArgumentException("Invalid corner combination 'Edge' can't border a 'Curve'");
    }
  }

  #endregion

  #region Override
  
  protected override void WithRender(XGraphics gfx)
  {
    const double outerSlant = 5.0;
    const double innerSlant = 10;
    
    const double outerDiameter = 15;
    const double innerDiameter = 8;
    const double dotDiameter = 4;
    
    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;

    const double lineDifference = (outerBorderSize - innerBorderSize) / 2;
    
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);

    XGraphicsPath outerPath = new();
    XGraphicsPath innerPath = new();

    XPoint tlOuterStart = default, tlOuterEnd = default, tlInnerStart = default, tlInnerEnd = default;
    XPoint trOuterStart = default, trOuterEnd = default, trInnerStart = default, trInnerEnd = default;
    XPoint brOuterStart = default, brOuterEnd = default, brInnerStart = default, brInnerEnd = default;
    XPoint blOuterStart = default, blOuterEnd = default, blInnerStart = default, blInnerEnd = default;
    
    #region Top Left

    switch (this._tl)
    {
      case CornerEnum.Slant:
      {
        tlOuterStart = new(this.Rect.X - lineDifference, this.Rect.Y + innerSlant);
        tlOuterEnd = new(this.Rect.X + innerSlant, this.Rect.Y - lineDifference);

        tlInnerStart = new(this.Rect.X, this.Rect.Y + outerSlant); 
        tlInnerEnd = new(this.Rect.X + outerSlant, this.Rect.Y);

        outerPath.AddLine(tlOuterStart, tlOuterEnd);
        innerPath.AddLine(tlInnerStart, tlInnerEnd);
        break;
      }
      case CornerEnum.Edge:
      {
        tlOuterStart = tlOuterEnd = new
        (
          this._bl == CornerEnum.Slant ? this.Rect.X - lineDifference : this.Rect.X - Constants.OuterMargin,
          this._tr == CornerEnum.Slant ? this.Rect.Y - lineDifference : this.Rect.Y - Constants.OuterMargin
        );

        tlInnerStart = tlInnerEnd = new
        (
          this.Rect.X,
          this.Rect.Y
        );
        break;
      }
      case CornerEnum.Curve:
      {
        tlOuterStart = new(this.Rect.X - Constants.OuterMargin, this.Rect.Y + outerDiameter - Constants.OuterMargin);
        tlOuterEnd = new(this.Rect.X + outerDiameter - Constants.OuterMargin, this.Rect.Y - Constants.OuterMargin);
        
        tlInnerStart = new(this.Rect.X, this.Rect.Y + innerDiameter);
        tlInnerEnd = new(this.Rect.X + innerDiameter, this.Rect.Y);
        
        outerPath.AddArc
        (
          tlOuterStart.X - outerDiameter,
          tlOuterEnd.Y - outerDiameter,
          outerDiameter + outerDiameter,
          outerDiameter + outerDiameter,
          90,
          -90
        );
        
        innerPath.AddArc
        (
          tlInnerStart.X - innerDiameter,
          tlInnerEnd.Y - innerDiameter,
          innerDiameter + innerDiameter,
          innerDiameter + innerDiameter,
          90,
          -90
        );
        
        gfx.DrawEllipse
        (
          this.BaseBrush,
          this.Rect.X - dotDiameter * 0.75,
          this.Rect.Y - dotDiameter * 0.75,
          dotDiameter + dotDiameter,
          dotDiameter + dotDiameter
        );
        
        break;
      }
    }

    #endregion

    #region Top Right
    
    switch (this._tr)
    {
      case CornerEnum.Slant:
      {
        trOuterStart = new
        (
          this.Rect.X + this.Rect.Width - innerSlant,
          tlOuterEnd.Y
        );
        trOuterEnd = new
        (
          this.Rect.X + this.Rect.Width + lineDifference,
          this.Rect.Y + innerSlant
        );

        trInnerStart = new
        (
          this.Rect.X + this.Rect.Width - outerSlant,
          tlInnerEnd.Y
        );
        trInnerEnd = new
        (
          this.Rect.X + this.Rect.Width,
          this.Rect.Y + outerSlant
        );
      
        outerPath.AddLines([tlOuterEnd, trOuterStart, trOuterEnd]);
      
        innerPath.AddLines([tlInnerEnd, trInnerStart, trInnerEnd]);
        break;
      }
      case CornerEnum.Edge:
      {
        trOuterStart = trOuterEnd = new
        (
          this._br == CornerEnum.Slant ? this.Rect.X + this.Rect.Width + lineDifference : this.Rect.X + this.Rect.Width + Constants.OuterMargin,
          tlOuterEnd.Y
        );

        trInnerStart = trInnerEnd = new
        (
          this.Rect.X + this.Rect.Width,
          tlInnerEnd.Y
        );

        outerPath.AddLine(tlOuterEnd, trOuterStart);

        innerPath.AddLine(tlInnerEnd, trInnerStart);
        break;
      }
      case CornerEnum.Curve:
      {
        trOuterStart = new(this.Rect.X + this.Rect.Width - outerDiameter + Constants.OuterMargin, tlOuterEnd.Y);
        trOuterEnd = new(this.Rect.X + this.Rect.Width + Constants.OuterMargin, this.Rect.Y + outerDiameter - Constants.OuterMargin);
        
        trInnerStart = new(this.Rect.X + this.Rect.Width - innerDiameter, tlInnerEnd.Y);
        trInnerEnd = new(this.Rect.X + this.Rect.Width, this.Rect.Y + innerDiameter);
        
        if (this._tl == CornerEnum.Edge)
        {
          outerPath.AddLine(tlOuterEnd, trOuterStart);
          innerPath.AddLine(tlInnerEnd, trInnerStart);
        }
        outerPath.AddArc
        (
          trOuterStart.X,
          trOuterEnd.Y - outerDiameter - outerDiameter,
          outerDiameter + outerDiameter,
          outerDiameter + outerDiameter,
          180,
          -90
        );
        
        innerPath.AddArc
        (
          trInnerStart.X,
          trInnerEnd.Y - innerDiameter - innerDiameter,
          innerDiameter + innerDiameter,
          innerDiameter + innerDiameter,
          180,
          -90
        );
        
        gfx.DrawEllipse
        (
          this.BaseBrush,
          this.Rect.X + this.Rect.Width - dotDiameter * 1.25,
          this.Rect.Y - dotDiameter * 0.75,
          dotDiameter + dotDiameter,
          dotDiameter + dotDiameter
        );
        break;
      }
    }

    #endregion

    #region Bottom Right
    
    switch (this._br)
    {
      case CornerEnum.Slant:
      {
        brOuterStart = new
        (
          trOuterEnd.X,
          this.Rect.Y + this.Rect.Height - innerSlant
        );
        brOuterEnd = new
        (
          this.Rect.X + this.Rect.Width - innerSlant,
          this.Rect.Y + this.Rect.Height + lineDifference
        );

        brInnerStart = new
        (
          trInnerEnd.X,
          this.Rect.Y + this.Rect.Height - outerSlant
        );
        brInnerEnd = new
        (
          this.Rect.X + this.Rect.Width - outerSlant,
          this.Rect.Y + this.Rect.Height
        );
      
        outerPath.AddLine(brOuterStart, brOuterEnd);
        innerPath.AddLine(brInnerStart, brInnerEnd);
        break;
      }
      case CornerEnum.Edge:
      {
        brOuterStart = brOuterEnd = new
        (
          trOuterEnd.X,
          this._bl == CornerEnum.Slant ? this.Rect.Y + this.Rect.Height : this.Rect.Y + this.Rect.Height + Constants.OuterMargin
        );

        brInnerStart = brInnerEnd = new
        (
          trInnerEnd.X,
          this._bl == CornerEnum.Slant ? this.Rect.Y + this.Rect.Height - lineDifference : this.Rect.Y + this.Rect.Height
        );
      
        outerPath.AddLine(trOuterEnd, brOuterStart);
      
        innerPath.AddLine(trInnerEnd, brInnerStart);
        break;
      }
      case CornerEnum.Curve:
      {
        brOuterStart = new(trOuterEnd.X, this.Rect.Y + this.Rect.Height + outerDiameter - Constants.OuterMargin);
        brOuterEnd = new(this.Rect.X + this.Rect.Width + Constants.OuterMargin - outerDiameter, this.Rect.Y + this.Rect.Height + Constants.OuterMargin);
        
        brInnerStart = new(trInnerEnd.X, this.Rect.Y + this.Rect.Height - innerDiameter);
        brInnerEnd = new(this.Rect.X + this.Rect.Width - innerDiameter, this.Rect.Y + this.Rect.Height);
        
        outerPath.AddArc
        (
          brOuterStart.X - outerDiameter,
          brOuterEnd.Y - outerDiameter,
          outerDiameter + outerDiameter,
          outerDiameter + outerDiameter,
          270,
          -90
        );
        
        innerPath.AddArc
        (
          brInnerStart.X - innerDiameter,
          brInnerEnd.Y - innerDiameter,
          innerDiameter + innerDiameter,
          innerDiameter + innerDiameter,
          270,
          -90
        );
        
        gfx.DrawEllipse
        (
          this.BaseBrush,
          this.Rect.X + this.Rect.Width - dotDiameter * 1.25,
          this.Rect.Y + this.Rect.Height - dotDiameter * 1.25,
          dotDiameter + dotDiameter,
          dotDiameter + dotDiameter
        );
        break;
      }
    }
    
    #endregion
    
    #region Bottom Left
    
    switch (this._bl)
    {
      case CornerEnum.Slant:
      {
        blOuterStart = new(this.Rect.X + innerSlant + lineDifference, brOuterEnd.Y);
        blOuterEnd = new(tlOuterStart.X, this.Rect.Y + this.Rect.Height - innerSlant);

        blInnerStart = new(this.Rect.X + outerSlant, brInnerEnd.Y);
        blInnerEnd = new(tlInnerStart.X, this.Rect.Y + this.Rect.Height - outerSlant);
        
        outerPath.AddLines([brOuterEnd, blOuterStart, blOuterEnd]);
        innerPath.AddLines([brInnerEnd, blInnerStart, blInnerEnd]);
        break;
      }
      case CornerEnum.Edge:
      {
        blOuterStart = blOuterEnd = new(tlOuterStart.X, brOuterEnd.Y);
        blInnerStart = blInnerEnd = new(tlInnerStart.X, brInnerEnd.Y);
      
        outerPath.AddLine(brOuterEnd, blOuterStart);
        innerPath.AddLine(brInnerEnd, blInnerStart);
        break;
      }
      case CornerEnum.Curve:
      {
        blOuterStart = new(this.Rect.X + outerDiameter - Constants.OuterMargin, brOuterEnd.Y);
        blOuterEnd = new(tlOuterStart.X, this.Rect.Y - innerDiameter + outerDiameter);
        
        blInnerStart = new(this.Rect.X + innerDiameter, brInnerEnd.Y);
        blInnerEnd = new(tlInnerStart.X, this.Rect.Y + this.Rect.Height - innerDiameter);
        
        outerPath.AddArc
        (
          blOuterStart.X - outerDiameter - outerDiameter,
          blOuterStart.Y - outerDiameter,
          outerDiameter + outerDiameter,
          outerDiameter + outerDiameter,
          0,
          -90
        );
        
        innerPath.AddArc
        (
          blInnerStart.X - innerDiameter - innerDiameter,
          blInnerStart.Y - innerDiameter,
          innerDiameter + innerDiameter,
          innerDiameter + innerDiameter,
          0,
          -90
        );
        
        gfx.DrawEllipse
        (
          this.BaseBrush,
          this.Rect.X - dotDiameter * 0.75,
          this.Rect.Y + this.Rect.Height - dotDiameter * 1.25,
          dotDiameter + dotDiameter,
          dotDiameter + dotDiameter
        );
        break;
      }
    }
    
    #endregion
    
    innerPath.CloseFigure();
    outerPath.CloseFigure();
    gfx.DrawPath(thick, innerPath);
    gfx.DrawPath(thin, outerPath);

    const double padding = 2;

    XRect contentRect = new
    (
      this.Rect.X + padding,
      this.Rect.Y + padding,
      this.Rect.Width - padding, 
      this.Rect.Height - padding
    );

    this.WithRender(gfx, contentRect);
  }

  #endregion

  #region Abstract Method

  public abstract void WithRender(XGraphics gfx, XRect contentRect);

  #endregion

  #region Private Methods

  private bool EdgeValidation(CornerEnum corner, CornerEnum edge1, CornerEnum edge2) =>
    corner == CornerEnum.Slant && (edge1 == CornerEnum.Curve || edge2 == CornerEnum.Curve);

  #endregion
}