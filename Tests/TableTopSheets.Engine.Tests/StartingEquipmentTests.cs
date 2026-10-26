using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

public class StartingEquipmentTests
{
  #region Catalog

  [Fact]
  public async Task EveryBackgroundAndImplementedClassOffersStartingEquipment()
  {
    await using TestWorld world = await TestWorld.CreateAsync();

    Assert.Equal(16, world.Catalog.Backgrounds.Count);
    Assert.All(world.Catalog.Backgrounds.Values, background => Assert.Equal(["a", "b"], background.StartingEquipment.Select(entry => entry.Key)));
    Assert.All(world.Catalog.Backgrounds.Values, background => Assert.Equal(50_00, background.StartingEquipment[1].CoinsCp));
    Assert.All(world.Catalog.Classes.Values, @class => Assert.NotEmpty(@class.StartingEquipment));
    Assert.Equal(["a", "b", "c"], world.Catalog.ClassesByKey["fighter"].StartingEquipment.Select(entry => entry.Key));
  }

  #endregion

  #region Background

  [Fact]
  public async Task BackgroundEquipment_AddsItemsNotesAndCoins()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Character character = await world.Service.CreateAsync(Request("acolyte", new StartingEquipmentRequest("a", Picks("holy-symbol", "holy-symbol-amulet"))));

    CharacterEvaluation evaluation = await world.EvaluateAsync(character.Id);
    Assert.Equal("a", evaluation.Character.BackgroundEquipment);
    Assert.Equal(8, evaluation.Character.PlayState.GoldPieces);
    Assert.Equal(
      ["book", "calligraphers-supplies", "holy-symbol-amulet", "parchment", "robe"],
      Items(evaluation)
        .Select(entry => entry.Key)
        .Order());
    Assert.Equal(10, Items(evaluation).Single(entry => entry.Key == "parchment").Item.Quantity);
    Assert.Equal("prayers", Items(evaluation).Single(entry => entry.Key == "book").Item.Notes);
  }

  [Fact]
  public async Task BackgroundEquipment_NeedsAPickFromTheOptions()
  {
    await using TestWorld world = await TestWorld.CreateAsync();

    RuleViolationException missing = await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.CreateAsync(Request("acolyte", new StartingEquipmentRequest("a"))));
    Assert.Contains("Pick an item for 'Holy Symbol'", missing.Message);

    RuleViolationException wrong = await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.CreateAsync(Request("acolyte", new StartingEquipmentRequest("a", Picks("holy-symbol", "dagger")))));
    Assert.Contains("'dagger' is not an option", wrong.Message);
  }

  [Fact]
  public async Task CoinsOnlyAlternative_AddsJustTheGold()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Character character = await world.Service.CreateAsync(Request("sage", new StartingEquipmentRequest("B")));

    CharacterEvaluation evaluation = await world.EvaluateAsync(character.Id);
    Assert.Empty(evaluation.Character.Items);
    Assert.Equal(50, evaluation.Character.PlayState.GoldPieces);
    Assert.Equal("b", evaluation.Character.BackgroundEquipment);
  }

  [Fact]
  public async Task SameAsAbove_PickAnswersTheOpenToolChoice()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Character character = await world.Service.CreateAsync(Request("artisan", new StartingEquipmentRequest("a", Picks("artisan", "smiths-tools"))));

    CharacterEvaluation evaluation = await world.EvaluateAsync(character.Id);
    Assert.Contains(evaluation.Selections, entry => entry.Choice.Key == "artisan.tool" && entry.Selection.OptionKey == "smiths-tools");
    Assert.Contains(Items(evaluation), entry => entry.Key == "smiths-tools");
    Assert.Equal(32, evaluation.Character.PlayState.GoldPieces);
  }

  [Fact]
  public async Task SameAsAbove_UsesTheToolAlreadyChosen()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Character character = await world.Service.CreateAsync(Request("guard", null, new SelectionRequest("guard.tool", "playing-cards")));

    RuleViolationException conflict = await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.TakeStartingEquipmentAsync(character.Id, StartingEquipmentSourceEnum.Background, new StartingEquipmentRequest("a", Picks("gaming-set", "dice"))));
    Assert.Contains("playing-cards", conflict.Message);

    world.Context.ChangeTracker.Clear();
    CharacterEvaluation evaluation = await world.Service.TakeStartingEquipmentAsync(character.Id, StartingEquipmentSourceEnum.Background, new StartingEquipmentRequest("a"));
    Assert.Contains(Items(evaluation), entry => entry.Key == "playing-cards");
    Assert.Equal(20, Items(evaluation).Single(entry => entry.Key == "bolts").Item.Quantity);
  }

  [Fact]
  public async Task StartingEquipment_IsTakenOnce()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Character character = await world.Service.CreateAsync(Request("sage", new StartingEquipmentRequest("b")));

    RuleViolationException again = await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.TakeStartingEquipmentAsync(character.Id, StartingEquipmentSourceEnum.Background, new StartingEquipmentRequest("a")));
    Assert.Contains("already taken", again.Message);
  }

  #endregion

  #region Class

  [Fact]
  public async Task ClassEquipment_ComesWithTheFirstLevel()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();

    await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.TakeStartingEquipmentAsync(id, StartingEquipmentSourceEnum.Class, new StartingEquipmentRequest("a")));
    world.Context.ChangeTracker.Clear();

    CharacterEvaluation evaluation = await world.Service.LevelUpAsync(id, new LevelUpRequest
    {
      ClassKey = "fighter",
      StartingEquipment = new StartingEquipmentRequest("b"),
      AllowIncomplete = true,
    });
    Assert.Equal("b", evaluation.Character.ClassEquipment);
    Assert.Equal(11, evaluation.Character.PlayState.GoldPieces);
    Assert.Equal(20, Items(evaluation).Single(entry => entry.Key == "arrows").Item.Quantity);
    Assert.Contains(Items(evaluation), entry => entry.Key == "studded-leather-armor");

    RuleViolationException second = await Assert.ThrowsAsync<RuleViolationException>(()
      => world.Service.LevelUpAsync(id, new LevelUpRequest { ClassKey = "fighter", StartingEquipment = new StartingEquipmentRequest("c"), AllowIncomplete = true }));
    Assert.Contains("first level", second.Message);
  }

  [Fact]
  public async Task UndoingTheFirstLevel_OffersTheClassEquipmentAgain()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    await world.Service.LevelUpAsync(id, new LevelUpRequest { ClassKey = "wizard", StartingEquipment = new StartingEquipmentRequest("a"), AllowIncomplete = true });

    CharacterEvaluation undone = await world.Service.UndoLastLevelAsync(id);
    Assert.Null(undone.Character.ClassEquipment);
    Assert.Contains(Items(undone), entry => entry.Key == "spellbook");
    Assert.Contains(Items(undone), entry => entry.Key == "arcane-focus-staff");

    IReadOnlyList<StartingEquipmentOffer> offers = await world.Service.GetStartingEquipmentAsync(id);
    Assert.Equal([StartingEquipmentSourceEnum.Background], offers.Select(entry => entry.Source));
  }

  [Fact]
  public async Task MonkPick_AnswersTheMonkToolChoice()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();

    CharacterEvaluation evaluation = await world.Service.LevelUpAsync(id, new LevelUpRequest
    {
      ClassKey = "monk",
      StartingEquipment = new StartingEquipmentRequest("a", Picks("artisan-or-musical-instrument", "flute")),
      AllowIncomplete = true,
    });

    Assert.Contains(evaluation.Selections, entry => entry.Choice.Key == "monk.tool" && entry.Selection.OptionKey == "flute");
    Assert.Equal(5, Items(evaluation).Single(entry => entry.Key == "dagger").Item.Quantity);

    IReadOnlyList<StartingEquipmentOffer> offers = await world.Service.GetStartingEquipmentAsync(id);
    StartingEquipmentOffer monk = offers.Single(entry => entry.Source == StartingEquipmentSourceEnum.Class);
    Assert.Equal("a", monk.Taken);
    Assert.Equal("flute", monk.SameAs["monk.tool"]);
  }

  #endregion

  #region Bundles

  [Fact]
  public async Task Ammunition_IsBoughtAndSoldByTheBundle()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    await world.Service.SetCoinsAsync(id, 0, 0, 0, 10, 0);

    CharacterItem arrows = await world.Service.AddItemAsync(id, "arrows", 20, purchase: true);
    Assert.Equal(9, (await world.EvaluateAsync(id)).Character.PlayState.GoldPieces);

    // a started bundle costs a whole one
    await world.Service.AddItemAsync(id, "arrows", 21, purchase: true);
    Assert.Equal(7, (await world.EvaluateAsync(id)).Character.PlayState.GoldPieces);

    // selling 20 arrows returns half of a bundle (5 SP)
    CharacterEvaluation sold = await world.Service.RemoveItemAsync(id, arrows.Id, 20, sell: true);
    Assert.Equal((7, 5), (sold.Character.PlayState.GoldPieces, sold.Character.PlayState.SilverPieces));
  }

  #endregion

  #region Helpers

  private static CreateCharacterRequest Request(string background, StartingEquipmentRequest? equipment, params SelectionRequest[] selections) => new()
  {
    Name = "Test",
    SpeciesKey = "dwarf",
    BackgroundKey = background,
    Abilities = TestWorld.StandardArray,
    Selections = selections,
    StartingEquipment = equipment,
    AllowIncomplete = true,
  };

  private static Dictionary<string, string> Picks(string entry, string item) => new() { [entry] = item };

  private static List<(string Key, CharacterItem Item)> Items(CharacterEvaluation evaluation)
    => evaluation.Character.Items
      .Select(entry => (evaluation.Catalog.Items[entry.ItemId].Key, entry))
      .ToList();

  #endregion
}
