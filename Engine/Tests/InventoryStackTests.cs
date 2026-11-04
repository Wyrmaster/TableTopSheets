using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   Several pieces at once: gear, tools and thrown weapons stack, other weapons and armor get an entry each
/// </summary>
public class InventoryStackTests
{
  [Fact]
  public async Task GearAndThrownWeaponsStack_OtherWeaponsAndArmorDont()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();

    await world.Service.AddItemAsync(id, "arrows", 20);
    await world.Service.AddItemAsync(id, "arrows", 20);
    await world.Service.AddItemAsync(id, "dart", 10);
    await world.Service.AddItemAsync(id, "longsword", 3);
    await world.Service.AddItemAsync(id, "chain-mail", 2);

    List<CharacterItem> items = (await world.EvaluateAsync(id)).Character.Items.ToList();
    Assert.Equal([40], await QuantitiesAsync(world, items, "arrows"));
    Assert.Equal([10], await QuantitiesAsync(world, items, "dart"));
    Assert.Equal([1, 1, 1], await QuantitiesAsync(world, items, "longsword"));
    Assert.Equal([1, 1], await QuantitiesAsync(world, items, "chain-mail"));
  }

  [Fact]
  public async Task PurchaseOfSeveralPiecesPaysEachPiece()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    await world.Service.SetCoinsAsync(id, 0, 0, 0, 10, 0);

    // 10 darts at 5 CP
    await world.Service.AddItemAsync(id, "dart", 10, purchase: true);
    CharacterPlayState purse = (await world.EvaluateAsync(id)).Character.PlayState;
    Assert.Equal(1000 - 50, Purse.Value(purse));

    // one arrow: a twentieth of 1 GP
    await world.Service.AddItemAsync(id, "arrows", purchase: true);
    Assert.Equal(1000 - 50 - 5, Purse.Value((await world.EvaluateAsync(id)).Character.PlayState));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(CharacterService.MaxQuantity + 1)]
  public async Task QuantityOutOfRangeIsRejected(int quantity)
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();

    await Assert.ThrowsAsync<RuleViolationException>(() => world.Service.AddItemAsync(id, "dart", quantity));
  }

  [Fact]
  public async Task HandsTakeOnePieceOfAStack_AndPutItBack()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    CharacterItem daggers = await world.Service.AddItemAsync(id, "dagger", 5);

    await world.Service.EquipAsync(id, daggers.Id, EquipSlotEnum.MainHand);
    await world.Service.EquipAsync(id, daggers.Id, EquipSlotEnum.OffHand);
    CharacterEvaluation armed = await world.EvaluateAsync(id);
    Assert.Equal(
      [(EquipSlotEnum.None, 3), (EquipSlotEnum.MainHand, 1), (EquipSlotEnum.OffHand, 1)],
      Slots(armed.Character.Items));

    // the off hand dagger goes back to the stowed stack
    CharacterItem offHand = armed.Character.Items.Single(entry => entry.EquipSlot == EquipSlotEnum.OffHand);
    CharacterEvaluation stowed = await world.Service.EquipAsync(id, offHand.Id, EquipSlotEnum.None);
    Assert.Equal([(EquipSlotEnum.None, 4), (EquipSlotEnum.MainHand, 1)], Slots(stowed.Character.Items));

    // the whole stack can be readied
    CharacterEvaluation ready = await world.Service.EquipAsync(id, daggers.Id, EquipSlotEnum.Ready);
    Assert.Equal([(EquipSlotEnum.MainHand, 1), (EquipSlotEnum.Ready, 4)], Slots(ready.Character.Items));
  }

  [Fact]
  public async Task NewPiecesJoinTheStowedStack()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    CharacterItem darts = await world.Service.AddItemAsync(id, "dart", 10);
    await world.Service.EquipAsync(id, darts.Id, EquipSlotEnum.Ready);

    CharacterItem more = await world.Service.AddItemAsync(id, "dart", 5);

    Assert.NotEqual(darts.Id, more.Id);
    Assert.Equal(
      [(EquipSlotEnum.None, 5), (EquipSlotEnum.Ready, 10)],
      Slots((await world.EvaluateAsync(id)).Character.Items));
  }

  [Fact]
  public async Task ExistingRowsOfAStackAreMergedWhenLoaded()
  {
    await using TestWorld world = await TestWorld.CreateAsync();
    Guid id = await world.CreateCharacterAsync();
    // rows as inventories held them before items stacked
    await AddRowsAsync(world, id, "dart", 1, 1, 1);
    await AddRowsAsync(world, id, "arrows", 20, 20);
    await AddRowsAsync(world, id, "longsword", 1, 1);
    await AddRowsAsync(world, id, "dart", EquipSlotEnum.Ready, 2, 3);
    await AddRowsAsync(world, id, "dart", EquipSlotEnum.MainHand, 1);
    world.Context.CharacterItems.Add(new CharacterItem
    {
      Id = Guid.NewGuid(),
      CharacterId = id,
      ItemId = await ItemIdAsync(world, "dart"),
      Notes = "silvered",
    });
    await world.Context.SaveChangesAsync();

    CharacterEvaluation loaded = await world.EvaluateAsync(id);

    List<CharacterItem> items = loaded.Character.Items.ToList();
    Assert.Equal([40], await QuantitiesAsync(world, items, "arrows"));
    Assert.Equal([1, 1], await QuantitiesAsync(world, items, "longsword"));
    Guid dart = await ItemIdAsync(world, "dart");
    Assert.Equal(
      [(EquipSlotEnum.None, 1), (EquipSlotEnum.None, 3), (EquipSlotEnum.MainHand, 1), (EquipSlotEnum.Ready, 5)],
      Slots(items.Where(entry => entry.ItemId == dart))
        .OrderBy(entry => entry.Item1)
        .ThenBy(entry => entry.Item2)
        .ToList());
    // the merge is saved: a fresh load finds the same rows
    Assert.Equal(items.Count, (await world.EvaluateAsync(id)).Character.Items.Count);
  }

  private static Task AddRowsAsync(TestWorld world, Guid characterId, string key, params int[] quantities)
    => AddRowsAsync(world, characterId, key, EquipSlotEnum.None, quantities);

  private static async Task AddRowsAsync(TestWorld world, Guid characterId, string key, EquipSlotEnum slot, params int[] quantities)
  {
    Guid itemId = await ItemIdAsync(world, key);
    foreach (int quantity in quantities)
    {
      world.Context.CharacterItems.Add(new CharacterItem
      {
        Id = Guid.NewGuid(),
        CharacterId = characterId,
        ItemId = itemId,
        Quantity = quantity,
        EquipSlot = slot,
      });
    }

    await world.Context.SaveChangesAsync();
  }

  private static Task<Guid> ItemIdAsync(TestWorld world, string key) => world.Context.Items
    .Where(entry => entry.Key == key)
    .Select(entry => entry.Id)
    .SingleAsync();

  private static async Task<List<int>> QuantitiesAsync(TestWorld world, IEnumerable<CharacterItem> items, string key)
  {
    Guid itemId = await ItemIdAsync(world, key);
    return items
      .Where(entry => entry.ItemId == itemId)
      .Select(entry => entry.Quantity)
      .ToList();
  }

  private static List<(EquipSlotEnum, int)> Slots(IEnumerable<CharacterItem> items) => items
    .OrderBy(entry => entry.EquipSlot)
    .Select(entry => (entry.EquipSlot, entry.Quantity))
    .ToList();
}
