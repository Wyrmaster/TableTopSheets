using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Engine.Tests;

public class MulticlassTests
{
  [Fact]
  public async Task SpellSlotsCombineCasterLevelsButPreparationStaysPerClass()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 12, 14, 13, 10, 15), "sage", "int", "int", "con");
    await world.LevelUpManyAsync(id, "sorcerer", 3, "draconic-sorcery");
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, "wizard", 2);

    // Sorcerer 3 + Wizard 2 = caster level 5
    Assert.Equal([0, 4, 3, 2, 0, 0, 0, 0, 0, 0], evaluation.SpellSlots());
    Assert.Equal(2, evaluation.MaxSpellLevel("sorcerer"));
    Assert.Equal(1, evaluation.MaxSpellLevel("wizard"));
    Assert.Equal(2, evaluation.SpellcastingProfiles.Count);
    Assert.All(evaluation.Selections.Where(entry => entry.Choice.Key == "sorcerer.prepared"),
      entry => Assert.True(evaluation.Catalog.SpellsByKey[entry.Selection.OptionKey].Level <= 2));
  }

  [Fact]
  public async Task EldritchKnightCountsAThirdOfFighterLevels()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(15, 13, 14, 13, 10, 8), "sage", "int", "int", "con");
    CharacterEvaluation fighter3 = await world.LevelUpManyAsync(id, "fighter", 3, "eldritch-knight");

    // single class Eldritch Knight uses its own table
    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], fighter3.SpellSlots());
    CharacterEvaluation fighter4 = await world.LevelUpAsync(id, "fighter");
    Assert.Equal([0, 3, 0, 0, 0, 0, 0, 0, 0, 0], fighter4.SpellSlots());

    // Fighter 4 (Eldritch Knight: 4 / 3 = 1) + Wizard 3 = caster level 4
    CharacterEvaluation multiclass = await world.LevelUpManyAsync(id, "wizard", 3, "evoker");
    Assert.Equal([0, 4, 3, 0, 0, 0, 0, 0, 0, 0], multiclass.SpellSlots());
    Assert.Equal(1, multiclass.MaxSpellLevel("fighter"));
    Assert.Equal(2, multiclass.MaxSpellLevel("wizard"));
  }

  [Fact]
  public async Task PactMagicSlotsStaySeparate()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 10, 12, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpManyAsync(id, "warlock", 3, "fiend-patron");
    CharacterEvaluation evaluation = await world.LevelUpManyAsync(id, "sorcerer", 2);
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal((2, 2), evaluation.PactSlots());
    Assert.Equal([0, 3, 0, 0, 0, 0, 0, 0, 0, 0], evaluation.SpellSlots());
    Assert.Equal(3, sheet.SpellSlots.Count(entry => entry.RefreshPeriod == Common.Enum.Magic.PeriodEnum.LongRest));
    Assert.Equal(2, sheet.SpellSlots.Count(entry => entry.RefreshPeriod == Common.Enum.Magic.PeriodEnum.ShortRest));
  }

  [Fact]
  public async Task ExtraAttackFromTwoClassesDoesNotStack()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(14, 15, 13, 8, 12, 10), "farmer", "con", "con", "wis");
    await world.LevelUpManyAsync(id, "fighter", 5, "eldritch-knight");
    Assert.Equal(2, (await world.SheetAsync(id)).AttacksPerAction);

    await world.LevelUpManyAsync(id, "monk", 5, "warrior-of-the-open-hand");
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(10, sheet.Level);
    Assert.Equal(2, sheet.AttacksPerAction);
    Assert.Equal(4, sheet.ProficiencyBonus);
  }

  [Fact]
  public async Task MulticlassingRequiresPrimaryAbilities()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 12, 14, 15, 10, 13), "sage", "int", "int", "con");
    await world.LevelUpAsync(id, "wizard");

    RuleViolationException exception = await Assert.ThrowsAsync<RuleViolationException>(() => world.LevelUpAsync(id, "fighter"));
    Assert.Contains(exception.Errors, entry => entry.Contains("Fighter"));
    Assert.DoesNotContain(exception.Errors, entry => entry.Contains("(Wizard)"));
  }

  [Fact]
  public async Task MulticlassGrantsOnlyPartOfTheStartingProficiencies()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 14, 14, 15, 10, 8), "sage", "int", "int", "con");
    await world.LevelUpAsync(id, "wizard");
    await world.LevelUpAsync(id, "fighter");
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(ArmorEnum.Light | ArmorEnum.Medium | ArmorEnum.Shield, sheet.ArmorTraining);
    Assert.Contains("Martial Weapons", sheet.WeaponProficiencies);
    Assert.Equal(["int", "wis"], sheet.Abilities
      .Where(entry => entry.SaveProficiency > 0)
      .Select(entry => entry.Key)
      .Order());
    // Wizard 1: 6 + Fighter 2nd level: 6 (average of d10), Constitution 15 (+2) and Dwarven Toughness (+1) per level
    Assert.Equal(6 + 6 + 2 * (2 + 1), sheet.HitPointMaximum);
    Assert.Equal("Wizard 1 / Fighter 1", sheet.Classes);
  }
}
