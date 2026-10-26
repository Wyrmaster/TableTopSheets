using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class BardTests
{
  // entertainer increases Charisma twice and Dexterity once: Dex 15 (+2), Int 10 (+0), Cha 17 (+3)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(8, 14, 13, 10, 12, 15);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "entertainer", "cha", "cha", "dex");

  [Fact]
  public async Task BardProgressionFollowsTheBardTable()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level1 = await world.LevelUpAsync(id, "bard", selections:
    [
      new("bard.skills", "deception"),
      new("bard.skills", "persuasion"),
      new("bard.skills", "insight"),
    ]);
    CharacterSheet sheet1 = await world.SheetAsync(id);

    Assert.Equal(2, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Bard"));
    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], level1.SpellSlots());
    Assert.Equal(4, sheet1.Spellcasting.Single().PreparedLimit);

    // one use per Charisma modifier, a d6, back on a Long Rest until Font of Inspiration
    ResourceSheet inspiration1 = sheet1.Resources.Single(entry => entry.Key == "bardic-inspiration");
    Assert.Equal(3, inspiration1.Maximum);
    Assert.Equal("1d6", inspiration1.Die);
    Assert.Equal("LongRest", inspiration1.Recharge);

    CharacterEvaluation level2 = await world.LevelUpAsync(id, "bard", selections:
    [
      new("bard.expertise", "deception"),
      new("bard.expertise", "persuasion"),
    ]);

    // Expertise doubles the Proficiency Bonus (+2), Jack of All Trades adds half of it to unproficient skills
    Assert.Equal(3 + 2 * 2, level2.SkillBonus("deception"));
    Assert.Equal(1 + 2, level2.SkillBonus("insight"));
    Assert.Equal(0 + 1, level2.SkillBonus("investigation"));

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "bard", 3, "college-of-lore");
    CharacterSheet sheet5 = await world.SheetAsync(id);

    Assert.Equal(3, level5
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Bard"));
    Assert.Equal([0, 4, 3, 2, 0, 0, 0, 0, 0, 0], level5.SpellSlots());
    Assert.Equal(9, sheet5.Spellcasting.Single().PreparedLimit);

    // Font of Inspiration: a d8 that comes back on a Short Rest
    ResourceSheet inspiration5 = sheet5.Resources.Single(entry => entry.Key == "bardic-inspiration");
    Assert.Equal("1d8", inspiration5.Die);
    Assert.Equal("ShortRest", inspiration5.Recharge);
    Assert.DoesNotContain(sheet5.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task DazzlingFootworkIsUnarmoredDefenseWithCharisma()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    await world.LevelUpManyAsync(id, "bard", 3, "college-of-dance");
    CharacterSheet sheet = await world.SheetAsync(id);

    // 10 + Dexterity (+2) + Charisma (+3)
    Assert.Equal(15, sheet.ArmorClass);
    Assert.Equal([new SourceValue("Unarmored Defense (Bard)", 15)], sheet.ArmorClassSources);
  }

  [Fact]
  public async Task CollegeOfValorTrainsMartialWeaponsAndAttacksTwice()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level6 = await world.LevelUpManyAsync(id, "bard", 6, "college-of-valor");

    Assert.True(level6.Stat("proficiency.weapon.martial") > 0);
    Assert.True(level6.Stat("proficiency.armor.medium") > 0);
    Assert.Equal(2, (await world.SheetAsync(id)).AttacksPerAction);
  }
}
