using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Tests;

public class WeaponMasteryTests
{
  [Fact]
  public async Task EquippedWeaponsListEachMasteryOnce()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    await EquipAsync(world, id, "scimitar", EquipSlotEnum.MainHand);
    await EquipAsync(world, id, "dagger", EquipSlotEnum.OffHand);

    FeatureSheet feature = WeaponMastery(await world.SheetAsync(id));

    // the Longsword is mastered but not equipped, so Sap is left out
    Assert.StartsWith("Nick (Scimitar, Dagger): ", feature.Description);
    Assert.DoesNotContain("Sap", feature.Description);
    Assert.Equal(new TextSpan("Nick (Scimitar, Dagger): ", Bold: true), feature.DescriptionBlocks[0].Spans[0]);
  }

  [Fact]
  public async Task WithoutAMatchingEquippedWeaponAllChosenMasteriesAreListed()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    // a Greatclub has Push, but the fighter has no mastery with it
    await EquipAsync(world, id, "greatclub", EquipSlotEnum.MainHand);

    FeatureSheet feature = WeaponMastery(await world.SheetAsync(id));

    Assert.Contains("Nick (Dagger, Scimitar): ", feature.Description);
    Assert.Contains("Sap (Longsword): ", feature.Description);
    Assert.DoesNotContain("Push", feature.Description);
  }

  [Fact]
  public async Task WeaponMasteryOfSeveralClassesIsOneBlock()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    await world.LevelUpAsync(id, "barbarian", selections:
    [
      new("barbarian.weapon-mastery", "greataxe"),
      new("barbarian.weapon-mastery", "handaxe"),
    ]);

    List<FeatureSheet> features = (await world.SheetAsync(id)).ClassFeatures
      .Where(entry => entry.Key.EndsWith(".weapon-mastery", StringComparison.Ordinal))
      .ToList();

    Assert.Equal(2, features.Count);
    Assert.Contains("Cleave (Greataxe): ", features[0].Description);
    Assert.Contains("Nick (Dagger, Scimitar): ", features[0].Description);
    Assert.Empty(features[1].Description);
    Assert.Empty(features[1].DescriptionBlocks);
  }

  [Fact]
  public async Task TacticalMasterAddsPushSapAndSlowOnce()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    await world.LevelUpManyAsync(id, "fighter", 8, "champion");
    await EquipAsync(world, id, "longsword", EquipSlotEnum.MainHand);

    CharacterSheet sheet = await world.SheetAsync(id);
    string description = WeaponMastery(sheet).Description;

    Assert.Contains("Sap (Longsword): ", description);
    Assert.Contains("Push: ", description);
    Assert.Contains("Slow: ", description);
    Assert.DoesNotContain("Sap: ", description);
    // Tactical Master only adds properties, no weapons
    Assert.DoesNotContain("push", sheet.WeaponMasteryKeys);
  }

  [Fact]
  public async Task MagicWeaponCountsAsItsBaseWeapon()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    await EquipAsync(world, id, "plus1-longsword", EquipSlotEnum.MainHand);

    CharacterSheet sheet = await world.SheetAsync(id);
    AttackSheet attack = sheet.Attacks.Single(entry => entry.Name == "+1 Longsword");

    Assert.StartsWith("Sap (+1 Longsword): ", WeaponMastery(sheet).Description);
    Assert.Equal("Sap", attack.Mastery);
    Assert.Equal("sap", attack.MasteryKey);
    Assert.DoesNotContain("Mastery", attack.Notes);
    Assert.Equal(["dagger", "longsword", "scimitar"], sheet.WeaponMasteryKeys);
  }

  [Fact]
  public async Task StaffFocusUsesTheMasteryOfTheQuarterstaff()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 13, 14, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "fighter", selections:
    [
      new("fighter.fighting-style", "defense"),
      new("fighter.weapon-mastery", "quarterstaff"),
      new("fighter.weapon-mastery", "longsword"),
      new("fighter.weapon-mastery", "scimitar"),
    ]);
    await EquipAsync(world, id, "arcane-focus-staff", EquipSlotEnum.MainHand);

    CharacterSheet sheet = await world.SheetAsync(id);
    AttackSheet attack = sheet.Attacks.Single(entry => entry.Name == "Arcane Focus (Staff)");

    Assert.Equal("Topple", attack.Mastery);
    Assert.DoesNotContain("not proficient", attack.Notes);
    Assert.StartsWith("Topple (Arcane Focus (Staff)): ", WeaponMastery(sheet).Description);

    // a magic weapon made from a staff focus stays a focus, but is a Quarterstaff too
    Item silvered = world.Catalog.ItemsByKey["silvered-staff"];
    Assert.Equal("quarterstaff", world.Catalog.Items[silvered.BaseItemId!.Value].Key);
    Assert.Equal("arcane-focus", silvered.Group);
  }

  [Fact]
  public async Task AttackWithoutMasteryHasNone()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await FighterAsync(world);
    await EquipAsync(world, id, "greatclub", EquipSlotEnum.MainHand);

    AttackSheet attack = (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Greatclub");

    Assert.Null(attack.Mastery);
    Assert.Null(attack.MasteryKey);
  }

  /// <summary>
  ///   Level 1 fighter with mastery of the Dagger and Scimitar (Nick) and the Longsword (Sap)
  /// </summary>
  private static async Task<Guid> FighterAsync(TestWorld world)
  {
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 13, 14, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpAsync(id, "fighter", selections:
    [
      new("fighter.fighting-style", "defense"),
      new("fighter.weapon-mastery", "dagger"),
      new("fighter.weapon-mastery", "longsword"),
      new("fighter.weapon-mastery", "scimitar"),
    ]);
    return id;
  }

  private static async Task EquipAsync(TestWorld world, Guid id, string itemKey, EquipSlotEnum slot)
  {
    CharacterItem item = await world.Service.AddItemAsync(id, itemKey);
    await world.Service.EquipAsync(id, item.Id, slot);
  }

  private static FeatureSheet WeaponMastery(CharacterSheet sheet)
    => sheet.ClassFeatures.First(entry => entry.Key.EndsWith(".weapon-mastery", StringComparison.Ordinal));
}
