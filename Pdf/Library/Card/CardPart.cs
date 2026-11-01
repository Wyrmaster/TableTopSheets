using PdfSharp.Drawing;
using TableTopSheets.Common.Card;
using TableTopSheets.Pdf.Text;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   One printed card: a <see cref="CardData"/> in a size with the part of its description that fits. A description that
///   doesn't fit a large card continues on further parts.
/// </summary>
/// <param name="Data">Content of the card</param>
/// <param name="Geometry">Positions of everything on the card</param>
/// <param name="Columns">Lines of the description on this card, one slice per text column of the geometry</param>
/// <param name="Image">Decoded picture; <c>null</c> when there is none or it couldn't be read</param>
/// <param name="Part">Number of this card, from 1</param>
/// <param name="Parts">Number of cards of <paramref name="Data"/></param>
public sealed record CardPart(CardData Data, CardGeometry Geometry, IReadOnlyList<RichTextSlice> Columns, XImage? Image, int Part, int Parts)
{
  #region Property

  public CardFormat Format => this.Geometry.Format;

  /// <summary>
  ///   Lines of the description on this card in reading order, including repeated table headers
  /// </summary>
  public IEnumerable<RichTextSliceLine> Lines => this.Columns.SelectMany(column => column.Lines);

  /// <summary>
  ///   Marker at the right of the footer: <c>1/2</c> on the first card, <c>continued 2/2</c> on the others; empty for a single card
  /// </summary>
  public string PartLabel => this.Parts <= 1 ? string.Empty : this.Part == 1 ? $"1/{this.Parts}" : $"continued {this.Part}/{this.Parts}";

  #endregion
}
