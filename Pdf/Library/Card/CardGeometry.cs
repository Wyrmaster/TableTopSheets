using PdfSharp.Drawing;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   Where everything of a card goes, relative to the top left of the trimmed card: frame, title, rule, picture, metadata
///   table, the text columns of the description and footer. Built once while choosing the size and used again for drawing.
/// </summary>
public sealed class CardGeometry
{
  #region Constants

  /// <summary>
  ///   Smallest size the name and the subtitle shrink to
  /// </summary>
  private const double TitleMinSize = 6;

  /// <summary>
  ///   Thickness of the rule under the title
  /// </summary>
  public const double RuleSize = 1.2;

  /// <summary>
  ///   Room left for the rounded and slanted corners of the frame
  /// </summary>
  private const double CornerRoom = 8;

  /// <summary>
  ///   Size of the metadata table relative to the body size
  /// </summary>
  private const double FactsScale = 0.95;

  #endregion

  #region Constructor

  private CardGeometry(CardFormat format, double bodySize)
  {
    this.Format = format;
    this.BodySize = bodySize;
  }

  #endregion

  #region Property

  public CardFormat Format { get; }

  /// <summary>
  ///   Size of the description text
  /// </summary>
  public double BodySize { get; }

  /// <summary>
  ///   Rectangle handed to the frame
  /// </summary>
  public XRect Frame { get; private init; }

  public XFont TitleFont { get; private init; } = null!;

  /// <summary>
  ///   Name, shrunk (or cut) to one line
  /// </summary>
  public string Title { get; private init; } = string.Empty;

  public XRect TitleRect { get; private init; }

  public XFont? SubtitleFont { get; private init; }

  public string? Subtitle { get; private init; }

  public XRect SubtitleRect { get; private init; }

  /// <summary>
  ///   Left end of the rule under the title; it is <see cref="RuleWidth"/> long
  /// </summary>
  public XPoint Rule { get; private init; }

  public double RuleWidth { get; private init; }

  /// <summary>
  ///   Picture (a square on a portrait card, a banner on a landscape card); <c>null</c> without picture and on continuation cards
  /// </summary>
  public XRect? ImageRect { get; private init; }

  /// <summary>
  ///   Laid out metadata table; <c>null</c> without facts and on continuation cards
  /// </summary>
  public RichTextSlice? Facts { get; private init; }

  public XPoint FactsOrigin { get; private init; }

  /// <summary>
  ///   Text columns of the description, left to right; on a first card the first column starts below the picture and
  ///   the metadata table
  /// </summary>
  public IReadOnlyList<XRect> Columns { get; private init; } = [];

  /// <summary>
  ///   Width of every text column
  /// </summary>
  public double ColumnWidth => this.Columns[0].Width;

  public XFont FooterFont { get; private init; } = null!;

  public XRect FooterRect { get; private init; }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Geometry of <paramref name="data"/> on a card of <paramref name="format"/>. A continuation card has no picture and
  ///   no metadata table, so its first column is as tall as the others.
  /// </summary>
  public static CardGeometry Create(XGraphics gfx, CardData data, CardFormat format, double bodySize, bool hasImage, bool continuation)
  {
    double inset = format.FrameInset;
    double padding = format.Padding;
    double left = inset + padding;
    double right = format.Width - inset - padding;
    double width = right - left;
    double gap = padding * 0.75;

    double y = inset + padding * 0.5;
    (XFont titleFont, string title, _) = TextFit.FitSingleLine(gfx, data.Name, TextFit.Family, XFontStyleEx.Bold, format.TitleSize,
      TitleMinSize, width - 2 * CornerRoom);
    XRect titleRect = new(left, y, width, TextFit.LineHeight(titleFont));
    y += titleRect.Height;

    XFont? subtitleFont = null;
    string? subtitle = null;
    XRect subtitleRect = default;
    if (!string.IsNullOrWhiteSpace(data.Subtitle))
    {
      (subtitleFont, subtitle, _) = TextFit.FitSingleLine(gfx, data.Subtitle, TextFit.Family, XFontStyleEx.Italic, format.SubtitleSize,
        TitleMinSize, width - 2 * CornerRoom);
      subtitleRect = new(left, y, width, TextFit.LineHeight(subtitleFont));
      y += subtitleRect.Height;
    }

    y += 1.5;
    XPoint rule = new(left, y);
    y += RuleSize + gap;

    XFont footerFont = new(TextFit.Family, format.FooterSize, XFontStyleEx.Regular);
    double footerHeight = TextFit.LineHeight(footerFont);
    XRect footerRect = new(left, format.Height - inset - padding * 0.5 - footerHeight, width, footerHeight);
    double bottom = footerRect.Y - gap * 0.5;

    int count = format.TextColumns;
    double columnGap = padding * 2;
    double columnWidth = (width - (count - 1) * columnGap) / count;
    double top = y;

    XRect? imageRect = null;
    RichTextSlice? facts = null;
    XPoint factsOrigin = default;
    bool header = !continuation && (hasImage || data.Facts.Count > 0);
    if (header && !format.Landscape)
    {
      // portrait: the table beside the square picture, both above the column
      double imageSide = hasImage ? format.ImageSize : 0;
      double factsLeft = hasImage ? left + imageSide + gap : left;
      if (data.Facts.Count > 0)
      {
        double factsSize = Math.Min(bodySize * FactsScale, format.FactsSize);
        facts = RichTextLayout
          .Create(gfx, [FactsTable(data.Facts)], right - factsLeft, factsSize, RichTextOptions.Facts)
          .All();
      }

      double factsHeight = facts?.Height ?? 0;
      double rowHeight = Math.Max(imageSide, factsHeight);
      if (hasImage)
      {
        imageRect = new XRect(left, y, imageSide, imageSide);
      }

      // the table sits in the middle of a taller picture
      factsOrigin = new(factsLeft, y + (rowHeight - factsHeight) / 2);
      y += rowHeight + gap;
    }
    else if (header)
    {
      // landscape: the picture as a banner over the first column, the table below it
      if (hasImage)
      {
        imageRect = new XRect(left, y, columnWidth, format.ImageSize);
        y += format.ImageSize + gap;
      }

      if (data.Facts.Count > 0)
      {
        facts = RichTextLayout
          .Create(gfx, [FactsTable(data.Facts)], columnWidth, format.FactsSize, RichTextOptions.Facts)
          .All();
        factsOrigin = new(left, y);
        y += facts.Height + gap;
      }
    }

    List<XRect> columns = Enumerable
      .Range(0, count)
      .Select(index => index == 0
        ? new XRect(left, y, columnWidth, Math.Max(0, bottom - y))
        : new XRect(left + index * (columnWidth + columnGap), top, columnWidth, Math.Max(0, bottom - top)))
      .ToList();

    return new(format, bodySize)
    {
      // the frame moves its rectangle half a point up and left and makes it a point smaller: this centers it on the inset
      Frame = new(inset + 0.5, inset + 0.5, format.Width - 2 * inset + 1, format.Height - 2 * inset + 1),
      TitleFont = titleFont,
      Title = title,
      TitleRect = titleRect,
      SubtitleFont = subtitleFont,
      Subtitle = subtitle,
      SubtitleRect = subtitleRect,
      Rule = rule,
      RuleWidth = width,
      ImageRect = imageRect,
      Facts = facts,
      FactsOrigin = factsOrigin,
      Columns = columns,
      FooterFont = footerFont,
      FooterRect = footerRect,
    };
  }

  #endregion

  #region Private Methods

  private static TextBlock FactsTable(IReadOnlyList<CardFact> facts) => new(TextBlockKindEnum.Table)
  {
    Rows = facts
      .Select(fact => (IReadOnlyList<IReadOnlyList<TextSpan>>)[[new TextSpan(fact.Label, Bold: true)], [new TextSpan(fact.Value)]])
      .ToList(),
  };

  #endregion
}
