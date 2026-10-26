using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Api.Querying;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Services;
using TableTopSheets.Pdf;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Create, list, change and delete characters and print their sheet
/// </summary>
public static class CharacterEndpoints
{
  #region Sort Fields

  private static readonly SortMap<CharacterSummaryDto> Sorts = new SortMap<CharacterSummaryDto>()
    .Add("name", entry => entry.Name)
    .Add("level", entry => entry.Level)
    .Add("species", entry => entry.Species);

  #endregion

  #region Methods

  public static RouteGroupBuilder MapCharacterEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters").WithTags("Characters").WithCharacterProblems();

    group.MapGet("/", List).WithName("ListCharacters").WithSummary("List characters");
    group.MapPost("/", Create).WithName("CreateCharacter")
      .WithSummary("Create a character from its origin (species, background, ability scores)")
      .WithDescription("Saves the character even when origin choices are open (unless allowIncomplete is false). The answer lists the pending choices.");
    group.MapGet("/{id:guid}", Get).WithName("GetCharacter").WithSummary("Current state with pending choices");
    group.MapPatch("/{id:guid}", Update).WithName("UpdateCharacter").WithSummary("Change name, alignment, appearance, backstory or experience");
    group.MapDelete("/{id:guid}", Delete).WithName("DeleteCharacter").WithSummary("Delete a character");
    group.MapGet("/{id:guid}/sheet", Sheet).WithName("GetCharacterSheet").WithSummary("Every computed value of the character sheet");
    group.MapGet("/{id:guid}/sheet.pdf", Pdf).WithName("GetCharacterSheetPdf").WithSummary("Printable character sheet")
      .Produces(StatusCodes.Status200OK, contentType: "application/pdf");
    return group;
  }

  private static async Task<PagedResult<CharacterSummaryDto>> List
  (
    TableTopSheetsContext context,
    [AsParameters] ListQuery query,
    [AsParameters] CharacterFilter filter,
    CancellationToken cancellationToken
  )
  {
    // the summary only needs names, so the characters are not evaluated
    string? species = filter.Species?.ToLowerInvariant();
    List<Character> characters = await context.Characters
      .AsNoTracking()
      .Include(entry => entry.Levels)
      .WhereIf(species != null, entry => entry.Species != null && entry.Species.Key.ToLower() == species)
      .ToListAsync(cancellationToken);
    Dictionary<Guid, CharacterClass> classes = await context.Classes.AsNoTracking().Include(entry => entry.Subclasses).ToDictionaryAsync(entry => entry.Id, cancellationToken);
    Dictionary<Guid, string> speciesNames = await context.Species.ToDictionaryAsync(entry => entry.Id, entry => entry.Name, cancellationToken);
    Dictionary<Guid, string> backgroundNames = await context.Backgrounds.ToDictionaryAsync(entry => entry.Id, entry => entry.Name, cancellationToken);

    return characters
      .Select(entry => StateMapper.Summary(entry, classes, speciesNames, backgroundNames))
      .WhereIf(filter.Class != null, entry => entry.Classes.Any(@class => string.Equals(@class.Key, filter.Class, StringComparison.OrdinalIgnoreCase)))
      .WhereIf(filter.MinLevel != null, entry => entry.Level >= filter.MinLevel)
      .WhereIf(filter.MaxLevel != null, entry => entry.Level <= filter.MaxLevel)
      .ToPaged(query, Sorts, entry => [entry.Name]);
  }

  private static async Task<Created<CharacterStateDto>> Create(CharacterService service, CreateCharacterDto dto, CancellationToken cancellationToken)
  {
    Character character = await service.CreateAsync(RequestMapper.Create(dto), cancellationToken);
    return TypedResults.Created($"/api/characters/{character.Id}", StateMapper.State(await service.EvaluateAsync(character, cancellationToken)));
  }

  private static async Task<Ok<CharacterStateDto>> Get(CharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Update(CharacterService service, Guid id, UpdateCharacterDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.UpdateDetailsAsync(id, RequestMapper.Details(dto), cancellationToken)));

  private static async Task<NoContent> Delete(CharacterService service, Guid id, CancellationToken cancellationToken)
  {
    await service.DeleteAsync(id, cancellationToken);
    return TypedResults.NoContent();
  }

  private static async Task<Ok<CharacterSheet>> Sheet(CharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(await service.BuildSheetAsync(id, cancellationToken));

  private static async Task<FileContentHttpResult> Pdf(CharacterService service, Guid id, CancellationToken cancellationToken)
  {
    CharacterSheet sheet = await service.BuildSheetAsync(id, cancellationToken);
    using MemoryStream stream = new();
    await using (Stream pdf = new CharacterSheetDocument(sheet).GenerateCharacterSheetStream())
    {
      await pdf.CopyToAsync(stream, cancellationToken);
    }

    return TypedResults.File(stream.ToArray(), "application/pdf", $"{FileName(sheet.Name)}.pdf");
  }

  private static string FileName(string name)
  {
    string cleaned = new(name.Select(entry => Path.GetInvalidFileNameChars().Contains(entry) || char.IsWhiteSpace(entry) ? '-' : entry).ToArray());
    return cleaned.Length == 0 ? "character" : cleaned;
  }

  #endregion
}
