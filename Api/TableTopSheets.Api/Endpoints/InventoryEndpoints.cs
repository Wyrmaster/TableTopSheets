using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Starting equipment, items (add, buy, remove, sell), coins, equipment slots, attunement, toggles and the armor class calculation
/// </summary>
public static class InventoryEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api/characters/{id:guid}")
      .WithTags("Inventory")
      .WithCharacterProblems()
      .RequireUser();

    group
      .MapGet("/starting-equipment", GetStartingEquipment)
      .WithName("GetStartingEquipment")
      .WithSummary("Starting equipment of the background and the first class, and which alternative was taken");
    group
      .MapPost("/starting-equipment/{source}", TakeStartingEquipment)
      .WithName("TakeStartingEquipment")
      .WithSummary("Take an alternative of the starting equipment (source Background or Class)")
      .WithDescription("Adds the items and coins of the alternative once per source. Entries with options need a pick keyed by the entry key; "
                       + "a pick of a \"same as above\" entry also answers that tool choice while it is open. Errors answer 422.");
    group
      .MapPost("/items", AddItem)
      .WithName("AddItem")
      .WithSummary("Add an item to the inventory")
      .WithDescription("With purchase true the item's cost is paid from the coins; change is returned. Items sold in bundles (20 arrows) cost per "
                       + "started bundle, the quantity counts pieces. Not enough coins answer 422.");
    group
      .MapDelete("/items/{itemId:guid}", RemoveItem)
      .WithName("RemoveItem")
      .WithSummary("Remove an item or part of a stack")
      .WithDescription("quantity removes that many pieces (default: the whole stack); sell=true credits half of the item's cost per piece to the coins.");
    group
      .MapPost("/items/{itemId:guid}/stack", AddToStack)
      .WithName("AddToStack")
      .WithSummary("Add pieces to an item stack without paying (ammunition recovered after a fight)");
    group
      .MapPut("/coins", SetCoins)
      .WithName("SetCoins")
      .WithSummary("Replace the coins of the character");
    group
      .MapPost("/coins/change", ChangeCoins)
      .WithName("ChangeCoins")
      .WithSummary("Add (positive) or remove (negative) coins of each kind");
    group
      .MapPost("/coins/payments", Pay)
      .WithName("PayCoins")
      .WithSummary("Pay an amount in copper pieces; coins are spent and change is returned (422 when not enough)");
    group
      .MapPut("/items/{itemId:guid}/slot", Equip)
      .WithName("EquipItem")
      .WithSummary("Equip an item (slot None unequips it)");
    group
      .MapPut("/items/{itemId:guid}/attunement", Attune)
      .WithName("AttuneItem")
      .WithSummary("Attune to an item or end the attunement");
    group
      .MapPut("/toggles/{featureKey}", Toggle)
      .WithName("ToggleFeature")
      .WithSummary("Switch a toggleable feature on or off");
    group
      .MapPut("/armor-class", ArmorClass)
      .WithName("ChooseArmorClass")
      .WithSummary("Choose the armor class calculation");
    return group;
  }

  private static async Task<Ok<IReadOnlyList<StartingEquipmentDto>>> GetStartingEquipment
  (
    ICharacterService service,
    TableTopSheetsContext context,
    Guid id,
    CancellationToken cancellationToken
  )
  {
    List<StartingEquipmentDto> result = new();
    foreach (StartingEquipmentOffer offer in await service.GetStartingEquipmentAsync(id, cancellationToken))
    {
      IReadOnlyDictionary<string, string> names = await RulesEndpoints.ItemNamesAsync(context, offer.Options, cancellationToken);
      result.Add(new StartingEquipmentDto
      (
        offer.Source,
        offer.OwnerKey,
        offer.OwnerName,
        offer.Taken,
        RuleMapper.StartingEquipment(offer.Options, names, offer.SameAs)
      ));
    }

    return TypedResults.Ok<IReadOnlyList<StartingEquipmentDto>>(result);
  }

  /// <param name="source"><c>background</c> or <c>class</c> (any case)</param>
  private static async Task<Results<Ok<CharacterStateDto>, NotFound>> TakeStartingEquipment
  (
    ICharacterService service,
    Guid id,
    string source,
    TakeStartingEquipmentDto dto,
    CancellationToken cancellationToken
  )
  {
    if (!Enum.TryParse(source, true, out StartingEquipmentSourceEnum parsed) || !Enum.IsDefined(parsed))
    {
      return TypedResults.NotFound();
    }

    CharacterEvaluation evaluation = await service.TakeStartingEquipmentAsync(id, parsed, RequestMapper.StartingEquipment(dto)!, cancellationToken);
    return TypedResults.Ok(StateMapper.State(evaluation));
  }

  private static async Task<Ok<CharacterStateDto>> AddItem(ICharacterService service, Guid id, AddItemDto dto, CancellationToken cancellationToken)
  {
    await service.AddItemAsync(id, dto.ItemKey, dto.Quantity, cancellationToken, dto.Purchase);
    return TypedResults.Ok(StateMapper.State(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));
  }

  private static async Task<Ok<CharacterStateDto>> RemoveItem(ICharacterService service, Guid id, Guid itemId, int? quantity, bool? sell, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.RemoveItemAsync(id, itemId, quantity, sell == true, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> AddToStack(ICharacterService service, Guid id, Guid itemId, AddToStackDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.AddToStackAsync(id, itemId, dto.Quantity, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> SetCoins(ICharacterService service, Guid id, CoinsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.SetCoinsAsync(id, dto.Copper, dto.Silver, dto.Electrum, dto.Gold, dto.Platinum, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> ChangeCoins(ICharacterService service, Guid id, CoinsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ChangeCoinsAsync(id, dto.Copper, dto.Silver, dto.Electrum, dto.Gold, dto.Platinum, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Pay(ICharacterService service, Guid id, CoinPaymentDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.PayAsync(id, dto.AmountCp, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Equip(ICharacterService service, Guid id, Guid itemId, EquipDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.EquipAsync(id, itemId, dto.Slot, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Attune(ICharacterService service, Guid id, Guid itemId, AttuneDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.AttuneAsync(id, itemId, dto.Attuned, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Toggle(ICharacterService service, Guid id, string featureKey, ToggleDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ToggleAsync(id, featureKey, dto.Active, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> ArmorClass(ICharacterService service, Guid id, ArmorClassDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ChooseArmorClassAsync(id, dto.Label, cancellationToken)));

  #endregion
}
