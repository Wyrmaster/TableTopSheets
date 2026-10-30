using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Spell of the long rest loadout of a class with <c>PrepareChange.LongRest</c>
/// </summary>
public class CharacterPreparedSpell: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public Guid ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  public Guid SpellId { get; set; }

  public Spell? Spell { get; set; }

  #endregion
}
