using TableTopSheets.Pdf.Interface;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public abstract class BaseComponent: IComponent
{
  #region Field

  protected readonly XColor BaseColor = XColor.FromArgb(170, 171, 174);
  protected readonly XBrush BaseBrush = new XSolidBrush(XColor.FromArgb(170, 171, 174));
  
  protected readonly XColor LineColor = XColor.FromArgb(179, 180, 182);
  protected readonly XColor FontColor = XColors.Black;
  protected readonly XBrush FontBrush = XBrushes.Black;

  #endregion

  #region Constructor

  protected BaseComponent(XRect rect)
  {
    this.Rect = rect;
  }

  #endregion

  #region Property

  public bool Debug { get; set; }

  public XRect Rect { get; protected set; }
  
  #endregion

  #region Public Methods

  public void Draw(XGraphics gfx)
  {
    if (Debug)
    {
      XPen debugPen = new XPen(XColors.LightGreen);
      gfx.DrawRectangle(debugPen, this.Rect);
    }
    
    this.WithRender(gfx);
  }

  #endregion

  #region Abstract Methods

  protected abstract void WithRender(XGraphics gfx);

  #endregion
}