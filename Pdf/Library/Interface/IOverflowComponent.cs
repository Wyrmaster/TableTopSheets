namespace TableTopSheets.Pdf.Interface;

/// <summary>
///   Component whose text may not fit; the rest is continued in a section on an extra page
/// </summary>
public interface IOverflowComponent: IComponent
{
  #region Property

  /// <summary>
  ///   Text that didn't fit, filled while the component is drawn; <c>null</c> when everything fit
  /// </summary>
  string? Overflow { get; }

  /// <summary>
  ///   Title of the continuation section, e.g. <c>CLASS FEATURES</c>
  /// </summary>
  string ContinuationTitle { get; }

  #endregion
}
