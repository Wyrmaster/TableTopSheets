namespace TableTopSheets.Persistence.Entity.Rules;

/// <summary>
///   Row of a spell slot table (<c>multiclass</c>, <c>pact</c> or a class specific table)
/// </summary>
public class SpellSlotRow: BaseEntity
{
  #region Properties

  public required string TableKey { get; set; }

  public byte CasterLevel { get; set; }

  public byte SpellLevel { get; set; }

  public byte Count { get; set; }

  #endregion
}
