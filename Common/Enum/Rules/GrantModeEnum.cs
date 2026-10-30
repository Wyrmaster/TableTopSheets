namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   Defines if a class feature is granted depending on the class being the first one of a character
/// </summary>
public enum GrantModeEnum
{
  Always = 0,
  /// <summary>
  ///   Only granted when the class is the initial class of the character (e.g. saving throw proficiencies)
  /// </summary>
  FirstClassOnly = 1,
  /// <summary>
  ///   Only granted when the class is added as a multiclass (reduced proficiencies)
  /// </summary>
  MulticlassOnly = 2,
}
