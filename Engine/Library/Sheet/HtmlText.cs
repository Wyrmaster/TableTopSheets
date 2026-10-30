using System.Net;
using System.Text.RegularExpressions;

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

  [GeneratedRegex(@"\n{2,}")]
  private static partial Regex MultiNewLineRegex();

  #endregion
}
