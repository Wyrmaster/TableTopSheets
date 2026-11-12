using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

public class MagicItemTests
{
  [Fact]
  public async Task MagicWeaponRaisesOnlyItsOwnRowAndKeepsTheMasteryOfItsBase()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 13, 14, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "fighter", selections:
    [
      new("fighter.fighting-style", "defense"),
      new("fighter.weapon-mastery", "longsword"),
    ]);
    CharacterItem longsword = await world.Service.AddItemAsync(id, "plus1-longsword");
    CharacterItem dagger = await world.Service.AddItemAsync(id, "dagger");
    await world.Service.EquipAsync(id, longsword.Id, EquipSlotEnum.MainHand);
    await world.Service.EquipAsync(id, dagger.Id, EquipSlotEnum.OffHand);

    CharacterSheet sheet = await world.SheetAsync(id);
    AttackSheet magic = sheet.Attacks.Single(entry => entry.Name == "+1 Longsword");
    AttackSheet plain = sheet.Attacks.Single(entry => entry.Name.StartsWith("Dagger", StringComparison.Ordinal));

    // Strength +2, proficiency +2, magic +1
    Assert.Equal(5, magic.AttackBonus);
    Assert.Equal("1d8+3", magic.Damage);
    Assert.Equal("Sap", magic.Mastery);
    Assert.DoesNotContain("not proficient", magic.Notes);
    Assert.Equal(4, plain.AttackBonus);
  }

  [Fact]
  public async Task MagicArmorOnlyCountsWhileWorn()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 10, 14, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterItem plate = await world.Service.AddItemAsync(id, "plus2-plate-armor");

    await world.Service.EquipAsync(id, plate.Id, EquipSlotEnum.Worn);
    int carried = (await world.SheetAsync(id)).ArmorClass;
    await world.Service.EquipAsync(id, plate.Id, EquipSlotEnum.Armor);

    Assert.Equal(18 + 2, (await world.SheetAsync(id)).ArmorClass);
    Assert.True(carried < 18);
  }

  [Fact]
  public async Task MithralArmorHasNoStrengthRequirement()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 10, 14, 8, 12, 15), "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterItem plate = await world.Service.AddItemAsync(id, "plate-armor");
    CharacterItem mithral = await world.Service.AddItemAsync(id, "mithral-plate-armor");

    await world.Service.EquipAsync(id, plate.Id, EquipSlotEnum.Armor);
    int heavy = (await world.SheetAsync(id)).Speed;
    await world.Service.EquipAsync(id, plate.Id, EquipSlotEnum.None);
    await world.Service.EquipAsync(id, mithral.Id, EquipSlotEnum.Armor);

    Assert.Equal(20, heavy);
    Assert.Equal(30, (await world.SheetAsync(id)).Speed);
  }

  [Fact]
  public async Task BeltOfGiantStrengthSetsStrengthOnlyWhileAttuned()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.StandardArray);
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterItem belt = await world.Service.AddItemAsync(id, "belt-of-hill-giant-strength");
    await world.Service.EquipAsync(id, belt.Id, EquipSlotEnum.Worn);

    int unattuned = (await world.SheetAsync(id)).Abilities.Single(entry => entry.Key == "str").Score;
    await world.Service.AttuneAsync(id, belt.Id, true);

    Assert.NotEqual(21, unattuned);
    Assert.Equal(21, (await world.SheetAsync(id)).Abilities.Single(entry => entry.Key == "str").Score);
  }

  [Fact]
  public async Task CloakOfProtectionRaisesArmorClassAndSaves()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.StandardArray);
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterSheet before = await world.SheetAsync(id);
    CharacterItem cloak = await world.Service.AddItemAsync(id, "cloak-of-protection");
    await world.Service.EquipAsync(id, cloak.Id, EquipSlotEnum.Worn);
    await world.Service.AttuneAsync(id, cloak.Id, true);

    CharacterSheet after = await world.SheetAsync(id);

    Assert.Equal(before.ArmorClass + 1, after.ArmorClass);
    Assert.All(after.Abilities, entry => Assert.Equal(before.Abilities.Single(old => old.Key == entry.Key).SaveBonus + 1, entry.SaveBonus));
  }

  [Fact]
  public async Task RodOfThePactKeeperRaisesWarlockSpellsWhileHeld()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 10, 12, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpManyAsync(id, "warlock", 1);
    SpellcastingSheet before = (await world.SheetAsync(id)).Spellcasting.Single();
    CharacterItem rod = await world.Service.AddItemAsync(id, "plus2-rod-of-the-pact-keeper");
    await world.Service.AttuneAsync(id, rod.Id, true);
    await world.Service.EquipAsync(id, rod.Id, EquipSlotEnum.MainHand);

    SpellcastingSheet after = (await world.SheetAsync(id)).Spellcasting.Single();

    Assert.Equal(before.AttackBonus + 2, after.AttackBonus);
    Assert.Equal(before.SaveDc + 2, after.SaveDc);
  }

  [Fact]
  public async Task WingedBootsGiveAFlySpeedWhenSwitchedOn()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.StandardArray);
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterItem boots = await world.Service.AddItemAsync(id, "winged-boots");
    await world.Service.EquipAsync(id, boots.Id, EquipSlotEnum.Worn);
    await world.Service.AttuneAsync(id, boots.Id, true);

    Assert.Empty((await world.SheetAsync(id)).OtherSpeeds);
    await world.Service.ToggleAsync(id, "winged-boots.flight", true);

    Assert.Contains((await world.SheetAsync(id)).OtherSpeeds, entry => entry.Value == 30);
  }

  [Fact]
  public async Task RingOfResistanceGrantsTheChosenResistance()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.StandardArray);
    await world.LevelUpManyAsync(id, "fighter", 1);
    CharacterItem ring = await world.Service.AddItemAsync(id, "ring-of-resistance");
    await world.Service.EquipAsync(id, ring.Id, EquipSlotEnum.Worn);
    await world.Service.SelectAsync(id, [new SelectionRequest("ring-of-resistance.resistance", "fire")]);

    Assert.Contains((await world.SheetAsync(id)).Resistances, entry => entry.Contains("fire", StringComparison.OrdinalIgnoreCase));
  }

  [Fact]
  public async Task MagicWeaponWhoseBaseTheCharacterNeverReferencedCanBeEquipped()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 15, 13, 8, 14, 10), "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "monk", 2);
    CharacterItem staff = await world.Service.AddItemAsync(id, "flame-tongue-quarterstaff");
    await world.Service.EquipAsync(id, staff.Id, EquipSlotEnum.MainHand);

    CharacterSheet sheet = await world.SheetAsync(id);

    AttackSheet attack = sheet.Attacks.Single(entry => entry.Name.Contains("Quarterstaff", StringComparison.Ordinal));
    Assert.DoesNotContain("not proficient", attack.Notes);
  }
}
