using System.Globalization;

namespace TableTopSheets.Engine.Evaluation;

internal static class Naming
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

  #endregion
}
