using System.Text;
using PdfSharp.Drawing;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Measures and wraps <see cref="TextBlock"/>s word by word into <see cref="RichTextLine"/>s for
///   <see cref="RichTextLayout"/>
/// </summary>
internal sealed class RichTextBuilder
{
  #region Constants

  private const double Epsilon = 0.0001;

  // all relative to the font size

  private const double BlockGap = 0.45;
  private const double HeadingGap = 0.7;
  private const double HeadingScale = 1.12;
  private const double ItemGap = 0.15;
  private const double ListIndent = 1.2;
  private const double BulletX = 0.35;
  private const double BulletSize = 0.28;
  private const double CellPaddingX = 0.4;
  private const double CellPaddingY = 0.2;

  /// <summary>
  ///   Largest width of a stretched table relative to the width of its content
  /// </summary>
  private const double MaxStretch = 1.5;

  #endregion

  #region Field

  private readonly XGraphics _gfx;
  private readonly double _width;
  private readonly double _size;
  private readonly RichTextOptions _options;
  private readonly Dictionary<(double Size, bool Bold, bool Italic), XFont> _fonts = [];
  private readonly Dictionary<(XFont Font, string Text), double> _widths = [];
  private readonly List<RichTextLine> _lines = [];

  #endregion

  #region Constructor

  public RichTextBuilder(XGraphics gfx, double width, double size, RichTextOptions options)
  {
    this._gfx = gfx;
    this._width = width;
    this._size = size;
    this._options = options;
  }

  #endregion

  #region Public Methods

  public List<RichTextLine> Build(IReadOnlyList<TextBlock> blocks)
  {
    for (int index = 0; index < blocks.Count; index++)
    {
      TextBlock block = blocks[index];
      switch (block.Kind)
      {
        case TextBlockKindEnum.Heading:
          this.AddFlow(index, block.Spans, this._size * HeadingScale, true, HeadingGap * this._size, keepWithNext: true);
          break;
        case TextBlockKindEnum.List:
          this.AddList(index, block);
          break;
        case TextBlockKindEnum.Table:
          this.AddTable(index, block);
          break;
        default:
          this.AddFlow(index, block.Spans, this._size, false, BlockGap * this._size, keepWithNext: false);
          break;
      }
    }

    return this._lines;
  }

  #endregion

  #region Blocks

  private void AddFlow(int block, IReadOnlyList<TextSpan> spans, double size, bool bold, double gap, bool keepWithNext)
  {
    List<FlowLine> wrapped = this.Wrap(this.Tokenize(spans, size, bold), this._width, size);
    for (int index = 0; index < wrapped.Count; index++)
    {
      RichTextLine line = Place(wrapped[index], block, index == 0 ? gap : 0, 0);
      line.KeepWithNext = keepWithNext;
      this._lines.Add(line);
    }
  }

  private void AddList(int block, TextBlock list)
  {
    double indent = ListIndent * this._size;
    double bullet = BulletSize * this._size;
    for (int item = 0; item < list.Items.Count; item++)
    {
      List<FlowLine> wrapped = this.Wrap(this.Tokenize(list.Items[item], this._size, false), this._width - indent, this._size);
      for (int index = 0; index < wrapped.Count; index++)
      {
        double gap = index > 0 ? 0 : item == 0 ? BlockGap * this._size : ItemGap * this._size;
        RichTextLine line = Place(wrapped[index], block, gap, indent);
        if (index == 0)
        {
          // centered on the x-height of the first line
          double center = wrapped[index].Ascent - this._size * 0.27;
          line.Bullets.Add(new(BulletX * this._size, center - bullet / 2, bullet, bullet));
        }

        this._lines.Add(line);
      }
    }
  }

  private void AddTable(int block, TextBlock table)
  {
    int columns = table.Rows.Count == 0 ? 0 : table.Rows.Max(row => row.Count);
    if (columns == 0)
    {
      return;
    }

    double size = this._size * this._options.TableScale;
    double padX = CellPaddingX * size;
    double padY = CellPaddingY * size;

    List<Word>[][] cells = table.Rows
      .Select((row, rowIndex) => Enumerable
        .Range(0, columns)
        .Select(column => column < row.Count ? this.Tokenize(row[column], size, rowIndex < table.HeaderRows) : [])
        .ToArray())
      .ToArray();

    double[] natural = new double[columns];
    double[] minimum = new double[columns];
    foreach (List<Word>[] row in cells)
    {
      for (int column = 0; column < columns; column++)
      {
        natural[column] = Math.Max(natural[column], this.NaturalWidth(row[column]) + 2 * padX);
        double longestWord = row[column]
          .Select(word => word.Width)
          .DefaultIfEmpty(0)
          .Max();
        minimum[column] = Math.Max(minimum[column], longestWord + 2 * padX);
      }
    }

    double[] widths = this.ColumnWidths(natural, minimum);
    double[] lefts = new double[columns + 1];
    for (int column = 0; column < columns; column++)
    {
      lefts[column + 1] = lefts[column] + widths[column];
    }

    double tableWidth = lefts[columns];
    double minHeight = TextFit.LineHeight(this.Font(size, false, false));
    List<RichTextLine> header = [];
    for (int rowIndex = 0; rowIndex < cells.Length; rowIndex++)
    {
      bool isHeader = rowIndex < table.HeaderRows;
      List<FlowLine>[] wrapped = cells[rowIndex]
        .Select((words, column) => this.Wrap(words, Math.Max(1, widths[column] - 2 * padX), size))
        .ToArray();
      double content = wrapped
        .Select(lines => lines.Sum(line => line.Height))
        .Append(minHeight)
        .Max();
      double height = content + 2 * padY;

      RichTextLine line = new(block, rowIndex == 0 ? BlockGap * this._size : 0, height);
      for (int column = 0; column < columns; column++)
      {
        double top = padY;
        foreach (FlowLine flow in wrapped[column])
        {
          AddRuns(line, flow, lefts[column] + padX, top);
          top += flow.Height;
        }
      }

      if (this._options.TableGrid)
      {
        line.Rules.Add(new(0, 0, tableWidth, 0));
        line.Rules.Add(new(0, height, tableWidth, height));
        line.Rules.AddRange(lefts.Select(left => new RichTextRule(left, 0, left, height)));
        if (isHeader)
        {
          line.Shades.Add(new(0, 0, tableWidth, height));
        }
      }
      else if (rowIndex < cells.Length - 1)
      {
        line.Rules.Add(new(0, height, tableWidth, height));
      }

      if (isHeader)
      {
        line.KeepWithNext = rowIndex < cells.Length - 1;
        header.Add(line);
      }
      else
      {
        line.Header = header;
      }

      this._lines.Add(line);
    }
  }

  private double[] ColumnWidths(double[] natural, double[] minimum)
  {
    int columns = natural.Length;
    double sumNatural = natural.Sum();
    double[] widths = new double[columns];
    if (sumNatural <= this._width)
    {
      // a stretched table of short cells grows a bit, but doesn't spread over a wide column
      double extra = this._options.StretchTables
        ? Math.Min(this._width, sumNatural * MaxStretch) - sumNatural
        : this._width - sumNatural;
      for (int column = 0; column < columns; column++)
      {
        widths[column] = natural[column];
        if (this._options.StretchTables)
        {
          widths[column] += sumNatural > 0 ? extra * natural[column] / sumNatural : extra / columns;
        }
      }

      if (!this._options.StretchTables)
      {
        widths[columns - 1] += extra;
      }

      return widths;
    }

    double others = natural[..^1].Sum();
    if (!this._options.StretchTables && others + minimum[^1] <= this._width)
    {
      // label/value lists: the labels keep one line, the last column wraps
      natural.CopyTo(widths, 0);
      widths[columns - 1] = this._width - others;
      return widths;
    }

    double sumMinimum = minimum.Sum();
    if (sumMinimum >= this._width)
    {
      // even the longest words don't fit: shrink every column, words get broken by character
      return minimum
        .Select(width => width * this._width / sumMinimum)
        .ToArray();
    }

    // every column gets its longest word, the rest of the width goes to the columns that wrap the most
    double flex = sumNatural - sumMinimum;
    for (int column = 0; column < columns; column++)
    {
      widths[column] = minimum[column] + (this._width - sumMinimum) * (natural[column] - minimum[column]) / flex;
    }

    return widths;
  }

  #endregion

  #region Wrapping

  private List<Word> Tokenize(IReadOnlyList<TextSpan> spans, double size, bool bold)
  {
    List<Word> words = [];
    Word? word = null;
    bool space = false;
    StringBuilder text = new();
    XFont font = this.Font(size, bold, false);

    void EndFragment()
    {
      if (text.Length > 0 && word != null)
      {
        string value = text.ToString();
        word.Fragments.Add(new(value, font, this.Measure(value, font)));
      }

      text.Clear();
    }

    foreach (TextSpan span in spans)
    {
      font = this.Font(size, bold || span.Bold, span.Italic);
      foreach (char character in span.Text)
      {
        if (character is '\n' or '\r')
        {
          EndFragment();
          word = null;
          space = false;
          words.Add(Word.Break());
          continue;
        }

        if (char.IsWhiteSpace(character) && character != ' ')
        {
          EndFragment();
          word = null;
          space = true;
          continue;
        }

        if (word == null)
        {
          word = new(space);
          words.Add(word);
          space = false;
        }

        text.Append(character);
      }

      EndFragment();
    }

    return words;
  }

  private List<FlowLine> Wrap(List<Word> words, double width, double size)
  {
    XFont baseFont = this.Font(size, false, false);
    List<FlowLine> lines = [];
    FlowLine line = new(baseFont);
    foreach (Word word in words)
    {
      if (word.IsBreak)
      {
        lines.Add(line);
        line = new(baseFont);
        continue;
      }

      double space = line.IsEmpty || !word.SpaceBefore ? 0 : this.Measure(" ", word.Fragments[0].Font);
      if (!line.IsEmpty && line.Width + space + word.Width > width + Epsilon)
      {
        lines.Add(line);
        line = new(baseFont);
        space = 0;
      }

      if (line.IsEmpty && word.Width > width + Epsilon)
      {
        line = this.BreakLongWord(word, width, lines, line, baseFont);
        continue;
      }

      for (int index = 0; index < word.Fragments.Count; index++)
      {
        Fragment fragment = word.Fragments[index];
        bool lead = index == 0 && space > 0;
        line.Add(lead ? " " + fragment.Text : fragment.Text, fragment.Font, lead ? space + fragment.Width : fragment.Width);
      }
    }

    if (!line.IsEmpty)
    {
      lines.Add(line);
    }

    return lines;
  }

  /// <summary>
  ///   A word wider than the line: put it on lines by character, at least one per line; returns the unfinished line
  /// </summary>
  private FlowLine BreakLongWord(Word word, double width, List<FlowLine> lines, FlowLine line, XFont baseFont)
  {
    foreach (Fragment fragment in word.Fragments)
    {
      foreach (char character in fragment.Text)
      {
        string piece = character.ToString();
        double pieceWidth = this.Measure(piece, fragment.Font);
        if (!line.IsEmpty && line.Width + pieceWidth > width + Epsilon)
        {
          lines.Add(line);
          line = new(baseFont);
        }

        line.Add(piece, fragment.Font, pieceWidth);
      }
    }

    return line;
  }

  private double NaturalWidth(List<Word> words)
  {
    double widest = 0;
    double current = 0;
    foreach (Word word in words)
    {
      if (word.IsBreak)
      {
        current = 0;
        continue;
      }

      double space = current > 0 && word.SpaceBefore ? this.Measure(" ", word.Fragments[0].Font) : 0;
      current += space + word.Width;
      widest = Math.Max(widest, current);
    }

    return widest;
  }

  private static RichTextLine Place(FlowLine flow, int block, double gap, double indent)
  {
    RichTextLine line = new(block, gap, flow.Height);
    AddRuns(line, flow, indent, 0);
    return line;
  }

  private static void AddRuns(RichTextLine line, FlowLine flow, double x, double top)
  {
    line.Runs.AddRange(flow.Pieces.Select((piece, index) =>
      new RichTextRun(piece.Text, piece.Font, x + piece.X, top + flow.Ascent, piece.Width, index == 0)));
  }

  #endregion

  #region Fonts

  private XFont Font(double size, bool bold, bool italic)
  {
    if (!this._fonts.TryGetValue((size, bold, italic), out XFont? font))
    {
      XFontStyleEx style = (bold, italic) switch
      {
        (true, true) => XFontStyleEx.BoldItalic,
        (true, false) => XFontStyleEx.Bold,
        (false, true) => XFontStyleEx.Italic,
        _ => XFontStyleEx.Regular,
      };
      font = new(TextFit.Family, size, style);
      this._fonts[(size, bold, italic)] = font;
    }

    return font;
  }

  private double Measure(string text, XFont font)
  {
    if (!this._widths.TryGetValue((font, text), out double width))
    {
      width = this._gfx.MeasureString(text, font).Width;
      this._widths[(font, text)] = width;
    }

    return width;
  }

  /// <summary>
  ///   Distance from the top of a line of <paramref name="font"/> to its baseline
  /// </summary>
  private static double Ascent(XFont font) => TextFit.LineHeight(font) * font.CellAscent / font.CellSpace;

  #endregion

  #region Nested Types

  /// <summary>
  ///   Part of a word in one font
  /// </summary>
  private sealed record Fragment(string Text, XFont Font, double Width);

  /// <summary>
  ///   Text between spaces, possibly in several fonts; or a forced line break
  /// </summary>
  private sealed class Word
  {
    public Word(bool spaceBefore)
    {
      this.SpaceBefore = spaceBefore;
    }

    public bool SpaceBefore { get; }

    public bool IsBreak { get; private init; }

    public List<Fragment> Fragments { get; } = [];

    public double Width => this.Fragments.Sum(fragment => fragment.Width);

    public static Word Break() => new(false) { IsBreak = true };
  }

  /// <summary>
  ///   Line being filled; pieces are merged while the font stays the same
  /// </summary>
  private sealed class FlowLine
  {
    private readonly XFont _baseFont;

    public FlowLine(XFont baseFont)
    {
      this._baseFont = baseFont;
    }

    public List<(string Text, XFont Font, double X, double Width)> Pieces { get; } = [];

    public double Width { get; private set; }

    public bool IsEmpty => this.Pieces.Count == 0;

    public double Height => this.Pieces
      .Select(piece => TextFit.LineHeight(piece.Font))
      .Append(TextFit.LineHeight(this._baseFont))
      .Max();

    public double Ascent => this.Pieces
      .Select(piece => RichTextBuilder.Ascent(piece.Font))
      .Append(RichTextBuilder.Ascent(this._baseFont))
      .Max();

    public void Add(string text, XFont font, double width)
    {
      if (this.Pieces.Count > 0 && ReferenceEquals(this.Pieces[^1].Font, font))
      {
        (string Text, XFont Font, double X, double Width) last = this.Pieces[^1];
        this.Pieces[^1] = last with { Text = last.Text + text, Width = last.Width + width };
      }
      else
      {
        this.Pieces.Add((text, font, this.Width, width));
      }

      this.Width += width;
    }
  }

  #endregion
}
