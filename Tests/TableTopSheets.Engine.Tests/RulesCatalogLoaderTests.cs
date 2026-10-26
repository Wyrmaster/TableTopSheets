using System.Text.Json;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   The rules read for one character have to evaluate exactly like the complete rule set
/// </summary>
public class RulesCatalogLoaderTests
{
  [Fact]
  public async Task LineageGrantedFeatureOriginFeatAndMulticlass()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    // the High Elf lineage is a static option that grants a feature; the Sage grants Magic Initiate whose list grants another one
    Guid id = await world.CreateCharacterAsync("elf", TestWorld.Scores(13, 14, 13, 15, 10, 8), "sage", [new("elf.elven-lineage", "high-elf")], "int", "int", "con");
    await world.LevelUpManyAsync(id, "wizard", 2);
    await world.LevelUpManyAsync(id, "fighter", 2);
    await world.Service.CopyToSpellbookAsync(id, "sleep", "scroll");
    CharacterItem shirt = await world.Service.AddItemAsync(id, "chain-shirt");
    await world.Service.EquipAsync(id, shirt.Id, Common.Enum.Rules.EquipSlotEnum.Armor);
    await world.Service.AddItemAsync(id, "longsword");

    CharacterEvaluation evaluation = await AssertSameAsFullCatalogAsync(world, id);
    Assert.Contains(evaluation.Selections, entry => entry.StaticOption?.GrantsFeatureId != null);
  }

  [Fact]
  public async Task NestedInvocationChoicesAndToggles()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync("dragonborn", TestWorld.Scores(8, 14, 13, 10, 12, 15), "entertainer", [new("dragonborn.draconic-ancestry", "red")], "cha", "cha", "dex");
    await world.LevelUpAsync(id, "warlock", selections: [new("warlock.cantrips", "eldritch-blast")]);
    await world.LevelUpAsync(id, "warlock", selections:
    [
      new("warlock.invocations", "eldritch-invocation.agonizing-blast", Ref: "agonizing"),
      new("agonizing-blast.cantrip", "eldritch-blast", ParentRef: "agonizing"),
    ]);
    await world.LevelUpManyAsync(id, "warlock", 3, "fiend-patron");
    await world.Service.ToggleAsync(id, "dragonborn.draconic-flight", true);
    await world.Service.ToggleAsync(id, "spell-effect.mage-armor", true);

    await AssertSameAsFullCatalogAsync(world, id);
  }

  [Fact]
  public async Task FeatChosenOnLevelUpAndOpenChoices()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync(TestWorld.Scores(10, 15, 13, 13, 14, 8), "entertainer", "dex", "dex", "str");
    await world.LevelUpManyAsync(id, "fighter", 3, "eldritch-knight");
    await world.LevelUpAsync(id, "fighter", selections: [new SelectionRequest("fighter.ability-score-improvement-4", "medium-armor-master")]);
    CharacterItem breastplate = await world.Service.AddItemAsync(id, "breastplate");
    await world.Service.EquipAsync(id, breastplate.Id, Common.Enum.Rules.EquipSlotEnum.Armor);

    // a level with open choices: the options of pending choices come from the option lists of the slice
    CharacterService service = world.Service;
    Character character = await service.LoadAsync(id);
    await service.LevelUpAsync(character, new LevelUpRequest { ClassKey = "wizard", AllowIncomplete = true });
    await world.Context.SaveChangesAsync();

    CharacterEvaluation evaluation = await AssertSameAsFullCatalogAsync(world, id);
    Assert.NotEmpty(ChoiceService.Pending(evaluation));
  }

  /// <summary>
  ///   Compare the sheet, the pending choices with every option and the option descriptions against the complete catalog
  /// </summary>
  private static async Task<CharacterEvaluation> AssertSameAsFullCatalogAsync(TestWorld world, Guid id)
  {
    CharacterService service = world.Service;
    Character character = await service.LoadAsync(id);
    CharacterEvaluation slice = await service.EvaluateAsync(character);
    CharacterEvaluation full = new(world.Catalog, character);

    Assert.Equal(Json(CharacterSheetBuilder.Build(full)), Json(CharacterSheetBuilder.Build(slice)));
    Assert.Equal(Json(Describe(full)), Json(Describe(slice)));
    Assert.Equal
    (
      full.Selections.Select(entry => full.OptionName(entry.Choice, entry.Selection.OptionKey)),
      slice.Selections.Select(entry => slice.OptionName(entry.Choice, entry.Selection.OptionKey))
    );
    return slice;
  }

  private static object Describe(CharacterEvaluation evaluation) => ChoiceService.Pending(evaluation)
    .Select(pending =>
    {
      List<OptionAvailability> options = ChoiceService.EvaluateOptions(evaluation, pending.Choice, pending.Owner).ToList();
      bool embedded = options.Count(entry => entry.Available) <= ChoiceService.EmbeddedOptionLimit;
      return new
      {
        pending.Choice.Key,
        pending.ParentSelectionId,
        pending.Total,
        pending.Selected,
        Options = options.Select(entry => new
        {
          entry.Option,
          entry.Reason,
          Text = embedded && entry.Available ? evaluation.Catalog.Describe(pending.Choice, entry.Option.Key) : null,
        }),
      };
    })
    .ToList();

  private static string Json(object value) => JsonSerializer.Serialize(value);
}
