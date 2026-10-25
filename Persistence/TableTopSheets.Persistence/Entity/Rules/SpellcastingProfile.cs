using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Describes how a Spellcasting / Pact Magic feature works; active while its <see cref="Feature"/> is active
/// </summary>
public class SpellcastingProfile: BaseEntity
{
  #region Properties

  public Guid FeatureId { get; set; }

  public Feature? Feature { get; set; }

  /// <summary>
  ///   Class whose levels count for this profile (the Fighter for an Eldritch Knight)
  /// </summary>
  public Guid ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  /// <summary>
  ///   Ability key used for spell attacks and save DCs
  /// </summary>
  public required string Ability { get; set; }

  public CasterProgressionEnum Progression { get; set; }

  /// <summary>
  ///   Slot table used when this is the only spellcasting profile (null = <c>multiclass</c> table, <c>pact</c> for Pact Magic)
  /// </summary>
  public string? SlotTableKey { get; set; }

  /// <summary>
  ///   Spell list spells are learned from (<c>wizard</c>)
  /// </summary>
  public required string SpellListKey { get; set; }

  /// <summary>
  ///   Scale key holding the number of prepared spells
  /// </summary>
  public string? PreparedCountKey { get; set; }

  /// <summary>
  ///   Scale key holding the number of cantrips
  /// </summary>
  public string? CantripCountKey { get; set; }

  public SpellPoolEnum Pool { get; set; }

  public PrepareChangeEnum PrepareChange { get; set; }

  public RitualModeEnum RitualMode { get; set; }

  public string? Focus { get; set; }

  #endregion
}
