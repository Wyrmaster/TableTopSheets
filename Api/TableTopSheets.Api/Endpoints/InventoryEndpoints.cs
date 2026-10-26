using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Items (add, buy, remove, sell), coins, equipment slots, attunement, toggles and the armor class calculation
/// </summary>
public static class InventoryEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters/{id:guid}").WithTags("Inventory").WithCharacterProblems().RequireUser();

    group.MapPost("/items", AddItem).WithName("AddItem")
      .WithSummary("Add an item to the inventory")
      .WithDescription("With purchase true the item's cost (times the quantity) is paid from the coins; change is returned. Not enough coins answer 422.");
    group.MapDelete("/items/{itemId:guid}", RemoveItem).WithName("RemoveItem")
      .WithSummary("Remove an item or part of a stack")
      .WithDescription("quantity removes that many pieces (default: the whole stack); sell=true credits half of the item's cost per piece to the coins.");
    group.MapPut("/coins", SetCoins).WithName("SetCoins").WithSummary("Replace the coins of the character");
    group.MapPost("/coins/change", ChangeCoins).WithName("ChangeCoins")
      .WithSummary("Add (positive) or remove (negative) coins of each kind");
    group.MapPost("/coins/payments", Pay).WithName("PayCoins")
      .WithSummary("Pay an amount in copper pieces; coins are spent and change is returned (422 when not enough)");
    group.MapPut("/items/{itemId:guid}/slot", Equip).WithName("EquipItem").WithSummary("Equip an item (slot None unequips it)");
    group.MapPut("/items/{itemId:guid}/attunement", Attune).WithName("AttuneItem").WithSummary("Attune to an item or end the attunement");
    group.MapPut("/toggles/{featureKey}", Toggle).WithName("ToggleFeature").WithSummary("Switch a toggleable feature on or off");
    group.MapPut("/armor-class", ArmorClass).WithName("ChooseArmorClass").WithSummary("Choose the armor class calculation");
    return group;
  }

  private static async Task<Ok<CharacterStateDto>> AddItem(CharacterService service, Guid id, AddItemDto dto, CancellationToken cancellationToken)
  {
    await service.AddItemAsync(id, dto.ItemKey, dto.Quantity, cancellationToken, dto.Purchase);
    return TypedResults.Ok(StateMapper.State(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));
  }

  private static async Task<Ok<CharacterStateDto>> RemoveItem(CharacterService service, Guid id, Guid itemId, int? quantity, bool? sell, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.RemoveItemAsync(id, itemId, quantity, sell == true, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> SetCoins(CharacterService service, Guid id, CoinsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.SetCoinsAsync(id, dto.Copper, dto.Silver, dto.Electrum, dto.Gold, dto.Platinum, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> ChangeCoins(CharacterService service, Guid id, CoinsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ChangeCoinsAsync(id, dto.Copper, dto.Silver, dto.Electrum, dto.Gold, dto.Platinum, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Pay(CharacterService service, Guid id, CoinPaymentDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.PayAsync(id, dto.AmountCp, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Equip(CharacterService service, Guid id, Guid itemId, EquipDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.EquipAsync(id, itemId, dto.Slot, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Attune(CharacterService service, Guid id, Guid itemId, AttuneDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.AttuneAsync(id, itemId, dto.Attuned, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Toggle(CharacterService service, Guid id, string featureKey, ToggleDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ToggleAsync(id, featureKey, dto.Active, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> ArmorClass(CharacterService service, Guid id, ArmorClassDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ChooseArmorClassAsync(id, dto.Label, cancellationToken)));

  #endregion
}
