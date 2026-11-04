using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using TableTopSheets.Api.Authentication;
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

  #region Constants

  /// <summary>
  ///   The url of a portrait changes with its content, so browsers may keep it; private because it is a user's picture
  /// </summary>
  private const string PortraitCacheControl = "private, max-age=86400";

  #endregion

  #region Methods

  public static RouteGroupBuilder MapCharacterEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api/characters")
      .WithTags("Characters")
      .WithCharacterProblems()
      .RequireUser();

    group
      .MapGet("/", List)
      .WithName("ListCharacters")
      .WithSummary("List the characters of the signed in user");
    group
      .MapPost("/", Create)
      .WithName("CreateCharacter")
      .WithSummary("Create a character from its origin (species, background, ability scores)")
      .WithDescription("Saves the character even when origin choices are open (unless allowIncomplete is false). The answer lists the pending choices.");
    group
      .MapGet("/{id:guid}", Get)
      .WithName("GetCharacter")
      .WithSummary("Current state with pending choices");
    group
      .MapPatch("/{id:guid}", Update)
      .WithName("UpdateCharacter")
      .WithSummary("Change name, alignment, appearance, backstory or experience");
    group
      .MapDelete("/{id:guid}", Delete)
      .WithName("DeleteCharacter")
      .WithSummary("Delete a character");
    group
      .MapGet("/{id:guid}/sheet", Sheet)
      .WithName("GetCharacterSheet")
      .WithSummary("Every computed value of the character sheet");
    group
      .MapGet("/{id:guid}/sheet.pdf", Pdf)
      .WithName("GetCharacterSheetPdf")
      .WithSummary("Printable character sheet; ?portrait=true puts the portrait on the first page")
      .Produces(StatusCodes.Status200OK, contentType: "application/pdf");
    group
      .MapPut("/{id:guid}/portrait", SetPortrait)
      .WithName("SetCharacterPortrait")
      .WithSummary("Upload the picture of a character (base64 JPEG, PNG or WebP, at most 2 MB)")
      .WithDescription("Replaces the previous picture. 422 when the data is no such image or too large.");
    group
      .MapDelete("/{id:guid}/portrait", RemovePortrait)
      .WithName("RemoveCharacterPortrait")
      .WithSummary("Remove the picture of a character");

    // an img element can't send the bearer token: the picture is anonymous, the character id is the secret
    routes
      .MapGroup("/api/characters")
      .WithTags("Characters")
      .MapGet("/{id:guid}/portrait", GetPortrait)
      .WithName("GetCharacterPortrait")
      .WithSummary("Picture of a character")
      .WithDescription("Anonymous like the rule images. Answers 304 when If-None-Match carries the current ETag.")
      .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
      .Produces(StatusCodes.Status304NotModified)
      .ProducesProblem(StatusCodes.Status404NotFound);
    return group;
  }

  private static async Task<PagedResult<CharacterSummaryDto>> List
  (
    TableTopSheetsContext context,
    CurrentUser currentUser,
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
      .Where(entry => entry.OwnerId == currentUser.UserId)
      .WhereIf(species != null, entry => entry.Species != null && entry.Species.Key.ToLower() == species)
      .ToListAsync(cancellationToken);
    Dictionary<Guid, CharacterClass> classes = await context.Classes
      .AsNoTracking()
      .Include(entry => entry.Subclasses)
      .ToDictionaryAsync(entry => entry.Id, cancellationToken);
    Dictionary<Guid, string> speciesNames = await context.Species.ToDictionaryAsync(entry => entry.Id, entry => entry.Name, cancellationToken);
    Dictionary<Guid, string> backgroundNames = await context.Backgrounds.ToDictionaryAsync(entry => entry.Id, entry => entry.Name, cancellationToken);

    return characters
      .Select(entry => StateMapper.Summary(entry, classes, speciesNames, backgroundNames))
      .WhereIf(filter.Class != null, entry => entry.Classes.Any(@class => string.Equals(@class.Key, filter.Class, StringComparison.OrdinalIgnoreCase)))
      .WhereIf(filter.MinLevel != null, entry => entry.Level >= filter.MinLevel)
      .WhereIf(filter.MaxLevel != null, entry => entry.Level <= filter.MaxLevel)
      .ToPaged(query, Sorts, entry => [entry.Name]);
  }

  private static async Task<Created<CharacterStateDto>> Create(ICharacterService service, CreateCharacterDto dto, CancellationToken cancellationToken)
  {
    Character character = await service.CreateAsync(RequestMapper.Create(dto), cancellationToken);
    return TypedResults.Created($"/api/characters/{character.Id}", StateMapper.State(await service.EvaluateAsync(character, cancellationToken)));
  }

  private static async Task<Ok<CharacterStateDto>> Get(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Update(ICharacterService service, Guid id, UpdateCharacterDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.UpdateDetailsAsync(id, RequestMapper.Details(dto), cancellationToken)));

  private static async Task<NoContent> Delete(ICharacterService service, Guid id, CancellationToken cancellationToken)
  {
    await service.DeleteAsync(id, cancellationToken);
    return TypedResults.NoContent();
  }

  private static async Task<Ok<CharacterSheet>> Sheet(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(await service.BuildSheetAsync(id, cancellationToken));

  private static async Task<FileContentHttpResult> Pdf(ICharacterService service, Guid id, CancellationToken cancellationToken, bool portrait = false)
  {
    CharacterSheet sheet = await service.BuildSheetAsync(id, cancellationToken);
    byte[]? picture = portrait ? await service.LoadPortraitAsync(id, cancellationToken) : null;
    using MemoryStream stream = new();
    await using (Stream pdf = new CharacterSheetDocument(sheet, picture).GenerateCharacterSheetStream())
    {
      await pdf.CopyToAsync(stream, cancellationToken);
    }

    return TypedResults.File(stream.ToArray(), "application/pdf", $"{FileName(sheet.Name)}.pdf");
  }

  private static async Task<Ok<CharacterStateDto>> SetPortrait(ICharacterService service, Guid id, SetPortraitDto dto, CancellationToken cancellationToken)
  {
    byte[] data;
    try
    {
      data = Convert.FromBase64String(dto.Data);
    }
    catch (FormatException)
    {
      throw new RuleViolationException("The portrait data isn't valid base64");
    }

    return TypedResults.Ok(StateMapper.State(await service.SetPortraitAsync(id, data, cancellationToken)));
  }

  private static async Task<Ok<CharacterStateDto>> RemovePortrait(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.RemovePortraitAsync(id, cancellationToken)));

  private static async Task<Results<FileContentHttpResult, StatusCodeHttpResult, NotFound>> GetPortrait
  (
    HttpContext httpContext,
    TableTopSheetsContext context,
    Guid id,
    CancellationToken cancellationToken
  )
  {
    string? hash = await context.Characters
      .AsNoTracking()
      .Where(entry => entry.Id == id)
      .Select(entry => entry.PortraitHash)
      .FirstOrDefaultAsync(cancellationToken);
    if (hash == null)
    {
      return TypedResults.NotFound();
    }

    EntityTagHeaderValue tag = new($"\"{hash}\"");
    httpContext.Response.Headers.CacheControl = PortraitCacheControl;
    if (httpContext.Request
      .GetTypedHeaders()
      .IfNoneMatch
      .Any(entry => entry.Compare(tag, false) || entry.Equals(EntityTagHeaderValue.Any)))
    {
      httpContext.Response.Headers.ETag = tag.ToString();
      return TypedResults.StatusCode(StatusCodes.Status304NotModified);
    }

    var portrait = await context.CharacterPortraits
      .AsNoTracking()
      .Where(entry => entry.CharacterId == id)
      .Select(entry => new
      {
        entry.ContentType,
        entry.Data,
      })
      .FirstOrDefaultAsync(cancellationToken);
    return portrait == null
      ? TypedResults.NotFound()
      : TypedResults.File(portrait.Data, portrait.ContentType, entityTag: tag);
  }

  private static string FileName(string name)
  {
    string cleaned = new(name
      .Select(entry => Path
        .GetInvalidFileNameChars()
        .Contains(entry) || char.IsWhiteSpace(entry) ? '-' : entry)
      .ToArray());
    return cleaned.Length == 0 ? "character" : cleaned;
  }

  #endregion
}
