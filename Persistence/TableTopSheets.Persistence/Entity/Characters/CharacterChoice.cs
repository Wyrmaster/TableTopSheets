using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   One selection of a <see cref="Choice"/>
/// </summary>
public class CharacterChoice: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public Guid ChoiceId { get; set; }

  public Choice? Choice { get; set; }

  /// <summary>
  ///   Key of the selected option (skill, ability, spell, feat, feature, item or static option key)
  /// </summary>
  public required string OptionKey { get; set; }

  /// <summary>
  ///   Level up that made the selection (null = character creation / origin)
  /// </summary>
  public Guid? LevelId { get; set; }

  public CharacterLevel? Level { get; set; }

  /// <summary>
  ///   Selection that granted the feature owning this choice (e.g. the Ability Score Improvement feat chosen at level 4).
  ///   Distinguishes the instances of a choice that can be granted more than once
  /// </summary>
  public Guid? ParentId { get; set; }

  public CharacterChoice? Parent { get; set; }

  public ICollection<CharacterChoice> Children { get; set; } = new List<CharacterChoice>();

  /// <summary>
  ///   Level up that replaced this selection (null = active)
  /// </summary>
  public Guid? SupersededByLevelId { get; set; }

  public CharacterLevel? SupersededByLevel { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  #endregion
}
