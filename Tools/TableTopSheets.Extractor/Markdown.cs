using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace TableTopSheets.Extractor;

/// <summary>
///   Minimal markdown → html conversion for rule descriptions (paragraphs, emphasis, lists, tables)
/// </summary>
internal static partial class Markdown
{
  #region Methods

  public static string ToHtml(IEnumerable<string> lines)
  {
    StringBuilder html = new();
    List<string> paragraph = new();
    List<string> list = new();
    List<string> table = new();

    void Flush()
    {
      if (paragraph.Count > 0)
      {
        html.Append("<p>").Append(Inline(string.Join(" ", paragraph))).Append("</p>");
        paragraph.Clear();
      }

      if (list.Count > 0)
      {
        html.Append("<ul>").Append(string.Concat(list.Select(entry => $"<li>{Inline(entry)}</li>"))).Append("</ul>");
        list.Clear();
      }

      if (table.Count > 0)
      {
        html.Append(Table(table));
        table.Clear();
      }
    }

    foreach (string raw in lines)
    {
      string line = raw.TrimEnd();
      if (line.StartsWith("![", StringComparison.Ordinal) || line is "___" or "---")
      {
        continue;
      }

      if (string.IsNullOrWhiteSpace(line))
      {
        Flush();
        continue;
      }

      string trimmed = line.TrimStart('>', ' ');
      if (trimmed.StartsWith('|'))
      {
        if (paragraph.Count > 0 || list.Count > 0)
        {
          Flush();
        }

        table.Add(trimmed);
      }
      else if (trimmed.StartsWith("- ", StringComparison.Ordinal))
      {
        if (paragraph.Count > 0 || table.Count > 0)
        {
          Flush();
        }

        list.Add(trimmed[2..]);
      }
      else if (trimmed.StartsWith('#'))
      {
        Flush();
        html.Append("<h5>").Append(Inline(trimmed.TrimStart('#', ' '))).Append("</h5>");
      }
      else
      {
        if (list.Count > 0 || table.Count > 0)
        {
          Flush();
        }

        // a paragraph starting with a bold-italic title starts a new paragraph
        if (trimmed.StartsWith("***", StringComparison.Ordinal) && paragraph.Count > 0)
        {
          Flush();
        }

        paragraph.Add(trimmed);
      }
    }

    Flush();
    return html.ToString();
  }

  public static string Inline(string text)
  {
    text = WebUtility.HtmlEncode(text).Replace("&#39;", "'");
    text = LinkRegex().Replace(text, "$2");
    text = BoldItalicRegex().Replace(text, "<strong><em>$1</em></strong>");
    text = BoldRegex().Replace(text, "<strong>$1</strong>");
    text = ItalicRegex().Replace(text, "<em>$1</em>");
    return text;
  }

  /// <summary>
  ///   Remove markdown emphasis from a short text
  /// </summary>
  public static string Plain(string text) => LinkRegex().Replace(text, "$2").Replace("*", string.Empty).Trim();

  private static string Table(List<string> rows)
  {
    StringBuilder html = new("<table>");
    bool header = true;
    foreach (string row in rows)
    {
      string[] cells = row.Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();
      if (cells.All(cell => cell.Length == 0 || cell.All(character => character is '-' or ':')))
      {
        continue;
      }

      string tag = header ? "th" : "td";
      html.Append("<tr>").Append(string.Concat(cells.Select(cell => $"<{tag}>{Inline(cell)}</{tag}>"))).Append("</tr>");
      header = false;
    }

    return html.Append("</table>").ToString();
  }

  [GeneratedRegex(@"\[([^\]]*)\]\(([^)]*)\)")]
  private static partial Regex LinkRegex();

  [GeneratedRegex(@"\*\*\*(.+?)\*\*\*")]
  private static partial Regex BoldItalicRegex();

  [GeneratedRegex(@"\*\*(.+?)\*\*")]
  private static partial Regex BoldRegex();

  [GeneratedRegex(@"\*(.+?)\*")]
  private static partial Regex ItalicRegex();

  #endregion
}
