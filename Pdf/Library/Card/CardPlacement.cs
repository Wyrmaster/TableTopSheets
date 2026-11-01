using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   Where a <see cref="CardPart"/> sits on an A4 page
/// </summary>
/// <param name="Part">The card</param>
/// <param name="Bounds">Trimmed card on the page in points; width and height are swapped when <paramref name="Rotated"/></param>
/// <param name="Rotated">The card is turned by 90° clockwise</param>
public sealed record CardPlacement(CardPart Part, XRect Bounds, bool Rotated);
