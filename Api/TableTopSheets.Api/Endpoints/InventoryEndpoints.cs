using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Items, equipment slots, attunement, toggles and the armor class calculation
/// </summary>
public static class InventoryEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapInventoryEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters/{id:guid}").WithTags("Inventory").WithCharacterProblems();

    group.MapPost("/items", AddItem).WithName("AddItem").WithSummary("Add an item to the inventory");
    group.MapPut("/items/{itemId:guid}/slot", Equip).WithName("EquipItem").WithSummary("Equip an item (slot None unequips it)");
    group.MapPut("/items/{itemId:guid}/attunement", Attune).WithName("AttuneItem").WithSummary("Attune to an item or end the attunement");
    group.MapPut("/toggles/{featureKey}", Toggle).WithName("ToggleFeature").WithSummary("Switch a toggleable feature on or off");
    group.MapPut("/armor-class", ArmorClass).WithName("ChooseArmorClass").WithSummary("Choose the armor class calculation");
    return group;
  }

  private static async Task<Ok<CharacterStateDto>> AddItem(CharacterService service, Guid id, AddItemDto dto, CancellationToken cancellationToken)
  {
    await service.AddItemAsync(id, dto.ItemKey, dto.Quantity, cancellationToken);
    return TypedResults.Ok(StateMapper.State(service.Evaluate(await service.LoadAsync(id, cancellationToken))));
  }

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
