using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Preview, gain and undo levels
/// </summary>
public static class LevelEndpoints
{
  #region Methods

  public static RouteGroupBuilder MapLevelEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters/{id:guid}").WithTags("Levels").WithCharacterProblems().RequireUser();

    group.MapGet("/level-up/preview", Preview).WithName("PreviewLevelUp")
      .WithSummary("Choices a level in the class would open (nothing is saved)")
      .WithDescription("When the level grants the subclass and no subclass is given, only requiresSubclass and the subclasses are returned.");
    group.MapPost("/levels", LevelUp).WithName("LevelUp")
      .WithSummary("Gain a level")
      .WithDescription("Saves the level even when its choices are open (unless allowIncomplete is false); open choices are answered with POST /choices and belong to this level, so undoing it removes them too.");
    group.MapPost("/levels/validate", ValidateLevelUp).WithName("ValidateLevelUp").WithSummary("Check a level up without saving it");
    group.MapDelete("/levels/last", Undo).WithName("UndoLevel")
      .WithSummary("Remove the highest level with its selections; selections it replaced become active again");
    return group;
  }

  private static async Task<Ok<LevelUpPreviewDto>> Preview
  (
    ICharacterService service,
    TableTopSheetsContext context,
    Guid id,
    string @class,
    string? subclass,
    CancellationToken cancellationToken
  )
  {
    CharacterClass characterClass = await context.Classes.AsNoTracking().Include(entry => entry.Subclasses).FirstOrDefaultAsync(entry => entry.Key == @class, cancellationToken)
                                    ?? throw new RuleViolationException($"Unknown class '{@class}'");
    CharacterEvaluation current = await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken);
    int classLevel = current.ClassLevel(characterClass.Id) + 1;
    bool requiresSubclass = classLevel == characterClass.SubclassLevel;
    IReadOnlyList<SubclassSummaryDto> subclasses = requiresSubclass ? RuleMapper.Subclasses(characterClass) : [];

    if (requiresSubclass && subclass == null)
    {
      return TypedResults.Ok(new LevelUpPreviewDto(characterClass.Key, classLevel, current.TotalLevel + 1, true, subclasses, [], []));
    }

    string preview = RequestMapper.Preview(characterClass.Key, subclass);
    LevelUpPreviewDto result = await service.PreviewLevelUpAsync(id, characterClass.Key, subclass, evaluation => new LevelUpPreviewDto
    (
      characterClass.Key,
      classLevel,
      evaluation.TotalLevel,
      requiresSubclass,
      subclasses,
      ChoiceMapper.Pending(evaluation, preview),
      evaluation.Selections
        .Where(entry => entry.Choice.ReplacePolicy == ReplacePolicyEnum.OneOnLevelUp && evaluation.OwningClass(entry.Owner)?.Id == characterClass.Id)
        .Select(entry => StateMapper.Selection(evaluation, entry, false))
        .ToList()
    ), cancellationToken);
    return TypedResults.Ok(result);
  }

  private static async Task<Ok<CharacterStateDto>> LevelUp(ICharacterService service, Guid id, LevelUpDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.LevelUpAsync(id, RequestMapper.LevelUp(dto), cancellationToken)));

  private static async Task<Ok<ValidationResultDto>> ValidateLevelUp(ICharacterService service, Guid id, LevelUpDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(await ChoiceEndpoints.Validate(() => service.ValidateLevelUpAsync(id, RequestMapper.LevelUp(dto), evaluation => StateMapper.State(evaluation), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Undo(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.UndoLastLevelAsync(id, cancellationToken)));

  #endregion
}
