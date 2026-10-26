using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Tests;

public class EquipmentTests
{
  [Fact]
  public async Task MonkDefenseAndMartialArtsNeedNoArmor()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 15, 13, 8, 14, 10), "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "monk", 2);

    CharacterSheet unarmored = await world.SheetAsync(id);
    // 10 + Dexterity (+2) + Wisdom (15: +2)
    Assert.Equal(14, unarmored.ArmorClass);
    Assert.Equal(30 + 10, unarmored.Speed);
    Assert.Equal("1d6+2", unarmored.Attacks.Single(entry => entry.Name == "Unarmed Strike").Damage);

    CharacterItem leather = await world.Service.AddItemAsync(id, "leather-armor");
    await world.Service.EquipAsync(id, leather.Id, EquipSlotEnum.Armor);
    CharacterSheet armored = await world.SheetAsync(id);

    Assert.Equal(11 + 2, armored.ArmorClass);
    Assert.Equal(30, armored.Speed);
    Assert.Equal("1", armored.Attacks.Single(entry => entry.Name == "Unarmed Strike").Damage);
    Assert.Contains(armored.Violations, entry => entry.Code == "armor.untrained");

    await world.Service.EquipAsync(id, leather.Id, EquipSlotEnum.None);
    CharacterItem shield = await world.Service.AddItemAsync(id, "shield");
    await world.Service.EquipAsync(id, shield.Id, EquipSlotEnum.OffHand);
    CharacterSheet shielded = await world.SheetAsync(id);

    // Unarmored Defense ends with a shield, and without shield training the shield gives nothing
    Assert.Equal(10 + 2, shielded.ArmorClass);
    Assert.Contains(shielded.Violations, entry => entry.Code == "shield.untrained");
  }

  [Fact]
  public async Task OnlyOneArmorClassCalculationApplies()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 10, 12, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpManyAsync(id, "sorcerer", 3, "draconic-sorcery");

    CharacterSheet draconic = await world.SheetAsync(id);
    // Draconic Resilience: 10 + Dexterity (15: +2) + Charisma (17: +3)
    Assert.Equal(15, draconic.ArmorClass);

    await world.Service.ToggleAsync(id, "spell-effect.mage-armor", true);
    CharacterSheet mageArmor = await world.SheetAsync(id);
    Assert.Equal(15, mageArmor.ArmorClass);
    Assert.Contains(mageArmor.ArmorClassOptions, entry => entry.Label == "Mage Armor" && entry.Value == 15);

    // shield spell adds to whichever calculation is used; it doesn't stack two base calculations
    await world.Service.ToggleAsync(id, "spell-effect.shield", true);
    Assert.Equal(20, (await world.SheetAsync(id)).ArmorClass);

    await world.Service.ChooseArmorClassAsync(id, "Unarmored");
    Assert.Equal(12 + 5, (await world.SheetAsync(id)).ArmorClass);
  }

  [Fact]
  public async Task HeavyArmorWithoutStrengthReducesSpeed()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(12, 15, 13, 8, 14, 10), "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "fighter");
    CharacterItem plate = await world.Service.AddItemAsync(id, "plate-armor");
    await world.Service.EquipAsync(id, plate.Id, EquipSlotEnum.Armor);

    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(18, sheet.ArmorClass);
    Assert.Equal(20, sheet.Speed);
    Assert.Contains(sheet.Violations, entry => entry.Code == "armor.strength");
    Assert.True(sheet.Abilities.Single(entry => entry.Key == "dex").Skills.Single(entry => entry.Key == "stealth").Disadvantage);
  }

  [Fact]
  public async Task WeaponAttacksUseProficiencyAndMastery()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 13, 14, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "fighter", selections:
    [
      new("fighter.fighting-style", "dueling"),
      new("fighter.weapon-mastery", "longsword"),
    ]);
    CharacterItem longsword = await world.Service.AddItemAsync(id, "longsword");
    await world.Service.EquipAsync(id, longsword.Id, EquipSlotEnum.MainHand);

    AttackSheet attack = (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Longsword");

    // Strength +2, proficiency +2; Dueling +2 damage
    Assert.Equal(4, attack.AttackBonus);
    Assert.Equal("1d8+4", attack.Damage);
    Assert.Contains("Mastery: Sap", attack.Notes);
  }

  [Fact]
  public async Task ArmorLimitsTheDexterityModifierByItsDexCap()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    // Dexterity 17: +3
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 15, 13, 8, 14, 12), "entertainer", "dex", "dex", "str");
    await world.LevelUpAsync(id, "fighter");

    async Task<int> ArmorClassWith(string key)
    {
      CharacterItem armor = await world.Service.AddItemAsync(id, key);
      await world.Service.EquipAsync(id, armor.Id, EquipSlotEnum.Armor);
      int armorClass = (await world.SheetAsync(id)).ArmorClass;
      await world.Service.EquipAsync(id, armor.Id, EquipSlotEnum.None);
      return armorClass;
    }

    Assert.Equal(11 + 3, await ArmorClassWith("leather-armor"));
    Assert.Equal(14 + 2, await ArmorClassWith("breastplate"));
    Assert.Equal(16, await ArmorClassWith("chain-mail"));

    // a special medium armor without a cap adds the full modifier
    await world.Context.Items
      .OfType<Armor>()
      .Where(entry => entry.Key == "breastplate")
      .ExecuteUpdateAsync(setters => setters.SetProperty(entry => entry.DexCap, (byte?)null));
    Assert.Equal(14 + 3, await ArmorClassWith("breastplate"));
  }

  [Fact]
  public async Task MediumArmorMasterRaisesTheDexCap()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 15, 13, 8, 14, 12), "entertainer", "dex", "dex", "str");
    await world.LevelUpManyAsync(id, "fighter", 3, "eldritch-knight");
    await world.LevelUpAsync(id, "fighter", selections: [new SelectionRequest("fighter.ability-score-improvement-4", "medium-armor-master")]);
    CharacterItem breastplate = await world.Service.AddItemAsync(id, "breastplate");
    await world.Service.EquipAsync(id, breastplate.Id, EquipSlotEnum.Armor);

    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(14 + 3, sheet.ArmorClass);
    Assert.Contains(sheet.ArmorClassOptions, entry => entry.Label == "Armor" && entry.Value == 14 + 3);

    // heavy armor still adds no Dexterity
    CharacterItem chainMail = await world.Service.AddItemAsync(id, "chain-mail");
    await world.Service.EquipAsync(id, chainMail.Id, EquipSlotEnum.Armor);
    Assert.Equal(16, (await world.SheetAsync(id)).ArmorClass);
  }
}
