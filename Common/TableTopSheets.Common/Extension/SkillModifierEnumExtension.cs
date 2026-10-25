using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Character;

namespace TableTopSheets.Common.Extension;

public static class SkillModifierEnumExtension
{
  #region Extension Methods

  public static int GetProficiency(this SkillModifierEnum skillModifierEnum, int proficiencyBonus) 
    => (int)Math.Floor(proficiencyBonus * (double)skillModifierEnum / 10);

  #endregion
}