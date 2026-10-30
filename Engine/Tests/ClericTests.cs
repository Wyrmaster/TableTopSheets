using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class ClericTests
{
  // farmer increases Wisdom twice and Constitution once: Int 10 (+0), Wis 17 (+3)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(10, 12, 14, 10, 15, 8);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "farmer", "wis", "wis", "con");

  [Fact]
  public async Task ThaumaturgeKnowsAnExtraCantripAndAddsWisdomToArcanaAndReligion()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "cleric", selections:
    [
      new("cleric.divine-order", "thaumaturge"),
      new("cleric.skills", "history"),
      new("cleric.skills", "insight"),
    ]);

    Assert.Equal(3 + 1, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Cleric"));
    Assert.Equal(0 + 3, level1.SkillBonus("religion"));
    Assert.Equal(0 + 3, level1.SkillBonus("arcana"));
    Assert.Equal(0, level1.Stat("proficiency.armor.heavy"));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Equal(4, sheet.Spellcasting.Single().PreparedLimit);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task ProtectorTrainsHeavyArmorAndChannelDivinityStartsAtTwoUses()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "cleric", selections: [new("cleric.divine-order", "protector")]);

    Assert.True(level1.Stat("proficiency.armor.heavy") > 0);
    Assert.True(level1.Stat("proficiency.weapon.martial") > 0);
    Assert.Equal(3, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Cleric"));

    await world.LevelUpAsync(id, "cleric");
    ResourceSheet channel = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "channel-divinity");
    Assert.Equal(2, channel.Maximum);
    Assert.Equal("ShortRest", channel.Recharge);
  }

  [Fact]
  public async Task LifeDomainSpellsAreAlwaysPrepared()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level3 = await world.LevelUpManyAsync(id, "cleric", 3, "life-domain");
    Assert.Equal(["aid", "bless", "cure-wounds", "lesser-restoration"], AlwaysPrepared(level3));

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "cleric", 2);
    Assert.Equal(["aid", "bless", "cure-wounds", "lesser-restoration", "mass-healing-word", "revivify"], AlwaysPrepared(level5));
    Assert.Equal([0, 4, 3, 2, 0, 0, 0, 0, 0, 0], level5.SpellSlots());
  }

  [Fact]
  public async Task WardingFlareRechargesOnAShortRestFromLevelSix()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    await world.LevelUpManyAsync(id, "cleric", 5, "light-domain");
    ResourceSheet level5 = (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "warding-flare");
    Assert.Equal(3, level5.Maximum);
    Assert.Equal("LongRest", level5.Recharge);

    await world.LevelUpAsync(id, "cleric");
    Assert.Equal("ShortRest", (await world.SheetAsync(id)).Resources.Single(entry => entry.Key == "warding-flare").Recharge);
  }

  private static List<string> AlwaysPrepared(CharacterEvaluation evaluation) => evaluation
    .KnownSpells()
    .Where(entry => entry.AlwaysPrepared)
    .Select(entry => entry.Spell.Key)
    .Order()
    .ToList();
}
