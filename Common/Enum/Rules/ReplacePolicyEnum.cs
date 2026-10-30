namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   When a selection of a <c>Choice</c> may be swapped for another option
/// </summary>
public enum ReplacePolicyEnum
{
  Never = 0,
  /// <summary>
  ///   One selection may be replaced whenever the character gains a level in the owning class (journaled)
  /// </summary>
  OneOnLevelUp = 1,
  /// <summary>
  ///   Selections may be changed in place after a long rest (not journaled)
  /// </summary>
  OnLongRest = 2,
  /// <summary>
  ///   Selections may be changed in place after a short rest (not journaled)
  /// </summary>
  OnShortRest = 3,
}
