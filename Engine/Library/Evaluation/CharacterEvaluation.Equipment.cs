using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Evaluation;

public sealed partial class CharacterEvaluation
{
  #region Properties

  public IEnumerable<CharacterItem> EquippedItems => this.Character.Items.Where(entry => entry.Equipped);

  /// <summary>
  ///   Body armor that is currently worn
  /// </summary>
  public Armor? WornArmor => this.Character.Items
    .Where(entry => entry.EquipSlot == EquipSlotEnum.Armor)
    .Select(entry => this.Catalog.Items[entry.ItemId])
    .OfType<Armor>()
    .FirstOrDefault(entry => entry.Category != ArmorEnum.Shield);

  /// <summary>
  ///   Shield that is currently wielded (held in a hand; a shield carried as <c>Worn</c> doesn't count)
  /// </summary>
  public Armor? WieldedShield => this.Character.Items
    .Where(entry => entry.EquipSlot is EquipSlotEnum.MainHand or EquipSlotEnum.OffHand)
    .Select(entry => this.Catalog.Items[entry.ItemId])
    .OfType<Armor>()
    .FirstOrDefault(entry => entry.Category == ArmorEnum.Shield);

  #endregion

  #region Methods

  /// <summary>
  ///   <c>armor</c> (any body armor), <c>light</c>, <c>medium</c>, <c>heavy</c>, or the key of an item
  /// </summary>
  public bool Wearing(string what)
  {
    Armor? armor = this.WornArmor;
    return what switch
    {
      "armor" => armor != null,
      "light" => armor?.Category == ArmorEnum.Light,
      "medium" => armor?.Category == ArmorEnum.Medium,
      "heavy" => armor?.Category == ArmorEnum.Heavy,
      _ => this.EquippedItems.Any(entry => this.IsItem(this.Catalog.Items[entry.ItemId], what)),
    };
  }

  /// <summary>
  ///   <c>shield</c>, <c>weapon</c>, <c>two-weapons</c>, or the key of an item held in a hand
  /// </summary>
  public bool Wielding(string what)
  {
    List<Item> held = this.Character.Items
      .Where(entry => entry.EquipSlot is EquipSlotEnum.MainHand or EquipSlotEnum.OffHand)
      .Select(entry => this.Catalog.Items[entry.ItemId])
      .ToList();
    return what switch
    {
      "shield" => this.WieldedShield != null,
      "weapon" => held
        .OfType<Weapon>()
        .Any(),
      "two-weapons" => held
        .OfType<Weapon>()
        .Count() >= 2,
      _ => held.Any(entry => this.IsItem(entry, what)),
    };
  }

  /// <summary>
  ///   Proficiency with the armor category (<c>proficiency.armor.*</c>)
  /// </summary>
  public bool HasArmorTraining(ArmorEnum category) => this.Stat($"proficiency.armor.{category
    .ToString()
    .ToLowerInvariant()}") > 0;

  public bool IsProficient(Weapon weapon)
    => this.Stat($"proficiency.weapon.{weapon.Category
      .ToString()
      .ToLowerInvariant()}") > 0
       || this.Stat($"proficiency.weapon.{this.BaseKey(weapon)}") > 0;

  /// <summary>
  ///   Key of the mundane item a magic variant is made from (<c>longsword</c> for a +1 Longsword), else the item's own key
  /// </summary>
  public string BaseKey(Item item)
    => item.BaseItemId is { } baseId && this.Catalog.Items.TryGetValue(baseId, out Item? baseItem) ? baseItem.Key : item.Key;

  /// <summary>
  ///   Mastery property of a weapon; a magic variant without its own takes the one of its base weapon
  /// </summary>
  public WeaponMastery? MasteryOf(Weapon weapon)
  {
    Guid? masteryId = weapon.MasteryId
                      ?? (weapon.BaseItemId is { } baseId
                          && this.Catalog.Items.TryGetValue(baseId, out Item? baseItem)
                          && baseItem is Weapon baseWeapon
                        ? baseWeapon.MasteryId
                        : null);
    return masteryId is { } id ? this.Catalog.Masteries[id] : null;
  }

  /// <summary>
  ///   The character has mastery with the weapon (<c>mastery.*</c> of its base weapon)
  /// </summary>
  public bool HasMastery(Weapon weapon) => this.Stat($"mastery.{this.BaseKey(weapon)}") > 0;

  /// <summary>
  ///   The item is the one of the key or a variant of it
  /// </summary>
  private bool IsItem(Item item, string key) => item.Key == key || this.BaseKey(item) == key;

  #endregion
}
