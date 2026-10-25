using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Text;

/// <summary>
///   Result of <see cref="TextFit.Fit"/>: the chosen font, the wrapped lines per rectangle and the text that didn't fit
/// </summary>
public sealed class TextFitResult
{
  #region Constructor

  public TextFitResult(XFont font, IReadOnlyList<IReadOnlyList<string>> lines, string? overflow)
  {
    this.Font = font;
    this.Lines = lines;
    this.Overflow = overflow;
  }

  #endregion

  #region Property

  /// <summary>
  ///   Font the text is drawn with
  /// </summary>
  public XFont Font { get; }

  /// <summary>
  ///   Wrapped lines, one list per rectangle (column) in the order they were given
  /// </summary>
  public IReadOnlyList<IReadOnlyList<string>> Lines { get; }

  /// <summary>
  ///   Text that didn't fit even at the minimum size; <c>null</c> when everything fit
  /// </summary>
  public string? Overflow { get; }

  /// <summary>
  ///   Height of one line of <see cref="Font"/>
  /// </summary>
  public double LineHeight => TextFit.LineHeight(this.Font);

  /// <summary>
  ///   All drawn lines across every rectangle
  /// </summary>
  public IEnumerable<string> AllLines => this.Lines.SelectMany(entry => entry);

  #endregion
}
