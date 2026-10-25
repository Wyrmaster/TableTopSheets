using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Interface;

public interface IComponent
{
  void Draw(XGraphics gfx);
}