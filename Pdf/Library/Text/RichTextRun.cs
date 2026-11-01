using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Piece of a line drawn with one font; <see cref="X"/> and <see cref="Baseline"/> are relative to the top left of its
///   <see cref="RichTextLine"/>. The text keeps the space in front of a word, so the runs of a line joined give its text.
///   <see cref="StartsLine"/> marks the first run of a wrapped line (of a table cell, too).
/// </summary>
public sealed record RichTextRun(string Text, XFont Font, double X, double Baseline, double Width, bool StartsLine);
