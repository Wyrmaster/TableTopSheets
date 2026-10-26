using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Breaks a text into lines one at a time; each line may have a different width.
///   Paragraphs are separated by <c>\n</c>, words by spaces. Words wider than the line are broken by character.
/// </summary>
internal sealed class LineBreaker
{
  #region Field

  private readonly XGraphics _gfx;
  private readonly XFont _font;
  private readonly string[][] _paragraphs;
  private int _paragraph;
  private int _word;
  private int _char;

  #endregion

  #region Constructor

  public LineBreaker(XGraphics gfx, XFont font, string text)
  {
    this._gfx = gfx;
    this._font = font;
    this._paragraphs = Paragraphs(text);
  }

  #endregion

  #region Property

  /// <summary>
  ///   <c>true</c> when every word has been put on a line
  /// </summary>
  public bool Done => this._paragraph >= this._paragraphs.Length;

  /// <summary>
  ///   Current position, used to retry a line with <see cref="Restore"/>
  /// </summary>
  public (int Paragraph, int Word, int Char) Position => (this._paragraph, this._word, this._char);

  #endregion

  #region Public Methods

  /// <summary>
  ///   Return to a position from <see cref="Position"/>
  /// </summary>
  public void Restore((int Paragraph, int Word, int Char) position)
  {
    (this._paragraph, this._word, this._char) = position;
  }

  /// <summary>
  ///   Take the next line that fits <paramref name="width"/>; <c>null</c> when <see cref="Done"/>
  /// </summary>
  public string? Next(double width)
  {
    if (this.Done)
    {
      return null;
    }

    string[] words = this._paragraphs[this._paragraph];
    string line = string.Empty;
    while (this._word < words.Length)
    {
      string piece = words[this._word][this._char..];
      string candidate = line.Length == 0 ? piece : $"{line} {piece}";
      if (this.Width(candidate) <= width)
      {
        line = candidate;
        this._word++;
        this._char = 0;
        continue;
      }

      if (line.Length > 0)
      {
        return line;
      }

      // a single word wider than the line: break it by character, at least one per line
      int count = 1;
      while (count < piece.Length && this.Width(piece[..(count + 1)]) <= width)
      {
        count++;
      }

      this._char += count;
      return piece[..count];
    }

    this._paragraph++;
    this._word = 0;
    this._char = 0;
    return line;
  }

  /// <summary>
  ///   Text from the current position to the end, without leading empty lines; <c>null</c> when <see cref="Done"/>
  /// </summary>
  public string? Rest()
  {
    if (this.Done)
    {
      return null;
    }

    string[] words = this._paragraphs[this._paragraph];
    IEnumerable<string> current = words
      .Skip(this._word)
      .Select((word, index) => index == 0 ? word[this._char..] : word);
    IEnumerable<string> paragraphs = this._paragraphs
      .Skip(this._paragraph + 1)
      .Select(entry => string.Join(" ", entry))
      .Prepend(string.Join(" ", current));
    string rest = string
      .Join("\n", paragraphs)
      .TrimStart('\n')
      .TrimEnd();
    return rest.Length == 0 ? null : rest;
  }

  #endregion

  #region Private Methods

  private double Width(string text) => this._gfx.MeasureString(text, this._font).Width;

  private static string[][] Paragraphs(string text)
  {
    string normalized = text
      .Replace("\r\n", "\n")
      .Replace('\r', '\n')
      .Replace('\t', ' ')
      .TrimEnd();
    if (normalized.Length == 0)
    {
      return [];
    }

    return normalized
      .Split('\n')
      .Select(paragraph => paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
      .ToArray();
  }

  #endregion
}
