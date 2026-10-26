using System.Text.RegularExpressions;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Registry of every stat key an effect may target. Data is validated against it when the catalog is loaded
/// </summary>
public static class StatKeys
{
  #region Fields

  private const string Ability = "(str|dex|con|int|wis|cha)";
  private const string Skill = "(athletics|acrobatics|sleight-of-hand|stealth|arcana|history|investigation|nature|religion|animal-handling|insight|medicine|perception|survival|deception|intimidation|performance|persuasion)";
  private const string Name = "[a-z0-9][a-z0-9'-]*";

  private static readonly Regex[] Patterns =
  [
    new($@"^ability\.{Ability}\.(score|max)$"),
    new($@"^save\.({Ability}|all)\.(prof|bonus|advantage)$"),
    new($@"^skill\.({Skill}|all)\.(prof|bonus|advantage|disadvantage)$"),
    new(@"^check\.(all|str|dex|con|int|wis|cha)\.(bonus|advantage|disadvantage)$"),
    new(@"^ac\.(base|bonus|dexcap)$"),
    new(@"^initiative\.(bonus|advantage)$"),
    new(@"^speed\.(walk|fly|swim|climb|burrow)$"),
    new(@"^hp\.(perLevel|bonus)$"),
    new(@"^sense\.(darkvision|blindsight|tremorsense|truesight)$"),
    new($@"^damage\.(resist|immune|vulnerable)\.{Name}$"),
    new($@"^condition\.immune\.{Name}$"),
    new(@"^damage\.(bonus|melee\.bonus|ranged\.bonus)$"),
    new($@"^damage\.spell\.{Name}$"),
    new(@"^attack\.(extraAttacks|bonus|melee\.bonus|ranged\.bonus|melee\.dex|offhand\.modifier|unarmed\.die|unarmed\.bonus)$"),
    new(@"^proficiency\.armor\.(light|medium|heavy|shield)$"),
    new($@"^proficiency\.(weapon|tool)\.{Name}$"),
    new($@"^language\.{Name}$"),
    new($@"^mastery\.{Name}$"),
    new($@"^spell\.{Name}$"),
    new(@"^spellcasting\.(attack|dc)\.bonus$"),
    new(@"^passive\.perception\.bonus$"),
    new(@"^size$"),
    new($@"^custom\.{Name}(\.{Name})*$"),
  ];

  #endregion

  #region Methods

  public static bool IsValid(string key) => Patterns.Any(pattern => pattern.IsMatch(key));

  #endregion
}
