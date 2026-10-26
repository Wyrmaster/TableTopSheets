using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class DruidTests
{
  // farmer increases Wisdom twice and Constitution once: Int 10 (+0), Wis 17 (+3)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(10, 12, 14, 10, 15, 8);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "farmer", "wis", "wis", "con");

  [Fact]
  public async Task MagicianKnowsAnExtraCantripAndAddsWisdomToArcanaAndNature()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "druid", selections:
    [
      new("druid.primal-order", "magician"),
      new("druid.skills", "medicine"),
      new("druid.skills", "perception"),
    ]);

    Assert.Equal(2 + 1, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Druid"));
    Assert.Equal(0 + 3, level1.SkillBonus("arcana"));
    // farmer is proficient in Nature
    Assert.Equal(2 + 3, level1.SkillBonus("nature"));
    Assert.Equal(0, level1.Stat("proficiency.armor.medium"));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Equal(4, sheet.Spellcasting.Single().PreparedLimit);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task WardenTrainsMartialWeaponsAndMediumArmor()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "druid", selections: [new("druid.primal-order", "warden")]);

    Assert.True(level1.Stat("proficiency.weapon.martial") > 0);
    Assert.True(level1.Stat("proficiency.armor.medium") > 0);
    Assert.Equal(0, level1.Stat("proficiency.armor.heavy"));
    Assert.Equal(2, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Druid"));
  }

  [Fact]
  public async Task DruidicGrantsTheLanguageAndSpeakWithAnimals()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "druid");

    Assert.Contains("speak-with-animals", AlwaysPrepared(level1));
    Assert.True(level1.SaveProficiency("int") > 0);
    Assert.True(level1.SaveProficiency("wis") > 0);

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Contains("Druidic", sheet.Languages);
  }

  [Fact]
  public async Task WildShapeHasTwoUsesFromLevelTwoAndThreeFromLevelSix()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    await world.LevelUpManyAsync(id, "druid", 2);
    ResourceSheet level2 = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "wild-shape");
    Assert.Equal(2, level2.Maximum);
    Assert.Equal("ShortRest", level2.Recharge);

    await world.LevelUpManyAsync(id, "druid", 4, "circle-of-the-stars");
    Assert.Equal(3, (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "wild-shape").Maximum);
  }

  [Fact]
  public async Task CircleOfTheMoonSpellsAreAlwaysPrepared()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level3 = await world.LevelUpManyAsync(id, "druid", 3, "circle-of-the-moon");
    Assert.Equal(["cure-wounds", "moonbeam", "speak-with-animals", "starry-wisp"], AlwaysPrepared(level3));

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "druid", 2);
    Assert.Contains("conjure-animals", AlwaysPrepared(level5));
    Assert.Equal([0, 4, 3, 2, 0, 0, 0, 0, 0, 0], level5.SpellSlots());
  }

  [Fact]
  public async Task CircleOfTheLandPreparesTheSpellsOfTheChosenLand()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    await world.LevelUpManyAsync(id, "druid", 2);

    CharacterEvaluation level3 = await world.LevelUpAsync(id, "druid", "circle-of-the-land", [new("circle-of-the-land.land", "polar")]);
    Assert.Equal(["fog-cloud", "hold-person", "ray-of-frost", "speak-with-animals"], AlwaysPrepared(level3));
    Assert.Equal(0, level3.Stat("damage.resist.cold"));
  }

  private static List<string> AlwaysPrepared(CharacterEvaluation evaluation) => evaluation
    .KnownSpells()
    .Where(entry => entry.AlwaysPrepared)
    .Select(entry => entry.Spell.Key)
    .Order()
    .ToList();
}
