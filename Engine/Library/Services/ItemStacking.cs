using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Which pieces of an item share one inventory entry. Gear, tools and thrown weapons stack; other weapons and armor are
///   held, worn and readied one by one, items to attune are attuned one by one
/// </summary>
public static class ItemStacking
{
  #region Methods

  /// <summary>
  ///   True when pieces of the item stack
  /// </summary>
  public static bool Stacks(Item item) => !item.RequiresAttunement && item switch
  {
    Armor => false,
    Weapon weapon => weapon.Properties.HasFlag(WeaponPropertyEnum.Thrown),
    _ => item.Kind is ItemKindEnum.Gear or ItemKindEnum.Tool,
  };

  /// <summary>
  ///   Plain stack of the item in the slot that more pieces can join: not attuned, without notes
  /// </summary>
  public static CharacterItem? Stack(Character character, Item item, EquipSlotEnum slot, CharacterItem? except = null)
    => character.Items.FirstOrDefault(entry => entry != except
                                               && entry.ItemId == item.Id
                                               && entry.EquipSlot == slot
                                               && !entry.Attuned
                                               && entry.Notes == null);


  /// <summary>
  ///   Plain rows (not attuned, without notes) that share an item and a place; hands and the armor slot hold one piece
  ///   each, so their rows never count. Whether the item stacks at all is up to <see cref="Stacks" />
  /// </summary>
  public static List<IGrouping<(Guid ItemId, EquipSlotEnum Slot), CharacterItem>> Duplicates(Character character) => character.Items
    .Where(entry => !entry.Attuned
                    && entry.Notes == null
                    && entry.EquipSlot is not (EquipSlotEnum.MainHand or EquipSlotEnum.OffHand or EquipSlotEnum.Armor))
    .GroupBy(entry => (entry.ItemId, entry.EquipSlot))
    .Where(group => group.Count() > 1)
    .ToList();

  #endregion
}
