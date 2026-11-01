using System.Net;
using System.Text.RegularExpressions;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Engine.Sheet;

public static partial class HtmlText
{
  #region Methods

  /// <summary>
  ///   Plain text of a html description (paragraphs, headings and table rows become new lines)
  /// </summary>
  public static string ToPlain(string html)
  {
    string text = TableRegex().Replace(html, match => Table(match.Groups[1].Value));
    text = BreakRegex().Replace(text, "\n");
    text = TagRegex().Replace(text, string.Empty);
    text = WebUtility.HtmlDecode(text);
    return MultiNewLineRegex()
      .Replace(text, "\n")
      .Trim();
  }

  /// <summary>
  ///   Structure of a html description: paragraphs, headings, lists and tables with bold and italic text; text outside of a
  ///   block becomes a paragraph
  /// </summary>
  public static IReadOnlyList<TextBlock> ToBlocks(string? html)
  {
    List<TextBlock> blocks = [];
    if (string.IsNullOrWhiteSpace(html))
    {
      return blocks;
    }

    void Loose(string text)
    {
      List<TextSpan> spans = Spans(text);
      if (spans.Count > 0)
      {
        blocks.Add(new TextBlock(TextBlockKindEnum.Paragraph) { Spans = spans });
      }
    }

    int position = 0;
    foreach (Match block in BlockRegex().Matches(html))
    {
      Loose(html[position..block.Index]);
      position = block.Index + block.Length;
      string tag = block.Groups[1].Value.ToLowerInvariant();
      string content = block.Groups[2].Value;
      switch (tag)
      {
        case "table":
          blocks.Add(TableBlock(content));
          break;
        case "ul" or "ol":
          blocks.Add(new TextBlock(TextBlockKindEnum.List)
          {
            Items = ItemRegex()
              .Matches(content)
              .Select(item => (IReadOnlyList<TextSpan>)Spans(item.Groups[1].Value))
              .ToList(),
          });
          break;
        case "p":
          Loose(content);
          break;
        default:
          blocks.Add(new TextBlock(TextBlockKindEnum.Heading) { Spans = Spans(content) });
          break;
      }
    }

    Loose(html[position..]);
    return blocks;
  }

  private static TextBlock TableBlock(string table)
  {
    List<(bool Header, IReadOnlyList<IReadOnlyList<TextSpan>> Cells)> rows = RowRegex()
      .Matches(table)
      .Select(row => CellRegex()
        .Matches(row.Groups[1].Value)
        .ToList())
      .Where(cells => cells.Count > 0)
      .Select(cells => (cells.All(cell => cell.Groups[1].Value.Equals("h", StringComparison.OrdinalIgnoreCase)), (IReadOnlyList<IReadOnlyList<TextSpan>>)cells
        .Select(cell => (IReadOnlyList<TextSpan>)Spans(cell.Groups[2].Value))
        .ToList()))
      .ToList();
    return new TextBlock(TextBlockKindEnum.Table)
    {
      Rows = rows
        .Select(row => row.Cells)
        .ToList(),
      HeaderRows = rows
        .TakeWhile(row => row.Header)
        .Count(),
    };
  }

  /// <summary>
  ///   Text runs of inline html; <c>strong</c>/<c>b</c> and <c>em</c>/<c>i</c> set the style, other tags are dropped and white
  ///   space is collapsed
  /// </summary>
  private static List<TextSpan> Spans(string html)
  {
    List<TextSpan> spans = [];
    int bold = 0;
    int italic = 0;
    foreach (Match token in InlineRegex().Matches(html))
    {
      if (token.Groups["tag"].Success)
      {
        int change = token.Groups["close"].Value.Length > 0 ? -1 : 1;
        switch (token.Groups["tag"].Value.ToLowerInvariant())
        {
          case "strong" or "b":
            bold = Math.Max(0, bold + change);
            break;
          case "em" or "i":
            italic = Math.Max(0, italic + change);
            break;
          case "br":
            Append(spans, "\n", bold > 0, italic > 0);
            break;
        }

        continue;
      }

      Append(spans, WhiteSpaceRegex().Replace(WebUtility.HtmlDecode(token.Value), " "), bold > 0, italic > 0);
    }

    // no white space at the ends of the block
    if (spans.Count > 0)
    {
      spans[0] = spans[0] with { Text = spans[0].Text.TrimStart() };
      spans[^1] = spans[^1] with { Text = spans[^1].Text.TrimEnd() };
    }

    return spans
      .Where(span => span.Text.Length > 0)
      .ToList();
  }

  private static void Append(List<TextSpan> spans, string text, bool bold, bool italic)
  {
    if (text.Length == 0)
    {
      return;
    }

    if (spans.Count > 0 && spans[^1].Bold == bold && spans[^1].Italic == italic)
    {
      spans[^1] = spans[^1] with { Text = spans[^1].Text + text };
      return;
    }

    spans.Add(new TextSpan(text, bold, italic));
  }

  /// <summary>
  ///   One line per row: <c>Black: Acid</c> for two columns, <c>Abyssal – Level 1: …; Level 3: …</c> for more;
  ///   the header row only labels the cells
  /// </summary>
  private static string Table(string table)
  {
    List<(bool Header, List<string> Cells)> rows = RowRegex()
      .Matches(table)
      .Select(row => CellRegex()
        .Matches(row.Groups[1].Value)
        .ToList())
      .Where(cells => cells.Count > 0)
      .Select(cells => (cells.All(cell => cell.Groups[1].Value.Equals("h", StringComparison.OrdinalIgnoreCase)), cells
        .Select(cell => Cell(cell.Groups[2].Value))
        .ToList()))
      .ToList();
    List<string>? headers = rows.FirstOrDefault().Header ? rows[0].Cells : null;

    IEnumerable<string> lines = rows
      .Where(row => !row.Header)
      .Select(row => row.Cells)
      .Select(cells => headers == null || cells.Count < 2
        ? string.Join(" | ", cells)
        : cells.Count == 2
          ? $"{cells[0]}: {cells[1]}"
          : $"{cells[0]} – {string.Join("; ", cells
            .Skip(1)
            .Select(cell => cell.TrimEnd('.'))
            .Select((cell, index) => headers.ElementAtOrDefault(index + 1) is { Length: > 0 } header ? $"{header}: {cell}" : cell))}");
    return "\n" + string.Join("\n", lines) + "\n";
  }

  private static string Cell(string html) => TagRegex()
    .Replace(html, string.Empty)
    .Trim();

  [GeneratedRegex(@"<table[^>]*>(.*?)</table>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex TableRegex();

  [GeneratedRegex(@"<tr[^>]*>(.*?)</tr>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex RowRegex();

  [GeneratedRegex(@"<t(h|d)[^>]*>(.*?)</t[hd]>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex CellRegex();

  [GeneratedRegex(@"</p>|<br\s*/?>|</li>|</h[1-6]>|</caption>", RegexOptions.IgnoreCase)]
  private static partial Regex BreakRegex();

  [GeneratedRegex("<[^>]+>")]
  private static partial Regex TagRegex();

  [GeneratedRegex(@"<(p|h[1-6]|ul|ol|table)\b[^>]*>(.*?)</\1\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex BlockRegex();

  [GeneratedRegex(@"<li[^>]*>(.*?)</li>", RegexOptions.IgnoreCase | RegexOptions.Singleline)]
  private static partial Regex ItemRegex();

  [GeneratedRegex(@"<(?<close>/?)(?<tag>[a-zA-Z][a-zA-Z0-9]*)[^>]*>|[^<]+")]
  private static partial Regex InlineRegex();

  [GeneratedRegex(@"\s+")]
  private static partial Regex WhiteSpaceRegex();

  [GeneratedRegex(@"\n{2,}")]
  private static partial Regex MultiNewLineRegex();

  #endregion
}
