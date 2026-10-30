using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class RangerTests
{
  // farmer increases Wisdom twice and Constitution once: Dex 15 (+2), Wis 17 (+3)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(10, 15, 13, 8, 15, 10);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "farmer", "wis", "wis", "con");

  [Fact]
  public async Task FavoredEnemyPreparesHuntersMarkWithFreeCasts()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "ranger");

    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], level1.SpellSlots());
    Assert.Equal(["hunters-mark"], AlwaysPrepared(level1));
    Assert.True(level1.Stat("proficiency.weapon.martial") > 0);
    Assert.True(level1.SaveProficiency("str") > 0);

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Equal(2, sheet.Spellcasting.Single().PreparedLimit);
    Assert.Equal(2, sheet.Resources.Single(entry => entry.Key == "favored-enemy").Maximum);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task DeftExplorerGivesExpertiseAndDruidicWarriorTwoDruidCantrips()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    await world.LevelUpAsync(id, "ranger", selections:
    [
      new("ranger.skills", "stealth"),
      new("ranger.skills", "perception"),
      new("ranger.skills", "survival"),
    ]);

    CharacterEvaluation level2 = await world.LevelUpAsync(id, "ranger", selections:
    [
      new("ranger.deft-explorer.expertise", "stealth"),
      new("ranger.fighting-style", "druidic-warrior"),
    ]);
    Assert.Equal(2 + 2 * 2, level2.SkillBonus("stealth"));
    Assert.Equal(3 + 2, level2.SkillBonus("perception"));
    List<KnownSpell> cantrips = level2
      .KnownSpells()
      .Where(entry => entry.Spell.Level == 0)
      .ToList();
    Assert.Equal(2, cantrips.Count);
    Assert.All(cantrips, entry => Assert.Contains("druid", entry.Spell.Lists));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task RovingAddsSpeedAndClimbAndSwimSpeeds()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "ranger", 5, "hunter");
    double walk = level5.Stat("speed.walk");
    Assert.Equal(1, level5.Stat("attack.extraAttacks"));
    Assert.Equal(0, level5.Stat("speed.climb"));

    CharacterEvaluation level6 = await world.LevelUpAsync(id, "ranger");
    Assert.Equal(walk + 10, level6.Stat("speed.walk"));
    Assert.Equal(walk + 10, level6.Stat("speed.climb"));
    Assert.Equal(walk + 10, level6.Stat("speed.swim"));
  }

  [Fact]
  public async Task GloomStalkerAddsDarkvisionWisdomToInitiativeAndItsSpells()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level2 = await world.LevelUpManyAsync(id, "ranger", 2);
    double darkvision = level2.Stat("sense.darkvision");
    double initiative = level2.Stat("initiative.bonus");

    CharacterEvaluation level3 = await world.LevelUpManyAsync(id, "ranger", 1, "gloom-stalker");
    Assert.Equal(darkvision + 60, level3.Stat("sense.darkvision"));
    Assert.Equal(initiative + 3, level3.Stat("initiative.bonus"));
    Assert.Equal(["disguise-self", "hunters-mark"], AlwaysPrepared(level3));
    Assert.Equal(3, (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "dreadful-strike").Maximum);
  }

  private static List<string> AlwaysPrepared(CharacterEvaluation evaluation) => evaluation
    .KnownSpells()
    .Where(entry => entry.AlwaysPrepared)
    .Select(entry => entry.Spell.Key)
    .Order()
    .ToList();
}
