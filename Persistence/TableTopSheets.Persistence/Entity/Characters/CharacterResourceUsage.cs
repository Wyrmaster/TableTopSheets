namespace TableTopSheets.Persistence.Entity.Characters;

/// <summary>
///   Expended uses of a resource (<c>resource.rage</c>, <c>slot.multiclass.3</c>, <c>slot.pact</c>, <c>hitdice.d8</c> ...)
/// </summary>
public class CharacterResourceUsage: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public required string Key { get; set; }

  public int Used { get; set; }

  #endregion
}
