using PdfSharp.Drawing;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Rich text (<see cref="TextBlock"/>s with bold and italic runs, lists and tables) laid out into a column of a given
///   width and body size. The lines can be split into columns of any height with <see cref="Fit"/> or flowed through
///   several columns with <see cref="Flow"/>; the resulting <see cref="RichTextSlice"/>s are what is measured and drawn.
/// </summary>
public sealed class RichTextLayout
{
  #region Constants

  private const double Epsilon = 0.0001;

  #endregion

  #region Constructor

  private RichTextLayout(IReadOnlyList<RichTextLine> lines, double width, double bodySize)
  {
    this.Lines = lines;
    this.Width = width;
    this.BodySize = bodySize;
  }

  #endregion

  #region Property

  /// <summary>
  ///   Lines and table rows in reading order
  /// </summary>
  public IReadOnlyList<RichTextLine> Lines { get; }

  /// <summary>
  ///   Width of the column
  /// </summary>
  public double Width { get; }

  /// <summary>
  ///   Font size of paragraphs
  /// </summary>
  public double BodySize { get; }

  /// <summary>
  ///   Height of all lines in one column
  /// </summary>
  public double Height => this.All().Height;

  #endregion

  #region Public Methods

  /// <summary>
  ///   Lay out <paramref name="blocks"/> at <paramref name="bodySize"/> into a column <paramref name="width"/> wide
  /// </summary>
  public static RichTextLayout Create(XGraphics gfx, IReadOnlyList<TextBlock> blocks, double width, double bodySize, RichTextOptions? options = null)
  {
    RichTextBuilder builder = new(gfx, width, bodySize, options ?? RichTextOptions.Default);
    return new(builder.Build(blocks), width, bodySize);
  }

  /// <summary>
  ///   All lines in one column of unlimited height
  /// </summary>
  public RichTextSlice All() => this.Fit(0, double.MaxValue);

  /// <summary>
  ///   The lines from <paramref name="start"/> that fit a column of <paramref name="height"/>. A column never ends with a
  ///   heading or table header and a table split between columns repeats its header. With <paramref name="force"/> a
  ///   column holds at least one line (even if it is too tall) so a split always progresses; otherwise a column without
  ///   room stays empty.
  /// </summary>
  public RichTextSlice Fit(int start, double height, bool force = true)
  {
    List<RichTextSliceLine> placed = [];
    double y = 0;
    if (start < this.Lines.Count)
    {
      foreach (RichTextLine header in this.Lines[start].Header)
      {
        placed.Add(new(header, y, true));
        y += header.Height;
      }
    }

    int repeated = placed.Count;
    int index = start;
    while (index < this.Lines.Count)
    {
      RichTextLine line = this.Lines[index];
      double gap = placed.Count == 0 ? 0 : line.SpaceBefore;
      if (y + gap + line.Height > height + Epsilon)
      {
        break;
      }

      placed.Add(new(line, y + gap, false));
      y += gap + line.Height;
      index++;
    }

    // don't end a column with a heading or a table header when more follows
    int keep = force ? 1 : 0;
    while (index < this.Lines.Count && placed.Count - repeated > keep && placed[^1].Line.KeepWithNext)
    {
      placed.RemoveAt(placed.Count - 1);
      index--;
    }

    if (placed.Count == repeated && index < this.Lines.Count)
    {
      if (force)
      {
        RichTextLine line = this.Lines[index];
        placed.Add(new(line, y, false));
        index++;
      }
      else
      {
        // no repeated header over an empty column
        placed.Clear();
      }
    }

    double bottom = placed.Count == 0 ? 0 : placed[^1].Y + placed[^1].Line.Height;
    return new(start, index - start, placed, bottom);
  }

  /// <summary>
  ///   The lines from <paramref name="start"/> flowed through columns of <paramref name="heights"/>, one slice per column:
  ///   each column continues where the one before ended. When no line fits any column, the first line goes into the
  ///   tallest column anyway so a split always progresses.
  /// </summary>
  public IReadOnlyList<RichTextSlice> Flow(int start, IReadOnlyList<double> heights)
  {
    List<RichTextSlice> slices = [];
    int index = start;
    foreach (double height in heights)
    {
      RichTextSlice slice = this.Fit(index, height, force: false);
      slices.Add(slice);
      index = slice.End;
    }

    if (index == start && start < this.Lines.Count && heights.Count > 0)
    {
      int tallest = heights
        .Select((height, column) => (Height: height, Column: column))
        .MaxBy(entry => entry.Height)
        .Column;
      for (int column = tallest; column < heights.Count; column++)
      {
        slices[column] = this.Fit(index, heights[column], force: column == tallest);
        index = slices[column].End;
      }
    }

    return slices;
  }

  #endregion
}
