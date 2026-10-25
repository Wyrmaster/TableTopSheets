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
}
