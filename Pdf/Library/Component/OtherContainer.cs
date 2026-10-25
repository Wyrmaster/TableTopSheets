using TableTopSheets.Pdf.Component.Generic;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

public class OtherContainer: BaseComponent
{
  #region Constants

  private const double X = 245.04;
  private const double Y = 128.88;
  private const double Width = 341;
  private const double Height = 48.69;

  #endregion
  
  #region Fields
  
  private readonly string _initiative;
  private readonly string _speed;
  private readonly string _size;
  private readonly string _passivePerception;
  
  #endregion
  
  #region Constructor
  
  public OtherContainer(string initiative, string speed, string size, string passivePerception)
    : base(new XRect(X, Y, Width, Height))
  {
    this._initiative = initiative;
    this._speed = speed;
    this._size = size;
    this._passivePerception = passivePerception;
  }

  #endregion

  #region Override

  protected override void WithRender(XGraphics gfx)
  {
    const double outerBorderSize = 2;
    const double spacing = 10;
    const double width = (Width - 3 * spacing)/ 4 - outerBorderSize;
    
    new OtherBox(this._initiative, "INITIATIVE", new (X + outerBorderSize / 2, Y, width, Height)).Draw(gfx); 
    new OtherBox($"{this._speed} ft.", "SPEED", new(X + outerBorderSize / 2 + width * 1 + spacing * 1, Y, width, Height)).Draw(gfx);
    new OtherBox(this._size, "SIZE", new(X + outerBorderSize / 2 + width * 2 + spacing * 2, Y, width, Height)).Draw(gfx);
    new OtherBox(this._passivePerception, "PASSIVE PERCEPTION", new(X + outerBorderSize / 2 + width * 3 + spacing * 3, Y, width, Height)).Draw(gfx);
  }

  #endregion
}