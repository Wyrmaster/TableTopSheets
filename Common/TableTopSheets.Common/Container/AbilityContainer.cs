using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;

namespace TableTopSheets.Common.Container;

public struct AbilityContainer
{
  public string AbilityName { get; set; }
  
  public int Score { get; set; }

  public IEnumerable<(SkillModifierEnum skillModifier, string skill)> Skill { get; set; }

  public bool SavingThrow { get; set; }
}