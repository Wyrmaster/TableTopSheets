using TableTopSheets.Common.Enum.Character;

namespace TableTopSheets.Common.Key;

/// <summary>
///   Stable string keys used by the rule data for abilities and skills
/// </summary>
public static class RuleKeys
{
  #region Fields

  private static readonly Dictionary<AbilityEnum, string> AbilityToKey = new()
  {
    [AbilityEnum.Strength] = "str",
    [AbilityEnum.Dexterity] = "dex",
    [AbilityEnum.Constitution] = "con",
    [AbilityEnum.Intelligence] = "int",
    [AbilityEnum.Wisdom] = "wis",
    [AbilityEnum.Charisma] = "cha",
  };

  private static readonly Dictionary<SkillEnum, (string Key, AbilityEnum Ability)> SkillToKey = new()
  {
    [SkillEnum.Athletics] = ("athletics", AbilityEnum.Strength),
    [SkillEnum.Acrobatics] = ("acrobatics", AbilityEnum.Dexterity),
    [SkillEnum.SleightOfHand] = ("sleight-of-hand", AbilityEnum.Dexterity),
    [SkillEnum.Stealth] = ("stealth", AbilityEnum.Dexterity),
    [SkillEnum.Arcana] = ("arcana", AbilityEnum.Intelligence),
    [SkillEnum.History] = ("history", AbilityEnum.Intelligence),
    [SkillEnum.Investigation] = ("investigation", AbilityEnum.Intelligence),
    [SkillEnum.Nature] = ("nature", AbilityEnum.Intelligence),
    [SkillEnum.Religion] = ("religion", AbilityEnum.Intelligence),
    [SkillEnum.AnimalHandling] = ("animal-handling", AbilityEnum.Wisdom),
    [SkillEnum.Insight] = ("insight", AbilityEnum.Wisdom),
    [SkillEnum.Medicine] = ("medicine", AbilityEnum.Wisdom),
    [SkillEnum.Perception] = ("perception", AbilityEnum.Wisdom),
    [SkillEnum.Survival] = ("survival", AbilityEnum.Wisdom),
    [SkillEnum.Deception] = ("deception", AbilityEnum.Charisma),
    [SkillEnum.Intimidation] = ("intimidation", AbilityEnum.Charisma),
    [SkillEnum.Performance] = ("performance", AbilityEnum.Charisma),
    [SkillEnum.Persuasion] = ("persuasion", AbilityEnum.Charisma),
  };

  #endregion

  #region Properties

  /// <summary>
  ///   All ability keys in sheet order
  /// </summary>
  public static IReadOnlyList<string> Abilities { get; } = ["str", "dex", "con", "int", "wis", "cha"];

  /// <summary>
  ///   All skill keys
  /// </summary>
  public static IReadOnlyList<string> Skills { get; } = SkillToKey.Values
    .Select(entry => entry.Key)
    .ToList();

  #endregion

  #region Methods

  public static string Key(this AbilityEnum ability) => AbilityToKey[ability];

  public static string Key(this SkillEnum skill) => SkillToKey[skill].Key;

  public static AbilityEnum Ability(this SkillEnum skill) => SkillToKey[skill].Ability;

  public static AbilityEnum ToAbility(string key) => AbilityToKey.Single(entry => entry.Value == key).Key;

  public static SkillEnum ToSkill(string key) => SkillToKey.Single(entry => entry.Value.Key == key).Key;

  public static string SkillAbilityKey(string skillKey) => ToSkill(skillKey)
    .Ability()
    .Key();

  public static bool IsAbility(string key) => AbilityToKey.ContainsValue(key);

  public static bool IsSkill(string key) => SkillToKey.Values.Any(entry => entry.Key == key);

  #endregion
}
