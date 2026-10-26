using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Engine.Tests;

public class WizardTests
{
  [Fact]
  public async Task WizardProgressionFollowsTheWizardTable()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 15, 12, 10), "sage", "int", "int", "con");

    CharacterEvaluation level1 = await world.LevelUpAsync(id, "wizard");
    CharacterSheet sheet1 = await world.SheetAsync(id);

    Assert.Equal(17, sheet1.Abilities.Single(entry => entry.Key == "int").Score);
    Assert.Equal(3, level1
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Wizard"));
    Assert.Equal(6, level1.SpellbookSpellIds().Count);
    Assert.Equal([0, 2, 0, 0, 0, 0, 0, 0, 0, 0], level1.SpellSlots());
    SpellcastingSheet casting = Assert.Single(sheet1.Spellcasting);
    Assert.Equal(4, casting.PreparedLimit);
    Assert.Equal(3 + 2, casting.AttackBonus);
    Assert.Equal(8 + 2 + 3, casting.SaveDc);

    // level 1 hit points: d6 maximum + Constitution (14 → +2) + Dwarven Toughness
    Assert.Equal(6 + 2 + 1, sheet1.HitPointMaximum);

    CharacterEvaluation level5 = await world.LevelUpManyAsync(id, "wizard", 4, "evoker");
    CharacterSheet sheet5 = await world.SheetAsync(id);

    Assert.Equal(4, level5
      .KnownSpells()
      .Count(entry => entry.Spell.Level == 0 && entry.Source == "Wizard"));
    // 6 + 2 per level after the first, plus Evocation Savant (one spell per spell slot level: 3)
    Assert.Equal(6 + 2 * 4 + 3, level5.SpellbookSpellIds().Count);
    Assert.Equal([0, 4, 3, 2, 0, 0, 0, 0, 0, 0], level5.SpellSlots());
    Assert.Equal(3, level5.MaxSpellLevel("wizard"));
    Assert.Equal(9, sheet5.Spellcasting.Single().PreparedLimit);
    Assert.Equal(3, sheet5.ProficiencyBonus);
    Assert.Equal(1, sheet5.AttacksPerAction);
    Assert.DoesNotContain(sheet5.Violations, entry => entry.Code == "choice.pending");
  }

  [Fact]
  public async Task PreparedSpellsComeFromTheSpellbookAndRitualsCanBeCastFromIt()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 15, 12, 10), "sage", "int", "int", "con");
    await world.LevelUpAsync(id, "wizard", selections:
    [
      new("wizard.spellbook-initial", "detect-magic"),
      new("wizard.spellbook-initial", "mage-armor"),
      new("wizard.spellbook-initial", "magic-missile"),
      new("wizard.spellbook-initial", "shield"),
      new("wizard.spellbook-initial", "sleep"),
      new("wizard.spellbook-initial", "thunderwave"),
    ]);

    await Assert.ThrowsAsync<RuleViolationException>(() => world.Service.SetPreparedSpellsAsync(id, "wizard", ["fireball"]));
    await Assert.ThrowsAsync<RuleViolationException>(() => world.Service.SetPreparedSpellsAsync(id, "wizard", ["burning-hands"]));
    await Assert.ThrowsAsync<RuleViolationException>(() =>
      world.Service.SetPreparedSpellsAsync(id, "wizard", ["mage-armor", "magic-missile", "shield", "sleep", "thunderwave"]));

    await world.Service.SetPreparedSpellsAsync(id, "wizard", ["mage-armor", "magic-missile", "shield", "sleep"]);
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Contains(sheet.Spells, entry => entry.Key == "magic-missile" && entry.Notes == string.Empty);
    // Detect Magic is not prepared but a ritual in the spellbook
    Assert.Contains(sheet.Spells, entry => entry.Key == "detect-magic" && entry.Notes == "ritual from spellbook");
    Assert.DoesNotContain(sheet.Spells, entry => entry.Key == "thunderwave");

    // copying a spell from a scroll extends the spellbook
    await world.Service.CopyToSpellbookAsync(id, "burning-hands", "scroll");
    await world.Service.SetPreparedSpellsAsync(id, "wizard", ["burning-hands"]);
  }
}
