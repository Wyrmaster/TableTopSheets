using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Api.Querying;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Engine.Services;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   The decision loop: list what is open, list the options, send selections, take selections back
/// </summary>
public static class ChoiceEndpoints
{
  #region Sort Fields

  private static readonly SortMap<OptionAvailability> OptionSorts = new SortMap<OptionAvailability>()
    .Add("name", entry => entry.Option.Name)
    .Add("key", entry => entry.Option.Key)
    .Add("level", entry => entry.Option.Level)
    .Add("school", entry => entry.Option.School)
    .Add("category", entry => entry.Option.Category)
    .Add("available", entry => entry.Available ? 0 : 1);

  #endregion

  #region Methods

  public static RouteGroupBuilder MapChoiceEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/characters/{id:guid}").WithTags("Choices").WithCharacterProblems().RequireUser();

    group.MapGet("/choices", Pending).WithName("GetPendingChoices").WithSummary("Choices that still need selections");
    group.MapPost("/choices", Select).WithName("SelectOptions")
      .WithSummary("Answer open choices")
      .WithDescription("Selections are applied in order and each one is validated against the character including the previous ones, so a feat and the choices it opens can be sent together (use ref / parentRef). All or nothing.");
    group.MapPost("/choices/validate", ValidateSelections).WithName("ValidateSelections").WithSummary("Check selections without saving them");
    group.MapGet("/choices/{choiceKey}/options", Options).WithName("GetChoiceOptions").WithSummary("Paged, filterable options of a choice instance");
    group.MapGet("/selections", Selections).WithName("GetSelections").WithSummary("Active selections");
    group.MapDelete("/selections/{selectionId:guid}", Deselect).WithName("RemoveSelection")
      .WithSummary("Take back a selection of the latest level (or of the origin before the first level) with the selections that depend on it");
    group.MapPost("/selections/{selectionId:guid}/rest-replace", RestReplace).WithName("ReplaceSelectionOnRest")
      .WithSummary("Change a selection whose choice allows changes on a rest");
    return group;
  }

  private static async Task<Ok<IReadOnlyList<PendingChoiceDto>>> Pending(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(ChoiceMapper.Pending(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Select(ICharacterService service, Guid id, SelectionsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.SelectAsync(id, RequestMapper.Selections(dto.Selections), cancellationToken)));

  private static async Task<Ok<ValidationResultDto>> ValidateSelections(ICharacterService service, Guid id, SelectionsDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(await Validate(() => service.ValidateSelectionsAsync(id, RequestMapper.Selections(dto.Selections), evaluation => StateMapper.State(evaluation), cancellationToken)));

  /// <summary>
  ///   Run a dry run and report rule violations as a result instead of an error
  /// </summary>
  public static async Task<ValidationResultDto> Validate(Func<Task<CharacterStateDto>> dryRun)
  {
    try
    {
      return new ValidationResultDto(true, [], await dryRun());
    }
    catch (RuleViolationException exception) when (exception is not CharacterNotFoundException)
    {
      return new ValidationResultDto(false, exception.Errors, null);
    }
  }

  private static async Task<Ok<PagedResult<OptionDto>>> Options
  (
    ICharacterService service,
    IRulesCatalogLoader rules,
    TableTopSheetsContext context,
    Guid id,
    string choiceKey,
    [AsParameters] OptionQuery filter,
    [AsParameters] ListQuery query,
    CancellationToken cancellationToken
  )
  {
    if (!await context.Choices.AnyAsync(entry => entry.Key == choiceKey, cancellationToken))
    {
      throw new RuleViolationException($"Unknown choice '{choiceKey}'");
    }

    Guid? parent = null;
    if (filter.ParentRef != null)
    {
      parent = Guid.TryParse(filter.ParentRef, out Guid value) ? value : throw new QueryException("parentRef", "parentRef has to be the id of a selection");
    }

    ChoiceOptions options;
    if (filter.Preview != null)
    {
      (string classKey, string? subclassKey) = RequestMapper.Preview(filter.Preview);
      options = await service.PreviewLevelUpAsync(id, classKey, subclassKey, evaluation => Evaluate(evaluation, choiceKey, parent), cancellationToken);
    }
    else
    {
      options = Evaluate(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken), choiceKey, parent);
    }

    PagedResult<OptionAvailability> page = options.Options
      .Where(entry => entry.Reason != ChoiceService.NotPartOfChoice)
      .WhereIf(filter.IncludeDisabled != true, entry => entry.Available)
      .WhereIf(filter.Level != null, entry => entry.Option.Level == filter.Level)
      .WhereIf(filter.MinLevel != null, entry => entry.Option.Level >= filter.MinLevel)
      .WhereIf(filter.MaxLevel != null, entry => entry.Option.Level <= filter.MaxLevel)
      .WhereIf(filter.School != null, entry => string.Equals(entry.Option.School, filter.School, StringComparison.OrdinalIgnoreCase))
      .WhereIf(filter.Ritual != null, entry => entry.Option.Ritual == filter.Ritual)
      .WhereIf(filter.Damage != null, entry => entry.Option.Damage == filter.Damage)
      .WhereIf(filter.Category != null, entry => string.Equals(entry.Option.Category, filter.Category, StringComparison.OrdinalIgnoreCase))
      .ToPaged(query, OptionSorts, entry => [entry.Option.Name, entry.Option.Key], defaultSort: null);

    // descriptions are only read for the options of the page
    await rules.DescribeAsync(options.Catalog, page.Items.Select(entry => (options.Choice, entry.Option.Key)), cancellationToken);
    return TypedResults.Ok(page.Select(entry => ChoiceMapper.Option(options.Catalog, options.Choice, entry.Option, entry.Reason, filter.IncludeDescriptions == true)));
  }

  private static ChoiceOptions Evaluate(CharacterEvaluation evaluation, string choiceKey, Guid? parent)
  {
    string scope = parent == null ? string.Empty : $" for selection {parent}";
    Choice choice = evaluation.Catalog.ChoicesByKey.GetValueOrDefault(choiceKey)
                    ?? throw new RuleViolationException($"'{choiceKey}' is not an available choice{scope}");
    FeatureInstance owner = ChoiceService.FindOwner(evaluation, choice, parent)
                            ?? throw new RuleViolationException($"'{choice.Key}' is not an available choice{scope}");
    return new ChoiceOptions(evaluation.Catalog, choice, ChoiceService.EvaluateOptions(evaluation, choice, owner).ToList());
  }

  private static async Task<Ok<IReadOnlyList<SelectionInfoDto>>> Selections(ICharacterService service, Guid id, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.Selections(await service.EvaluateAsync(await service.LoadAsync(id, cancellationToken), cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> Deselect(ICharacterService service, Guid id, Guid selectionId, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.DeselectAsync(id, selectionId, cancellationToken)));

  private static async Task<Ok<CharacterStateDto>> RestReplace(ICharacterService service, Guid id, Guid selectionId, RestReplaceDto dto, CancellationToken cancellationToken)
    => TypedResults.Ok(StateMapper.State(await service.ReplaceOnRestAsync(id, selectionId, dto.OptionKey, cancellationToken)));

  #endregion

  #region Nested Types

  /// <summary>
  ///   Options of a choice instance with the rules they were evaluated against
  /// </summary>
  private sealed record ChoiceOptions(RulesCatalog Catalog, Choice Choice, List<OptionAvailability> Options);

  #endregion
}
