using System.Globalization;
using System.Text.RegularExpressions;
using TableTopSheets.Common.Enum;

namespace TableTopSheets.Engine.Evaluation;

public static class Naming
{
  #region Fields

  private static readonly Dictionary<string, string> Abilities = new()
  {
    ["str"] = "Strength",
    ["dex"] = "Dexterity",
    ["con"] = "Constitution",
    ["int"] = "Intelligence",
    ["wis"] = "Wisdom",
    ["cha"] = "Charisma",
  };

  #endregion

  #region Methods

  /// <summary>
  ///   Turns a key (<c>sleight-of-hand</c>, <c>dex</c>) into a display name
  /// </summary>
  public static string Title(string key)
    => Abilities.TryGetValue(key, out string? ability)
      ? ability
      : CultureInfo.InvariantCulture.TextInfo.ToTitleCase(key.Replace('-', ' '));

  /// <summary>
  ///   Turns an alignment into a display name (<c>NeutralGood</c> to <c>Neutral Good</c>)
  /// </summary>
  public static string Alignment(AlignmentEnum alignment)
    => Regex.Replace(alignment.ToString(), "([a-z])([A-Z])", "$1 $2");

  #endregion
}
