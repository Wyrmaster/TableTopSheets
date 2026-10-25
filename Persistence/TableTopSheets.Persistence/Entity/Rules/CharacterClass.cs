using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

public class CharacterClass: RuleEntity
{
  #region Properties

  public DiceEnum HitDie { get; set; }

  /// <summary>
  ///   Ability keys (<c>str</c>, <c>dex</c> ...) checked for the multiclass prerequisite
  /// </summary>
  public List<string> PrimaryAbilities { get; set; } = [];

  public PrimaryAbilityModeEnum PrimaryAbilityMode { get; set; }

  /// <summary>
  ///   Class level at which the subclass is chosen
  /// </summary>
  public byte SubclassLevel { get; set; } = 3;

  public ICollection<Subclass> Subclasses { get; set; } = new List<Subclass>();

  public ICollection<Feature> Features { get; set; } = new List<Feature>();

  #endregion
}
