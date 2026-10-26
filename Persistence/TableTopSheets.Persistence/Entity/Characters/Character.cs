using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   A character only stores decisions and play state; every value on the sheet is computed from the rules
/// </summary>
public class Character: BaseEntity
{
  #region Properties

  public required Guid OwnerId { get; set; }

  public User? Owner { get; set; }

  public required string Name { get; set; }

  public Guid? SpeciesId { get; set; }

  public Species? Species { get; set; }

  public Guid? BackgroundId { get; set; }

  public Background? Background { get; set; }

  public AlignmentEnum Alignment { get; set; }

  public int Experience { get; set; }

  public string Appearance { get; set; } = string.Empty;

  public string Backstory { get; set; } = string.Empty;

  #endregion

  #region Base Ability Scores

  public AbilityMethodEnum AbilityMethod { get; set; }

  public byte Strength { get; set; } = 10;

  public byte Dexterity { get; set; } = 10;

  public byte Constitution { get; set; } = 10;

  public byte Intelligence { get; set; } = 10;

  public byte Wisdom { get; set; } = 10;

  public byte Charisma { get; set; } = 10;

  #endregion

  #region State

  /// <summary>
  ///   Keys of toggleable features that are currently active (Rage, Mage Armor ...)
  /// </summary>
  public List<string> ActiveToggles { get; set; } = [];

  /// <summary>
  ///   Key of the effect label chosen when several Armor Class calculations apply (null = best)
  /// </summary>
  public string? ArmorClassChoice { get; set; }

  public CharacterPlayState PlayState { get; set; } = new();

  #endregion

  #region Navigation

  public ICollection<CharacterLevel> Levels { get; set; } = new List<CharacterLevel>();

  public ICollection<CharacterChoice> Choices { get; set; } = new List<CharacterChoice>();

  public ICollection<CharacterItem> Items { get; set; } = new List<CharacterItem>();

  public ICollection<CharacterPreparedSpell> PreparedSpells { get; set; } = new List<CharacterPreparedSpell>();

  public ICollection<CharacterSpellbookEntry> SpellbookEntries { get; set; } = new List<CharacterSpellbookEntry>();

  public ICollection<CharacterResourceUsage> ResourceUsages { get; set; } = new List<CharacterResourceUsage>();

  #endregion
}
