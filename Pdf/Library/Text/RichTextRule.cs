namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Thin line (table border) relative to the top left of its <see cref="RichTextLine"/>
/// </summary>
public sealed record RichTextRule(double X1, double Y1, double X2, double Y2);
