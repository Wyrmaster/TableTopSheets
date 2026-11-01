namespace TableTopSheets.Pdf.Text;

/// <summary>
///   How <see cref="RichTextLayout"/> lays out tables
/// </summary>
public sealed record RichTextOptions
{
  #region Property

  /// <summary>
  ///   Defaults for descriptions: a full grid, stretched to the width of the column, slightly smaller text
  /// </summary>
  public static RichTextOptions Default { get; } = new();

  /// <summary>
  ///   Label/value list: only lines between the rows, the last column takes the rest of the width
  /// </summary>
  public static RichTextOptions Facts { get; } = new()
  {
    TableGrid = false,
    StretchTables = false,
    TableScale = 1,
  };

  /// <summary>
  ///   Tables span the whole width; otherwise the extra width goes to the last column
  /// </summary>
  public bool StretchTables { get; init; } = true;

  /// <summary>
  ///   Draw the border and the lines between the columns; otherwise only lines between the rows
  /// </summary>
  public bool TableGrid { get; init; } = true;

  /// <summary>
  ///   Size of table text relative to the body size
  /// </summary>
  public double TableScale { get; init; } = 0.92;

  #endregion
}
