using System.Net;
using System.Text.RegularExpressions;

namespace TableTopSheets.Engine.Sheet;

internal static partial class HtmlText
{
  #region Methods

  /// <summary>
  ///   Plain text of a html description (paragraphs become new lines)
  /// </summary>
  public static string ToPlain(string html)
  {
    string text = BreakRegex().Replace(html, "\n");
    text = TagRegex().Replace(text, string.Empty);
    text = WebUtility.HtmlDecode(text);
    return MultiNewLineRegex().Replace(text, "\n").Trim();
  }

  [GeneratedRegex(@"</p>|<br\s*/?>|</li>", RegexOptions.IgnoreCase)]
  private static partial Regex BreakRegex();

  [GeneratedRegex("<[^>]+>")]
  private static partial Regex TagRegex();

  [GeneratedRegex(@"\n{2,}")]
  private static partial Regex MultiNewLineRegex();

  #endregion
}
