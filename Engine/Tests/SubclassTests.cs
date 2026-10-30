using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class SubclassTests
{
  [Theory]
  [InlineData("bard", "college-of-dance")]
  [InlineData("bard", "college-of-glamour")]
  [InlineData("bard", "college-of-lore")]
  [InlineData("bard", "college-of-valor")]
  [InlineData("cleric", "life-domain")]
  [InlineData("cleric", "light-domain")]
  [InlineData("cleric", "trickery-domain")]
  [InlineData("cleric", "war-domain")]
  [InlineData("fighter", "battle-master")]
  [InlineData("fighter", "champion")]
  [InlineData("fighter", "psi-warrior")]
  [InlineData("monk", "warrior-of-mercy")]
  [InlineData("monk", "warrior-of-shadow")]
  [InlineData("monk", "warrior-of-the-elements")]
  [InlineData("sorcerer", "aberrant-sorcery")]
  [InlineData("sorcerer", "clockwork-sorcery")]
  [InlineData("sorcerer", "wild-magic-sorcery")]
  [InlineData("warlock", "archfey-patron")]
  [InlineData("warlock", "celestial-patron")]
  [InlineData("warlock", "great-old-one-patron")]
  [InlineData("wizard", "abjurer")]
  [InlineData("wizard", "diviner")]
  [InlineData("wizard", "illusionist")]
  public async Task EverySubclassReachesLevelThreeWithoutOpenChoices(string classKey, string subclassKey)
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, classKey, 3, subclassKey);
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Contains(evaluation.Features, entry => entry.Feature.Key.StartsWith($"{subclassKey}.", StringComparison.Ordinal));
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task BattleMasterLearnsThreeManeuversFueledByFourSuperiorityDice()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, "fighter", 3, "battle-master");

    Assert.Equal(3, evaluation.Features.Count(entry => entry.Feature.Category == "maneuver"));
    ResourceSheet dice = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "superiority-dice");
    Assert.Equal(4, dice.Maximum);
    Assert.Equal("1d8", dice.Die);
    Assert.Equal("ShortRest", dice.Recharge);
  }

  [Fact]
  public async Task ChampionHasAdvantageOnInitiativeAndAthletics()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, "fighter", 3, "champion");

    Assert.True(evaluation.Stat("initiative.advantage") > 0);
    Assert.True(evaluation.Stat("skill.athletics.advantage") > 0);
  }

  [Fact]
  public async Task ShadowArtsExtendsTheDwarfsDarkvision()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    double darkvision = (await world.EvaluateAsync(id)).Stat("sense.darkvision");
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, "monk", 3, "warrior-of-shadow");

    Assert.Equal(darkvision + 60, evaluation.Stat("sense.darkvision"));
    Assert.Contains(evaluation.KnownSpells(), entry => entry.Spell.Key == "minor-illusion");
  }

  [Fact]
  public async Task HealingLightHasOneDiePerWarlockLevelPlusOne()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    await world.LevelUpManyAsync(id, "warlock", 3, "celestial-patron");

    ResourceSheet pool = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "healing-light");
    Assert.Equal(1 + 3, pool.Maximum);
    Assert.Equal("1d6", pool.Die);
  }

  [Fact]
  public async Task PortentRollsThreeDiceFromLevelFourteen()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();

    await world.LevelUpManyAsync(id, "wizard", 13, "diviner");
    Assert.Equal(2, (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "portent").Maximum);

    await world.LevelUpAsync(id, "wizard");
    Assert.Equal(3, (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "portent").Maximum);
  }
}
