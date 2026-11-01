namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Line of a <see cref="RichTextSlice"/> at its vertical offset; <see cref="Repeated"/> marks a table header drawn again
///   at the top of a continuation
/// </summary>
public sealed record RichTextSliceLine(RichTextLine Line, double Y, bool Repeated);
