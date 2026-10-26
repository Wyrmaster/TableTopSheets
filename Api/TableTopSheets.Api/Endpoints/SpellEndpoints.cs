using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Known spells, prepared spell loadouts and the spellbook
/// </summary>
public static class SpellEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapSpellEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api/characters/{id:guid}")
      .WithTags("Spells")
      .WithCharacterProblems()
      .RequireUser();

    group
      .MapGet("/spells", Spells)
      .WithName("GetSpells")
      .WithSummary("Spells per spellcasting class: spellbook, prepared and always prepared spells with the limits")
      .WithDescription("A class with prepareChange LongRest changes its prepared spells with PUT prepared-spells/{classKey}: from the spellbook (pool Spellbook) or from its spell list (pool ClassList, see GET /api/rules/spells?class={spellList}&maxLevel={maxSpellLevel}).");
    group
      .MapPut("/prepared-spells/{classKey}", Prepare)
      .WithName("SetPreparedSpells")
      .WithSummary("Replace the prepared spells of a class (long rest)");
    group
      .MapPost("/spellbook", CopyToSpellbook)
      .WithName("CopyToSpellbook")
      .WithSummary("Copy a spell into the spellbook");
    return group;
  }

  private static async Task<Ok<CharacterSpellsDto>> Spells(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(SpellMapper.Spells(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Prepare(ICharacterService service, Guid id, string classKey, PreparedSpellsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.SetPreparedSpellsAsync(id, classKey, dto.SpellKeys, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> CopyToSpellbook(ICharacterService service, Guid id, SpellbookEntryDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.CopyToSpellbookAsync(id, dto.SpellKey, dto.Note, cancellationToken)));

  #endregion
}
