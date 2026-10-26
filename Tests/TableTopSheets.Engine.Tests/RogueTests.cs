using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class RogueTests
{
  // farmer increases Wisdom twice and Constitution once: Dex 15 (+2), Int 14 (+2)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(8, 15, 13, 14, 12, 10);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "farmer", "wis", "wis", "con");

  [Fact]
  public async Task ExpertiseThievesCantAndFinesseWeapons()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "rogue", selections:
    [
      new("rogue.skills", "stealth"),
      new("rogue.skills", "perception"),
      new("rogue.skills", "acrobatics"),
      new("rogue.skills", "investigation"),
      new("rogue.expertise", "stealth"),
      new("rogue.expertise", "investigation"),
    ]);

    Assert.Equal(2 + 2 * 2, level1.SkillBonus("stealth"));
    Assert.Equal(2 + 2 * 2, level1.SkillBonus("investigation"));
    Assert.Equal(2 + 2, level1.SkillBonus("acrobatics"));
    Assert.True(level1.Stat("proficiency.weapon.rapier") > 0);
    Assert.Equal(0, level1.Stat("proficiency.weapon.martial"));
    Assert.True(level1.SaveProficiency("int") > 0);

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Contains("Thieves' Cant", sheet.Languages);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task ArcaneTricksterKnowsMageHandAndCastsAsAThirdCaster()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level3 = await world.LevelUpManyAsync(id, "rogue", 3, "arcane-trickster");
    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], level3.SpellSlots());
    List<KnownSpell> cantrips = level3
      .KnownSpells()
      .Where(entry => entry.Spell.Level == 0)
      .ToList();
    Assert.Equal(3, cantrips.Count);
    Assert.Contains(cantrips, entry => entry.Spell.Key == "mage-hand");
    Assert.Equal(3, level3
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 1));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task SoulknifeHasSixD8EnergyDiceAtLevelFive()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    await world.LevelUpManyAsync(id, "rogue", 5, "soulknife");
    ResourceSheet dice = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "soulknife-energy-dice");
    Assert.Equal(6, dice.Maximum);
    Assert.Equal("1d8", dice.Die);
  }

  [Fact]
  public async Task SlipperyMindAddsWisdomAndCharismaSaves()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level14 = await world.LevelUpManyAsync(id, "rogue", 14, "thief");
    Assert.Equal(0, level14.SaveProficiency("cha"));
    Assert.True(level14.Stat("speed.climb") > 0);

    CharacterEvaluation level15 = await world.LevelUpAsync(id, "rogue");
    Assert.True(level15.SaveProficiency("wis") > 0);
    Assert.True(level15.SaveProficiency("cha") > 0);
  }
}
