using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class PaladinTests
{
  // farmer increases Strength twice and Constitution once: Str 17 (+3), Dex 10 (+0), Cha 15 (+2)
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(15, 10, 13, 8, 12, 15);

  private static Task<Guid> CreateAsync(TestWorld world) => world.CreateCharacterAsync(Scores, "farmer", "str", "str", "con");

  [Fact]
  public async Task LevelOneHasTwoSlotsTwoPreparedSpellsAndLayOnHands()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level1 = await world.LevelUpAsync(id, "paladin");

    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], level1.SpellSlots());
    Assert.True(level1.Stat("proficiency.armor.heavy") > 0);
    Assert.True(level1.Stat("proficiency.weapon.martial") > 0);

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Equal(2, sheet.Spellcasting.Single().PreparedLimit);
    Assert.Equal(5, sheet.Resources.Single(entry => entry.Key == "lay-on-hands").Maximum);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task HalfCasterSlotsExtraAttackAndLayOnHandsAtLevelFive()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "paladin", 5, "oath-of-vengeance");

    Assert.Equal([0, 4, 2, 0, 0, 0, 0, 0, 0, 0], level5.SpellSlots());
    Assert.Equal(1, level5.Stat("attack.extraAttacks"));
    Assert.Contains("divine-smite", AlwaysPrepared(level5));
    Assert.Contains("find-steed", AlwaysPrepared(level5));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.Equal(25, sheet.Resources.Single(entry => entry.Key == "lay-on-hands").Maximum);
    ResourceSheet channel = sheet.Resources.Single(entry => entry.Key == "paladin-channel-divinity");
    Assert.Equal(2, channel.Maximum);
    Assert.Equal("ShortRest", channel.Recharge);
  }

  [Fact]
  public async Task AuraOfProtectionAddsCharismaToEverySavingThrow()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "paladin", 5, "oath-of-glory");
    Assert.Equal(0, level5.SaveBonus("dex"));

    CharacterEvaluation level6 = await world.LevelUpAsync(id, "paladin");
    Assert.Equal(0 + 2, level6.SaveBonus("dex"));
    Assert.Equal(2 + 3 + 2, level6.SaveBonus("cha"));
  }

  [Fact]
  public async Task BlessedWarriorLearnsTwoClericCantrips()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);
    await world.LevelUpAsync(id, "paladin");

    CharacterEvaluation level2 = await world.LevelUpAsync(id, "paladin", selections: [new("paladin.fighting-style", "blessed-warrior")]);
    List<KnownSpell> cantrips = level2
      .KnownSpells()
      .Where(entry => entry.Spell.Level == 0)
      .ToList();
    Assert.Equal(2, cantrips.Count);
    Assert.All(cantrips, entry => Assert.Contains("cleric", entry.Spell.Lists));

    CharacterSheet sheet = await world.SheetAsync(id);
    Assert.DoesNotContain(sheet.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task OathOfDevotionSpellsAreAlwaysPrepared()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await CreateAsync(world);

    CharacterEvaluation level3 = await world.LevelUpManyAsync(id, "paladin", 3, "oath-of-devotion");
    Assert.Equal(["divine-smite", "protection-from-evil-and-good", "shield-of-faith"], AlwaysPrepared(level3));

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "paladin", 2);
    Assert.Equal(["aid", "divine-smite", "find-steed", "protection-from-evil-and-good", "shield-of-faith", "zone-of-truth"], AlwaysPrepared(level5));
  }

  private static List<string> AlwaysPrepared(CharacterEvaluation evaluation) => evaluation
    .KnownSpells()
    .Where(entry => entry.AlwaysPrepared)
    .Select(entry => entry.Spell.Key)
    .Order()
    .ToList();
}
