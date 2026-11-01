using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   The lines of a <see cref="RichTextLayout"/> that fit one column, positioned from its top. Measuring and drawing use
///   the same positions.
/// </summary>
public sealed class RichTextSlice
{
  #region Field

  private static readonly XBrush ShadeBrush = new XSolidBrush(XColor.FromArgb(232, 232, 234));

  #endregion

  #region Constructor

  internal RichTextSlice(int start, int count, IReadOnlyList<RichTextSliceLine> lines, double height)
  {
    this.Start = start;
    this.Count = count;
    this.Lines = lines;
    this.Height = height;
  }

  #endregion

  #region Property

  /// <summary>
  ///   Index of the first line of the layout in the slice
  /// </summary>
  public int Start { get; }

  /// <summary>
  ///   Number of layout lines in the slice, without repeated table headers
  /// </summary>
  public int Count { get; }

  /// <summary>
  ///   Index of the first layout line after the slice
  /// </summary>
  public int End => this.Start + this.Count;

  /// <summary>
  ///   Lines to draw, including repeated table headers
  /// </summary>
  public IReadOnlyList<RichTextSliceLine> Lines { get; }

  /// <summary>
  ///   Height of the drawn lines
  /// </summary>
  public double Height { get; }

  #endregion

  #region Public Methods

  /// <summary>
  ///   Draw the slice with its top left at <paramref name="origin"/>
  /// </summary>
  public void Draw(XGraphics gfx, XPoint origin, XBrush brush, XPen rulePen)
  {
    foreach (RichTextSliceLine entry in this.Lines)
    {
      double x = origin.X;
      double y = origin.Y + entry.Y;
      foreach (XRect shade in entry.Line.Shades)
      {
        gfx.DrawRectangle(ShadeBrush, shade.X + x, shade.Y + y, shade.Width, shade.Height);
      }

      foreach (RichTextRule rule in entry.Line.Rules)
      {
        gfx.DrawLine(rulePen, rule.X1 + x, rule.Y1 + y, rule.X2 + x, rule.Y2 + y);
      }

      foreach (XRect bullet in entry.Line.Bullets)
      {
        gfx.DrawEllipse(brush, bullet.X + x, bullet.Y + y, bullet.Width, bullet.Height);
      }

      foreach (RichTextRun run in entry.Line.Runs)
      {
        gfx.DrawString(run.Text, run.Font, brush, run.X + x, run.Baseline + y, XStringFormats.BaseLineLeft);
      }
    }
  }

  #endregion
}
