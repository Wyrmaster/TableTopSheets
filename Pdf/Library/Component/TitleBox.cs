using TableTopSheets.Pdf.Extension;
using PdfSharp.Drawing;
using PdfSharp.UniversalAccessibility.Drawing;

namespace TableTopSheets.Pdf.Component;

public class TitleBox: BaseComponent
{
  #region Constants

  private const double X = 13;
  private const double Y = 8.6;
  private const double Width = 292.281;
  private const double Height = 83.05;

  #endregion
  
  #region Field
  
  private readonly string _name;
  private readonly string _background;
  private readonly string _class;
  private readonly string _species;
  private readonly string _subclass;
  private readonly string _level;

  #endregion
  
  #region Constructor

  public TitleBox(string name, string background, string @class, string species, string subclass, string level) : base(new XRect(X, Y, Width, Height))
  {
    this._name = name;
    this._background = background;
    this._class = @class;
    this._species = species;
    this._subclass = subclass;
    _level = level;
  }

  #endregion

  #region Override
  
  protected override void WithRender(XGraphics gfx)
  {
    const double ellipseSize = 9.13;
    gfx.DrawEllipse(this.BaseBrush, X, Y, ellipseSize, ellipseSize);

    const double outerBorderSize = 2;
    const double innerBorderSize = 0.5;
    const double lineSize = 0.4;
    XPen thin = new XPen(BaseColor, innerBorderSize);
    XPen thick = new XPen(BaseColor, outerBorderSize);
    
    XPen line = new XPen(LineColor, lineSize);

    const double outerBoxWidth = 247.92;
    const double outerBottomRightCorner = 11;
    const double outerRadius = 20.05;
    
    const double innerBoxWidth = outerBoxWidth - Constants.OuterMargin;
    const double innerBottomRightCorner = 5.5;
    const double innerRadius = 12;
    
    #region Lines

    const double leftLineIntent = 16;
    gfx.DrawLine
    (
      line,
      new XPoint(X + leftLineIntent, Y + outerRadius + Constants.OuterMargin / 2),
      new XPoint(X + outerBoxWidth, Y + outerRadius + Constants.OuterMargin / 2)
    );
    XFont font = new XFont("Roboto#", 7);

    const double nameRectIntent = 5;
    XRect nameRect = new
    (
      X + leftLineIntent + nameRectIntent, 
      Y + Constants.OuterMargin + outerBorderSize + innerBorderSize + 1,
      X + outerBoxWidth - (X + leftLineIntent) - nameRectIntent - 10,
      15
    );
    gfx.DrawText(this._name, nameRect);

    const string characterName = "CHARACTER NAME";
    XSize size = gfx.MeasureString(characterName, font);
    gfx.DrawString(characterName, font, this.FontBrush, new XPoint(X + leftLineIntent, Y + outerRadius + Constants.OuterMargin / 2 - 2 + size.Height));

    const double innerSpacing = 2.0;

    const double lineSpace = 44;
    double lineWidth = outerBoxWidth; 
    gfx.DrawLine
    (
      line,
      new XPoint(X + leftLineIntent, Y + lineSpace + Constants.OuterMargin / 2),
      new XPoint(X + lineWidth / 2 + innerSpacing * 2.5, Y + lineSpace + Constants.OuterMargin / 2)
    );
    const string background = "BACKGROUND";
    size = gfx.MeasureString(background, font);
    gfx.DrawString(background, font, this.FontBrush, new XPoint(X + leftLineIntent, Y + lineSpace + Constants.OuterMargin / 2 + size.Height - 2));

    XRect backgroundRect = new
    (
      X + leftLineIntent, 
      nameRect.Y + nameRect.Height + 10,
      lineWidth / 2 - innerSpacing * 2.5 - 7.5,
      14
    );
    gfx.DrawText(this._background, backgroundRect);
    
    gfx.DrawLine
    (
      line,
      new XPoint(X + lineWidth / 2 + innerSpacing * 4, Y + lineSpace + Constants.OuterMargin / 2),
      new XPoint(X + outerBoxWidth, Y + lineSpace + Constants.OuterMargin / 2)
    );
    const string @class = "CLASS";
    size = gfx.MeasureString(@class, font);
    gfx.DrawString(@class, font, this.FontBrush, new XPoint(X + lineWidth / 2 + innerSpacing * 4, Y + lineSpace + Constants.OuterMargin / 2 + size.Height - 2));

    XRect classRect = new
    (
      X + lineWidth / 2 + innerSpacing * 4, 
      nameRect.Y + nameRect.Height + 10,
      lineWidth / 2 - innerSpacing * 2.5 - 17,
      14
    );
    gfx.DrawText(this._class, classRect);
    
    const double secondRowMultiplier = 1.55;
    gfx.DrawLine
    (
      line,
      new XPoint(X + leftLineIntent, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2),
      new XPoint(X + lineWidth / 2 + innerSpacing * 2.5, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2)
    );
    const string species = "SPECIES";
    size = gfx.MeasureString(species, font);
    gfx.DrawString(species, font, this.FontBrush, new XPoint(X + leftLineIntent, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2 + size.Height - 2));
    
    XRect speciesRect = new
    (
      X + leftLineIntent, 
      Y + lineSpace * 1.26,
      lineWidth / 2 - innerSpacing * 2.5 - 7.5,
      14
    );
    gfx.DrawText(this._species, speciesRect);
    
    gfx.DrawLine
    (
      line,
      new XPoint(X + lineWidth / 2 + innerSpacing * 4, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2),
      new XPoint(X + outerBoxWidth, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2)
    );
    const string subClass = "SUBCLASS";
    size = gfx.MeasureString(subClass, font);
    gfx.DrawString(subClass, font, this.FontBrush, new XPoint(X + lineWidth / 2 + innerSpacing * 4, Y + lineSpace * secondRowMultiplier + Constants.OuterMargin / 2 + size.Height - 2));

    XRect subClassRect = new
    (
      X + lineWidth / 2 + innerSpacing * 4, 
      Y + lineSpace * 1.26,
      lineWidth / 2 - innerSpacing * 2.5 - 17,
      14
    );
    gfx.DrawText(this._subclass, subClassRect);
    
    #endregion
    
    #region Outer Border
    
    XGraphicsPath outerRing = new XGraphicsPath();
    
    outerRing.AddLine
    (
      X + outerBoxWidth - outerBottomRightCorner,
      Y + Height,
      X,
      Y + Height
    );
    outerRing.AddLine
    (
      X,
      Y + Height,
      X,
      Y + outerRadius
    );
    outerRing.AddArc
    (
      X - outerRadius,
      Y - outerRadius,
      outerRadius + outerRadius,
      outerRadius + outerRadius,
      90,
      -90
    );
    outerRing.AddLine
    (
      X + outerRadius,
      Y,
      X + outerBoxWidth,
      Y
    );
    outerRing.AddLine
    (
      X + outerBoxWidth,
      Y,
      X + outerBoxWidth,
      Y + Height - outerBottomRightCorner
    );
    
    outerRing.CloseFigure();
    
    gfx.DrawPath(thin, outerRing);

    #endregion

    #region Inner Border
    
    XGraphicsPath innerRing = new XGraphicsPath();
    
    innerRing.AddLine
    (
      X + outerBoxWidth - innerBottomRightCorner,
      Y + Height - (innerBorderSize * 1.5),
      X + Constants.OuterMargin,
      Y + Height - (innerBorderSize * 1.5)
    );
    innerRing.AddLine
    (
      X + Constants.OuterMargin,
      Y + Height - (innerBorderSize * 1.5),
      X + Constants.OuterMargin,
      Y + innerRadius + Constants.OuterMargin
    );
    innerRing.AddArc
    (
      X + Constants.OuterMargin - innerRadius,
      Y + Constants.OuterMargin - innerRadius,
      innerRadius + innerRadius,
      innerRadius + innerRadius,
      90,
      -90
    );
    innerRing.AddLine
    (
      X + Constants.OuterMargin + innerRadius,
      Y + Constants.OuterMargin,
      X + outerBoxWidth,
      Y + Constants.OuterMargin
    );
    innerRing.AddLine
    (
      X + outerBoxWidth - (innerBorderSize * 1.5),
      Y + Constants.OuterMargin,
      X + outerBoxWidth - (innerBorderSize * 1.5),
      Y + Height - innerBottomRightCorner
    );
    
    innerRing.CloseFigure();
    
    gfx.DrawPath(thick, innerRing);

    #endregion

    #region Small Corner

    gfx.DrawPolygon
    (
      this.BaseBrush,
      [
        new XPoint(X + outerBoxWidth + 1.5, Y + Height + 0.35),
        new XPoint(X + outerBoxWidth - outerBottomRightCorner, Y + Height + 0.35),
        new XPoint(X + outerBoxWidth - 2.5, Y + Height - 2.5),
      ],
      XFillMode.Winding
    );

    #endregion

    #region Left Bezier

    XGraphicsPath leftBezier = new();
    
    leftBezier.AddBezier
    (
      new (X + 6.14, Y + 14.217),
      new (X + 6.464, Y + 21.742),
      new (X + 6.4, Y + 28.765),
      new (X + 14.17, Y + 34.637)
    );
    leftBezier.AddBezier
    (
      new (X + 14.17, Y + 34.637),
      new (X + 4.425, Y + 53.604),
      new (X + 7.097, Y + 75.286),
      new (X + 6.011, Y + 81.826)
    );
    
    gfx.DrawPath(thin, leftBezier);

    #endregion

    #region Level Circle

    const double ellipseX = X + 235;
    const double ellipseY = Y + 3;
    const double ellipseWidth = 57.41;
    const double ellipseHeight = 77.33;
    gfx.DrawEllipse
    (
      thick,
      XBrushes.White,
      new XRect
      (
        ellipseX,
        ellipseY,
        ellipseWidth,
        ellipseHeight
      )
    );

    const double ellipseYCorrection = 2;
    const double ellipseXCorrection = 2;
    
    gfx.DrawBezier
    (
      thin,
      new XPoint(ellipseX - ellipseXCorrection + 9.298893, ellipseY + 13.431735),
      new XPoint(ellipseX - ellipseXCorrection + 20.077325, ellipseY + 32.166366),
      new XPoint(ellipseX - ellipseXCorrection + 7.753499, ellipseY + 36.437599),
      new XPoint(ellipseX - ellipseXCorrection + 3.8376384, ellipseY + 51.512916)
    );
    gfx.DrawBezier
    (
      thin,
      new XPoint(ellipseX + ellipseXCorrection + 48, ellipseY + 13.284134),
      new XPoint(ellipseX + ellipseXCorrection + 37.81861, ellipseY + 32.018765),
      new XPoint(ellipseX + ellipseXCorrection + 50.142436, ellipseY + 36.289998),
      new XPoint(ellipseX + ellipseXCorrection + 54.058296, ellipseY + 51.365315)
    );
    gfx.DrawBezier
    (
      thin,
      new XPoint(ellipseX + 26.715867, ellipseY - ellipseYCorrection + 3.3948339),
      new XPoint(ellipseX - ellipseXCorrection + 11.365314, ellipseY + 11.660517),
      new XPoint(ellipseX - ellipseXCorrection + 7.9704797, ellipseY + 30.99631),
      new XPoint(ellipseX - ellipseXCorrection + 7.5276753, ellipseY + 41.918819)
    );
    gfx.DrawBezier
    (
      thin,
      new XPoint(ellipseX + 30.554394, ellipseY - ellipseYCorrection + 3.4070252),
      new XPoint(ellipseX + ellipseXCorrection + 45.904947, ellipseY + 11.672709),
      new XPoint(ellipseX + ellipseXCorrection + 49.594984, ellipseY + 31.008502),
      new XPoint(ellipseX + ellipseXCorrection + 50.037788, ellipseY + 41.931011)
    );
    
    gfx.DrawPolygon
    (
      thin,
      [
        new XPoint(ellipseX + ellipseWidth / 2, ellipseY + 5),
        new XPoint(ellipseX + ellipseWidth / 2 - 3, ellipseY),
        new XPoint(ellipseX + ellipseWidth / 2 + 3, ellipseY),
      ]
    );

    const double levelLine = 13;
    gfx.DrawLine
    (
      line,
      new XPoint(ellipseX + ellipseWidth / 2 - levelLine, ellipseY + ellipseHeight * 0.65),
      new XPoint(ellipseX + ellipseWidth / 2 + levelLine, ellipseY + ellipseHeight * 0.65)
    );

    const string level = "LEVEL";
    size = gfx.MeasureString(level, font);
    gfx.DrawString(level, font, this.FontBrush, new XPoint(ellipseX + ellipseWidth / 2 - size.Width / 2, ellipseY + ellipseHeight * 0.65 + size.Height - 2));

    const double levelRectHeight = 30;
    XRect levelRect = new
    (
      ellipseX + ellipseWidth / 2 - levelLine + 0.75,
      ellipseY + ellipseHeight * 0.65 - levelRectHeight,
      levelLine + levelLine,
      levelRectHeight
    );
    gfx.DrawText(this._level, levelRect);

    #endregion
  }

  #endregion
}