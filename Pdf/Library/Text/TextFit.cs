using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Measures, wraps and shrinks text so it fits into one or more rectangles.
///   <see cref="Draw"/> draws exactly the lines that were measured, so nothing is clipped.
/// </summary>
public static class TextFit
{
  #region Constants

  /// <summary>
  ///   Font family used for all text
  /// </summary>
  public const string Family = "Roboto#";

  /// <summary>
  ///   Largest size of body text
  /// </summary>
  public const double BodyMaxSize = 8;

  /// <summary>
  ///   Smallest size body text shrinks to before it overflows
  /// </summary>
  public const double BodyMinSize = 5.5;

  /// <summary>
  ///   Smallest size of single line cells
  /// </summary>
  public const double CellMinSize = 6;

  /// <summary>
  ///   Size step used while shrinking
  /// </summary>
  public const double Step = 0.25;

  /// <summary>
  ///   Marker that ends a line whose text continues elsewhere
  /// </summary>
  public const string Ellipsis = "…";

  private const double Epsilon = 0.0001;

  #endregion

  #region Public Methods

  /// <summary>
  ///   Height of one line of <paramref name="font"/>
  /// </summary>
  public static double LineHeight(XFont font) => font.GetHeight();

  /// <summary>
  ///   Word wrap a text into lines of at most <paramref name="width"/>; paragraphs are separated by <c>\n</c>
  /// </summary>
  public static List<string> Wrap(XGraphics gfx, string text, XFont font, double width)
  {
    LineBreaker breaker = new(gfx, font, text);
    List<string> lines = [];
    while (breaker.Next(width) is { } line)
    {
      lines.Add(line);
    }

    return lines;
  }

  /// <summary>
  ///   Find the largest font size between <paramref name="maxSize"/> and <paramref name="minSize"/> at which the text fits into
  ///   <paramref name="rects"/> (filled in order, like columns). When it doesn't fit at <paramref name="minSize"/> the rectangles are
  ///   filled at that size, the last line ends with <see cref="Ellipsis"/> and the rest is returned as <see cref="TextFitResult.Overflow"/>.
  /// </summary>
  public static TextFitResult Fit(XGraphics gfx, string text, string family, XFontStyleEx style, XRect[] rects, double maxSize, double minSize, double step = Step)
  {
    for (double size = maxSize; size >= minSize - Epsilon; size -= step)
    {
      XFont font = new(family, size, style);
      LineBreaker breaker = new(gfx, font, text);
      List<IReadOnlyList<string>> lines = Fill(breaker, rects, font, ellipsis: null, gfx);
      if (breaker.Done)
      {
        return new(font, lines, null);
      }
    }

    XFont minFont = new(family, minSize, style);
    LineBreaker truncated = new(gfx, minFont, text);
    List<IReadOnlyList<string>> truncatedLines = Fill(truncated, rects, minFont, Ellipsis, gfx);
    return new(minFont, truncatedLines, truncated.Rest());
  }

  /// <summary>
  ///   Draw the lines of a <see cref="Fit"/> result into the same rectangles
  /// </summary>
  public static void Draw(XGraphics gfx, TextFitResult result, XBrush brush, XRect[] rects, XParagraphAlignment alignment = XParagraphAlignment.Left)
  {
    double lineHeight = result.LineHeight;
    for (int index = 0; index < rects.Length && index < result.Lines.Count; index++)
    {
      XRect rect = rects[index];
      IReadOnlyList<string> lines = result.Lines[index];
      for (int line = 0; line < lines.Count; line++)
      {
        DrawLine(gfx, lines[line], result.Font, brush, rect with { Y = rect.Y + line * lineHeight, Height = lineHeight }, alignment);
      }
    }
  }

  /// <summary>
  ///   Shrink a one line text from <paramref name="maxSize"/> to <paramref name="minSize"/> until it fits <paramref name="width"/>
  ///   (and <paramref name="maxHeight"/>); at <paramref name="minSize"/> it is cut and ends with <see cref="Ellipsis"/>.
  ///   <c>Truncated</c> tells whether text was cut.
  /// </summary>
  public static (XFont Font, string Text, bool Truncated) FitSingleLine(XGraphics gfx, string text, string family, XFontStyleEx style, double maxSize, double minSize, double width, double maxHeight = double.MaxValue)
  {
    string line = string.Join(" ", text
      .Replace("\r", " ")
      .Replace("\n", " ")
      .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    for (double size = maxSize; size >= minSize - Epsilon; size -= Step)
    {
      XFont font = new(family, size, style);
      if (gfx.MeasureString(line, font).Width <= width && LineHeight(font) <= maxHeight + Epsilon)
      {
        return (font, line, false);
      }
    }

    XFont minFont = new(family, minSize, style);
    string truncated = Truncate(gfx, line, minFont, width);
    return (minFont, truncated, truncated != line);
  }

  /// <summary>
  ///   Draw one line inside <paramref name="rect"/>, top aligned
  /// </summary>
  public static void DrawLine(XGraphics gfx, string text, XFont font, XBrush brush, XRect rect, XParagraphAlignment alignment = XParagraphAlignment.Left)
  {
    if (text.Length == 0)
    {
      return;
    }

    double x = rect.X;
    if (alignment is XParagraphAlignment.Center or XParagraphAlignment.Right)
    {
      double width = gfx.MeasureString(text, font).Width;
      x = alignment == XParagraphAlignment.Center ? rect.X + (rect.Width - width) / 2 : rect.X + rect.Width - width;
    }

    gfx.DrawString(text, font, brush, x, rect.Y, XStringFormats.TopLeft);
  }

  #endregion

  #region Private Methods

  private static List<IReadOnlyList<string>> Fill(LineBreaker breaker, XRect[] rects, XFont font, string? ellipsis, XGraphics gfx)
  {
    double lineHeight = LineHeight(font);
    int[] capacity = rects
      .Select(rect => Math.Max(0, (int)Math.Floor((rect.Height + Epsilon) / lineHeight)))
      .ToArray();
    int lastRect = Array.FindLastIndex(capacity, entry => entry > 0);

    List<IReadOnlyList<string>> result = [];
    for (int index = 0; index < rects.Length; index++)
    {
      List<string> lines = [];
      result.Add(lines);
      for (int line = 0; line < capacity[index] && !breaker.Done; line++)
      {
        bool lastSlot = ellipsis != null && index == lastRect && line == capacity[index] - 1;
        if (!lastSlot)
        {
          lines.Add(breaker.Next(rects[index].Width)!);
          continue;
        }

        (int, int, int) position = breaker.Position;
        string last = breaker.Next(rects[index].Width)!;
        if (!breaker.Done)
        {
          // text continues: take the line again with room for the marker
          breaker.Restore(position);
          double room = Math.Max(0, rects[index].Width - gfx.MeasureString(ellipsis!, font).Width);
          last = (breaker.Next(room) ?? string.Empty).TrimEnd() + ellipsis;
        }

        lines.Add(last);
      }
    }

    return result;
  }

  private static string Truncate(XGraphics gfx, string text, XFont font, double width)
  {
    if (gfx.MeasureString(text, font).Width <= width)
    {
      return text;
    }

    for (int length = text.Length - 1; length > 0; length--)
    {
      string candidate = text[..length].TrimEnd() + Ellipsis;
      if (gfx.MeasureString(candidate, font).Width <= width)
      {
        return candidate;
      }
    }

    return Ellipsis;
  }

  #endregion
}
