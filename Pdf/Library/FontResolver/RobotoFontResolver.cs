using System.Reflection;
using PdfSharp.Fonts;

namespace TableTopSheets.Pdf.FontResolver;

public class RobotoFontResolver: IFontResolver
{
  #region Field

  private readonly Dictionary<string, byte[]> _fonts = new();

  #endregion

  #region RobotoFontResolver

  public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
  {
    var name = familyName.ToLower().Trim('#');

    // Deal with the fonts we know.
    switch (name)
    {
      case "roboto" when isBold && isItalic:
      {
        return new FontResolverInfo("Roboto#bi");
      }
      case "roboto" when isBold:
      {
        return new FontResolverInfo("Roboto#b");
      }
      case "roboto" when isItalic:
      {
        return new FontResolverInfo("Roboto#i");
      }
      case "roboto":
      {
        return new FontResolverInfo("Roboto#");
      }
    }

    return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic)!;
  }

  public byte[] GetFont(string faceName)
  {
    if (!this._fonts.ContainsKey(faceName))
    {
      switch (faceName)
      {
        case "Roboto#":
        {
          const string keyString = "TableTopSheets.Pdf.Resources.Sarabun.SarabunLight.ttf";
          this.LoadFontFromAssembly(faceName, keyString);
          break;
        }
        case "Roboto#b":
        {
          const string keyString = "TableTopSheets.Pdf.Resources.Sarabun.SarabunRegular.ttf";
          this.LoadFontFromAssembly(faceName, keyString);
          break;
        }
        case "Roboto#i":
        {
          const string keyString = "TableTopSheets.Pdf.Resources.Roboto.RobotoLightItalic.ttf";
          this.LoadFontFromAssembly(faceName, keyString);
          break;
        }
        case "Roboto#bi":
        {
          const string keyString = "TableTopSheets.Pdf.Resources.Roboto.RobotoItalic.ttf";
          this.LoadFontFromAssembly(faceName, keyString);
          break;
        }
      }
    }

    return this._fonts[faceName];
  }

  private void LoadFontFromAssembly(string faceName, string keyString)
  {
    Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(keyString);
    byte[] data = new byte[stream!.Length];

    stream.Read(data, 0, data.Length);

    this._fonts[faceName] = data;
  }

  #endregion
}