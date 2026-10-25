using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Magic;

namespace TableTopSheets.Persistence.Entity.Rules;

public class Spell: RuleEntity
{
  #region Properties

  /// <summary>
  ///   Spell level, 0 for cantrips
  /// </summary>
  public byte Level { get; set; }

  public SpellSchoolEnum School { get; set; }

  public string CastingTime { get; set; } = string.Empty;

  public bool Ritual { get; set; }

  public string Range { get; set; } = string.Empty;

  public string Components { get; set; } = string.Empty;

  public string? Material { get; set; }

  public bool Concentration { get; set; }

  public string Duration { get; set; } = string.Empty;

  /// <summary>
  ///   Class spell lists that contain the spell (<c>wizard</c>, <c>sorcerer</c> ...)
  /// </summary>
  public List<string> Lists { get; set; } = [];

  #endregion

  #region Damage

  /// <summary>
  ///   Base damage dice of a damaging cantrip (<c>1d10</c>)
  /// </summary>
  public string? Damage { get; set; }

  /// <summary>
  ///   Damage type (null when the caster chooses the type)
  /// </summary>
  public DamageTypeEnum? DamageType { get; set; }

  /// <summary>
  ///   The spell uses a spell attack roll
  /// </summary>
  public bool AttackRoll { get; set; }

  /// <summary>
  ///   Ability key of the saving throw the spell forces (<c>dex</c>)
  /// </summary>
  public string? SaveAbility { get; set; }

  /// <summary>
  ///   Cantrip damage at character level 1, 5, 11 and 17 (<c>1d10</c>, <c>2d10</c> ... or <c>2 × 1d10</c> for more beams)
  /// </summary>
  public List<string> CantripDamage { get; set; } = [];

  #endregion
}
