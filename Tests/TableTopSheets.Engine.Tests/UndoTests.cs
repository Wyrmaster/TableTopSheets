using System.Text.Json;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;

namespace TableTopSheets.Engine.Tests;

public class UndoTests
{
  [Theory]
  [InlineData("wizard", "evoker")]
  [InlineData("warlock", "fiend-patron")]
  [InlineData("fighter", "eldritch-knight")]
  [InlineData("sorcerer", "draconic-sorcery")]
  [InlineData("monk", "warrior-of-the-open-hand")]
  public async Task UndoRestoresEveryPreviousSheet(string classKey, string subclassKey)
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(13, 14, 13, 13, 13, 15), "sage", "int", "int", "con");

    List<string> snapshots = [Snapshot(await world.SheetAsync(id))];
    for (int level = 1; level <= 6; level++)
    {
      await world.LevelUpManyAsync(id, classKey, 1, subclassKey);
      world.Context.ChangeTracker.Clear();
      snapshots.Add(Snapshot(await world.SheetAsync(id)));
    }

    for (int level = 5; level >= 0; level--)
    {
      world.Context.ChangeTracker.Clear();
      await world.Service.UndoLastLevelAsync(id);
      world.Context.ChangeTracker.Clear();
      Assert.Equal(snapshots[level], Snapshot(await world.SheetAsync(id)));
    }
  }

  [Fact]
  public async Task UndoReportsInvalidPreparedSpellsInsteadOfDeletingThem()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(8, 14, 13, 15, 12, 10), "sage", "int", "int", "con");
    CharacterEvaluation wizard = await world.LevelUpManyAsync(id, "wizard", 5, "evoker");

    List<string> spellbook = wizard
      .SpellbookSpellIds()
      .Select(entry => wizard.Catalog.Spells[entry])
      .OrderByDescending(entry => entry.Level)
      .Select(entry => entry.Key)
      .ToList();
    await world.Service.SetPreparedSpellsAsync(id, "wizard", spellbook
      .Take(9)
      .ToList());

    world.Context.ChangeTracker.Clear();
    CharacterEvaluation undone = await world.Service.UndoLastLevelAsync(id);
    CharacterSheet sheet = await world.SheetAsync(id);

    Assert.Equal(9, undone.Character.PreparedSpells.Count);
    Assert.Contains(sheet.Violations, entry => entry.Code == "prepared.over-limit");
    Assert.Contains(sheet.Violations, entry => entry.Code == "prepared.level");
    Assert.Contains(sheet.Violations, entry => entry.Code == "prepared.pool");
  }

  private static string Snapshot(CharacterSheet sheet) => JsonSerializer.Serialize(sheet);
}
