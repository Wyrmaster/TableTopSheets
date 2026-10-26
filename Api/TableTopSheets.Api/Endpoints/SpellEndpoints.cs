using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Prepared spell loadouts and the spellbook
/// </summary>
public static class SpellEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapSpellEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters/{id:guid}").WithTags("Spells").WithCharacterProblems().RequireUser();

    group.MapPut("/prepared-spells/{classKey}", Prepare).WithName("SetPreparedSpells").WithSummary("Replace the prepared spells of a class (long rest)");
    group.MapPost("/spellbook", CopyToSpellbook).WithName("CopyToSpellbook").WithSummary("Copy a spell into the spellbook");
    return group;
  }

  private static async Task<Ok<CharacterStateDto>> Prepare(CharacterService service, Guid id, string classKey, PreparedSpellsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.SetPreparedSpellsAsync(id, classKey, dto.SpellKeys, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> CopyToSpellbook(CharacterService service, Guid id, SpellbookEntryDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.CopyToSpellbookAsync(id, dto.SpellKey, dto.Note, cancellationToken)));

  #endregion
}
