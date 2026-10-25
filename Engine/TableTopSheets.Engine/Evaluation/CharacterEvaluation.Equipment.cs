using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Characters;
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
  ///   Shield that is currently wielded
  /// </summary>
  public Armor? WieldedShield => this.EquippedItems
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
      _ => this.EquippedItems.Any(entry => this.Catalog.Items[entry.ItemId].Key == what),
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
      "weapon" => held.OfType<Weapon>().Any(),
      "two-weapons" => held.OfType<Weapon>().Count() >= 2,
      _ => held.Any(entry => entry.Key == what),
    };
  }

  /// <summary>
  ///   Proficiency with the armor category (<c>proficiency.armor.*</c>)
  /// </summary>
  public bool HasArmorTraining(ArmorEnum category) => this.Stat($"proficiency.armor.{category.ToString().ToLowerInvariant()}") > 0;

  public bool IsProficient(Weapon weapon)
    => this.Stat($"proficiency.weapon.{weapon.Category.ToString().ToLowerInvariant()}") > 0
       || this.Stat($"proficiency.weapon.{weapon.Key}") > 0;

  #endregion
}
