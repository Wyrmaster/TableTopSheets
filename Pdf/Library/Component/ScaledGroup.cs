using TableTopSheets.Pdf.Interface;
using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Component;

/// <summary>
///   Draws components laid out for the full page shrunk by <c>scale</c>, with their point (0, 0) moved to <c>origin</c>
/// </summary>
public class ScaledGroup: IComponent
{
  #region Fields

  private readonly XPoint _origin;
  private readonly double _scale;
  private readonly IComponent[] _components;

  #endregion

  #region Constructor

  public ScaledGroup(XPoint origin, double scale, params IComponent[] components)
  {
    this._origin = origin;
    this._scale = scale;
    this._components = components;
  }

  #endregion

  #region Public Methods

  public void Draw(XGraphics gfx)
  {
    XGraphicsState state = gfx.Save();
    gfx.TranslateTransform(this._origin.X, this._origin.Y);
    gfx.ScaleTransform(this._scale);
    foreach (IComponent component in this._components)
    {
      component.Draw(gfx);
    }

    gfx.Restore(state);
  }

  #endregion
}
