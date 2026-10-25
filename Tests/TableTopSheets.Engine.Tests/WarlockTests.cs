using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

public class WarlockTests
{
  private static readonly IReadOnlyDictionary<string, byte> Scores = TestWorld.Scores(8, 14, 13, 10, 12, 15);

  [Fact]
  public async Task PactMagicAndInvocationsFollowTheWarlockTable()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "entertainer", "cha", "cha", "dex");

    CharacterEvaluation level1 = await world.LevelUpAsync(id, "warlock", selections: [new("warlock.invocations", "eldritch-invocation.pact-of-the-blade")]);
    Assert.Equal((1, 1), level1.PactSlots());
    Assert.Equal(new int[10], level1.SpellSlots());
    Assert.True(level1.HasFeature("eldritch-invocation.pact-of-the-blade"));

    CharacterEvaluation level2 = await world.LevelUpAsync(id, "warlock", selections:
    [
      new("warlock.invocations", "eldritch-invocation.agonizing-blast"),
      new("warlock.invocations", "eldritch-invocation.devils-sight"),
    ]);
    Assert.Equal(3, level2.Selections.Count(entry => entry.Choice.Key == "warlock.invocations"));
    Assert.Equal((2, 1), level2.PactSlots());

    // Thirsting Blade requires Warlock level 5: the level up is rejected and nothing changes
    Guid devilsSight = level2.Selections.Single(entry => entry.Selection.OptionKey == "eldritch-invocation.devils-sight").Selection.Id;
    await Assert.ThrowsAsync<RuleViolationException>(() => world.LevelUpAsync(id, "warlock", "fiend-patron",
      replacements: [new(devilsSight, "eldritch-invocation.thirsting-blade")]));
    world.Context.ChangeTracker.Clear();
    Assert.Equal(2, (await world.EvaluateAsync(id)).TotalLevel);

    await world.LevelUpAsync(id, "warlock", "fiend-patron");
    await world.LevelUpAsync(id, "warlock");
    CharacterEvaluation level5 = await world.LevelUpAsync(id, "warlock", selections: [new("warlock.invocations", "eldritch-invocation.thirsting-blade")]);
    CharacterSheet sheet5 = await world.SheetAsync(id);

    Assert.Equal((2, 3), level5.PactSlots());
    Assert.Equal(5, level5.Selections.Count(entry => entry.Choice.Key == "warlock.invocations"));
    Assert.Equal(2, sheet5.AttacksPerAction);
    Assert.Equal(2, sheet5.SpellSlots.Count);
    Assert.All(sheet5.SpellSlots, slot => Assert.Equal(3, slot.Level));
    // Fiend spells of level 3 are always prepared
    Assert.Contains(sheet5.Spells, entry => entry.Key == "fireball" && entry.Notes == "always prepared");
    Assert.Equal(6, sheet5.Spellcasting.Single().PreparedLimit);
  }

  [Fact]
  public async Task ReplacedInvocationComesBackWhenTheLevelIsUndone()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(Scores, "entertainer", "cha", "cha", "dex");
    await world.LevelUpAsync(id, "warlock", selections: [new("warlock.invocations", "eldritch-invocation.armor-of-shadows")]);
    CharacterEvaluation level2 = await world.LevelUpAsync(id, "warlock", selections:
    [
      new("warlock.invocations", "eldritch-invocation.agonizing-blast"),
      new("warlock.invocations", "eldritch-invocation.devils-sight"),
    ]);
    Guid devilsSight = level2.Selections.Single(entry => entry.Selection.OptionKey == "eldritch-invocation.devils-sight").Selection.Id;

    CharacterEvaluation level3 = await world.LevelUpAsync(id, "warlock", "fiend-patron", replacements: [new(devilsSight, "eldritch-invocation.eldritch-mind")]);
    Assert.False(level3.HasFeature("eldritch-invocation.devils-sight"));
    Assert.True(level3.HasFeature("eldritch-invocation.eldritch-mind"));

    world.Context.ChangeTracker.Clear();
    CharacterEvaluation undone = await world.Service.UndoLastLevelAsync(id);

    Assert.Equal(2, undone.TotalLevel);
    Assert.True(undone.HasFeature("eldritch-invocation.devils-sight"));
    Assert.False(undone.HasFeature("eldritch-invocation.eldritch-mind"));
    Assert.Empty(undone.Subclasses);

    // the database agrees with the in memory result
    world.Context.ChangeTracker.Clear();
    Character reloaded = await world.Service.LoadAsync(id);
    Assert.Equal(2, reloaded.Levels.Count);
    Assert.DoesNotContain(reloaded.Choices, entry => entry.OptionKey == "eldritch-invocation.eldritch-mind");
    Assert.All(reloaded.Choices, entry => Assert.Null(entry.SupersededByLevelId));
  }

  [Fact]
  public async Task OnlyOneSwapPerChoiceAndOnlyWhenLevelingTheClass()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 13, 12, 15), "entertainer", "cha", "cha", "dex");
    await world.LevelUpAsync(id, "warlock");
    CharacterEvaluation level2 = await world.LevelUpAsync(id, "warlock");
    List<Guid> invocations = level2.Selections.Where(entry => entry.Choice.Key == "warlock.invocations").Select(entry => entry.Selection.Id).ToList();

    await Assert.ThrowsAsync<RuleViolationException>(() => world.LevelUpAsync(id, "warlock", "fiend-patron", replacements:
    [
      new(invocations[0], "eldritch-invocation.eldritch-mind"),
      new(invocations[1], "eldritch-invocation.mask-of-many-faces"),
    ]));
    world.Context.ChangeTracker.Clear();

    // gaining a Wizard level doesn't allow changing Warlock invocations
    await Assert.ThrowsAsync<RuleViolationException>(() => world.LevelUpAsync(id, "wizard", replacements: [new(invocations[0], "eldritch-invocation.eldritch-mind")]));
  }
}
