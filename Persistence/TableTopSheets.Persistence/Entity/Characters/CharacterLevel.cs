using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Journal entry of a level up. Only the entry with the highest <see cref="Number"/> may be removed (undo)
/// </summary>
public class CharacterLevel: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  /// <summary>
  ///   Character level reached with this entry (1..20)
  /// </summary>
  public byte Number { get; set; }

  public Guid ClassId { get; set; }

  public CharacterClass? Class { get; set; }

  /// <summary>
  ///   Subclass chosen with this level (only set on the level the subclass is gained)
  /// </summary>
  public Guid? SubclassId { get; set; }

  public Subclass? Subclass { get; set; }

  /// <summary>
  ///   Rolled hit points for this level (null = fixed average); ignored for character level 1
  /// </summary>
  public byte? HitPointRoll { get; set; }

  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  #endregion
}
