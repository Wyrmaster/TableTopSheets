using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   One line of a paragraph, heading or list entry, or one table row; the unit <see cref="RichTextLayout"/> splits at.
///   Everything inside is relative to the top left of the line.
/// </summary>
public sealed class RichTextLine
{
  #region Constructor

  internal RichTextLine(int block, double spaceBefore, double height)
  {
    this.Block = block;
    this.SpaceBefore = spaceBefore;
    this.Height = height;
  }

  #endregion

  #region Property

  /// <summary>
  ///   Index of the <see cref="TableTopSheets.Common.Sheet.TextBlock"/> the line belongs to
  /// </summary>
  public int Block { get; }

  /// <summary>
  ///   Gap above the line; dropped when the line starts a column
  /// </summary>
  public double SpaceBefore { get; }

  /// <summary>
  ///   Height of the line
  /// </summary>
  public double Height { get; }

  /// <summary>
  ///   Text runs
  /// </summary>
  public List<RichTextRun> Runs { get; } = [];

  /// <summary>
  ///   Table lines
  /// </summary>
  public List<RichTextRule> Rules { get; } = [];

  /// <summary>
  ///   Shaded areas (table header)
  /// </summary>
  public List<XRect> Shades { get; } = [];

  /// <summary>
  ///   Bullets of list entries, as the box of a filled circle
  /// </summary>
  public List<XRect> Bullets { get; } = [];

  /// <summary>
  ///   A heading or table header: a column must not end with it
  /// </summary>
  public bool KeepWithNext { get; internal set; }

  /// <summary>
  ///   Header rows of the table a body row belongs to, repeated when a column starts with the row; empty otherwise
  /// </summary>
  public IReadOnlyList<RichTextLine> Header { get; internal set; } = [];

  /// <summary>
  ///   Text of the line: the runs joined, wrapped lines and table cells separated by a space
  /// </summary>
  public string Text => string.Concat(this.Runs.Select((run, index) => index > 0 && run.StartsLine ? " " + run.Text : run.Text));

  #endregion
}
