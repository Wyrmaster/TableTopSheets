using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Entity.Characters;

public class CharacterItem: BaseEntity
{
  #region Properties

  public Guid CharacterId { get; set; }

  public Character? Character { get; set; }

  public Guid ItemId { get; set; }

  public Item? Item { get; set; }

  public int Quantity { get; set; } = 1;

  public EquipSlotEnum EquipSlot { get; set; }

  public bool Equipped => this.EquipSlot != EquipSlotEnum.None;

  public bool Attuned { get; set; }

  public string? Notes { get; set; }

  #endregion
}
