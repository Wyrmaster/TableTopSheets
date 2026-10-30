using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   Rules that grow with the character level (all classes together) instead of a class level
/// </summary>
public class CharacterLevelScalingTests
{
  [Fact]
  public async Task CantripDamageFollowsTheCharacterLevelNotTheClassLevel()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(13, 14, 13, 8, 10, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpAsync(id, "sorcerer", selections:
    [
      new("sorcerer.cantrips", "fire-bolt"),
      new("sorcerer.cantrips", "acid-splash"),
    ]);
    AttackSheet level1 = (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Fire Bolt");
    Assert.Equal("1d10", level1.Damage);

    // Sorcerer 1 / Fighter 4 = character level 5: the cantrip upgrades even though the Sorcerer level is 1
    await world.LevelUpManyAsync(id, "fighter", 2);
    await world.LevelUpAsync(id, "fighter", "eldritch-knight", [new("eldritch-knight.cantrips", "light"), new("eldritch-knight.cantrips", "mage-hand")]);
    await world.LevelUpAsync(id, "fighter");
    CharacterSheet level5 = await world.SheetAsync(id);
    AttackSheet fireBolt = level5.Attacks.Single(entry => entry.Name == "Fire Bolt");
    AttackSheet acidSplash = level5.Attacks.Single(entry => entry.Name == "Acid Splash");

    Assert.Equal("2d10", fireBolt.Damage);
    Assert.Equal([DiceEnum.D10, DiceEnum.D10], fireBolt.DamageDice);
    Assert.Equal(DamageTypeEnum.Fire, fireBolt.DamageType);
    // Charisma 17 (+3) and the proficiency bonus of character level 5 (+3)
    Assert.Equal(6, fireBolt.AttackBonus);
    Assert.Null(fireBolt.SaveDc);
    Assert.Equal("2d6", acidSplash.Damage);
    Assert.Equal(8 + 3 + 3, acidSplash.SaveDc);
    Assert.Equal("dex", acidSplash.SaveAbility);

    // undoing back to character level 4 brings the damage back down
    world.Context.ChangeTracker.Clear();
    await world.Service.UndoLastLevelAsync(id);
    world.Context.ChangeTracker.Clear();
    Assert.Equal("1d10", (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Fire Bolt").Damage);
  }

  [Fact]
  public async Task EldritchBlastGainsBeamsAndAgonizingBlastAddsCharisma()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 10, 12, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpAsync(id, "warlock", selections: [new("warlock.cantrips", "eldritch-blast")]);
    await world.LevelUpAsync(id, "warlock", selections:
    [
      new("warlock.invocations", "eldritch-invocation.agonizing-blast", Ref: "agonizing"),
      new("agonizing-blast.cantrip", "eldritch-blast", ParentRef: "agonizing"),
    ]);
    Assert.Equal("1d10+3", (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Eldritch Blast").Damage);

    await world.LevelUpManyAsync(id, "warlock", 3, "fiend-patron");
    AttackSheet blast = (await world.SheetAsync(id)).Attacks.Single(entry => entry.Name == "Eldritch Blast");

    Assert.Equal("2 × (1d10+3)", blast.Damage);
    Assert.Equal([DiceEnum.D10, DiceEnum.D10], blast.DamageDice);
  }

  [Fact]
  public async Task SpeciesTraitsFollowTheCharacterLevel()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync("dragonborn", TestWorld.Scores(13, 14, 13, 15, 10, 8), "sage",
      [new("dragonborn.draconic-ancestry", "red")], "int", "int", "con");

    await world.LevelUpManyAsync(id, "wizard", 2);
    await world.LevelUpManyAsync(id, "fighter", 2);
    CharacterSheet level4 = await world.SheetAsync(id);
    Assert.Contains("Fire", level4.Resistances);
    Assert.Equal("1d10", level4.Resources.Single(entry => entry.Key == "breath-weapon").Die);
    Assert.Equal(2, level4.Resources.Single(entry => entry.Key == "breath-weapon").Maximum);

    // Wizard 2 / Fighter 3 = character level 5: no class has 5 levels, yet the Breath Weapon and Draconic Flight improve
    await world.LevelUpAsync(id, "fighter", "eldritch-knight");
    CharacterSheet level5 = await world.SheetAsync(id);
    Assert.Equal("2d10", level5.Resources.Single(entry => entry.Key == "breath-weapon").Die);
    Assert.Equal(3, level5.Resources.Single(entry => entry.Key == "breath-weapon").Maximum);

    await world.Service.ToggleAsync(id, "dragonborn.draconic-flight", true);
    Assert.Contains((await world.SheetAsync(id)).OtherSpeeds, entry => entry.Label == "Fly" && entry.Value == 30);
  }
}
