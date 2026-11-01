using PdfSharp.Drawing;
using TableTopSheets.Pdf.Enum;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   Dimensions of a <see cref="CardSizeEnum"/> (all in points): the trimmed card, its text columns and font sizes. The
///   small card is portrait with one column, the medium and large cards are landscape with two and three columns.
/// </summary>
public sealed class CardFormat
{
  #region Constructor

  private CardFormat(CardSizeEnum size, double widthMm, double heightMm, double imageMm, int textColumns, double frameInset,
    double padding, double titleSize, double subtitleSize, double factsSize, double footerSize, double maxBodySize)
  {
    this.Size = size;
    this.Width = XUnit.FromMillimeter(widthMm).Point;
    this.Height = XUnit.FromMillimeter(heightMm).Point;
    this.ImageSize = XUnit.FromMillimeter(imageMm).Point;
    this.TextColumns = textColumns;
    this.FrameInset = frameInset;
    this.Padding = padding;
    this.TitleSize = titleSize;
    this.SubtitleSize = subtitleSize;
    this.FactsSize = factsSize;
    this.FooterSize = footerSize;
    this.MaxBodySize = maxBodySize;
  }

  #endregion

  #region Property

  /// <summary>
  ///   Poker card, 63 × 88 mm portrait, one column
  /// </summary>
  public static CardFormat Small { get; } = new(CardSizeEnum.Small, 63, 88, 22, 1, 5, 4.5, 10, 6.5, 7, 5.5, 9);

  /// <summary>
  ///   138 × 98 mm landscape, two columns
  /// </summary>
  public static CardFormat Medium { get; } = new(CardSizeEnum.Medium, 138, 98, 34, 2, 7, 5, 13, 8, 7.5, 6, 10);

  /// <summary>
  ///   200 × 140 mm landscape, three columns
  /// </summary>
  public static CardFormat Large { get; } = new(CardSizeEnum.Large, 200, 140, 42, 3, 8, 6, 15, 9, 8, 6.5, 10);

  /// <summary>
  ///   Every format from the smallest to the largest
  /// </summary>
  public static IReadOnlyList<CardFormat> All { get; } = [Small, Medium, Large];

  public CardSizeEnum Size { get; }

  /// <summary>
  ///   Trimmed width of the card
  /// </summary>
  public double Width { get; }

  /// <summary>
  ///   Trimmed height of the card
  /// </summary>
  public double Height { get; }

  /// <summary>
  ///   The card is wider than tall
  /// </summary>
  public bool Landscape => this.Width > this.Height;

  /// <summary>
  ///   Height of the picture: the side of a square on a portrait card, a banner as wide as the first column on a
  ///   landscape card
  /// </summary>
  public double ImageSize { get; }

  /// <summary>
  ///   Text columns side by side
  /// </summary>
  public int TextColumns { get; }

  /// <summary>
  ///   Distance of the frame from the cut edge, so the frame survives an imprecise cut
  /// </summary>
  public double FrameInset { get; }

  /// <summary>
  ///   Distance of the content from the frame; twice of it separates the text columns
  /// </summary>
  public double Padding { get; }

  /// <summary>
  ///   Largest size of the name
  /// </summary>
  public double TitleSize { get; }

  /// <summary>
  ///   Largest size of the line under the name
  /// </summary>
  public double SubtitleSize { get; }

  /// <summary>
  ///   Size of the metadata table: fixed on a landscape card, the largest size on a portrait card (where it follows a
  ///   smaller body)
  /// </summary>
  public double FactsSize { get; }

  /// <summary>
  ///   Size of the footer line
  /// </summary>
  public double FooterSize { get; }

  /// <summary>
  ///   Largest body size the description grows to when the card has room
  /// </summary>
  public double MaxBodySize { get; }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Format of a size
  /// </summary>
  public static CardFormat Of(CardSizeEnum size) => size switch
  {
    CardSizeEnum.Small => Small,
    CardSizeEnum.Medium => Medium,
    _ => Large,
  };

  #endregion
}
