using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Spell copied into a spellbook from a scroll or another book (spells gained on level up are choices)
/// </summary>
public class CharacterSpellbookEntry: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public Guid SpellId { get; set; }

  public Spell? Spell { get; set; }

  public string? Note { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  #endregion
}
