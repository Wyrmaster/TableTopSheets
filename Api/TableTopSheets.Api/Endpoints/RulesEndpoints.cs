using Microsoft.AspNetCore.Http.HttpResults;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Api.Querying;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Read-only access to the rules catalog (what can be chosen when building a character)
/// </summary>
public static class RulesEndpoints
{
  #region Sort Fields

  private static readonly SortMap<Species> SpeciesSorts = new SortMap<Species>().Add("name", entry => entry.Name).Add("key", entry => entry.Key);
  private static readonly SortMap<Background> BackgroundSorts = new SortMap<Background>().Add("name", entry => entry.Name).Add("key", entry => entry.Key);
  private static readonly SortMap<CharacterClass> ClassSorts = new SortMap<CharacterClass>().Add("name", entry => entry.Name).Add("key", entry => entry.Key);

  private static readonly SortMap<Feat> FeatSorts = new SortMap<Feat>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .Add("category", entry => ChoiceService.FeatCategoryKey(entry.Category));

  private static readonly SortMap<Spell> SpellSorts = new SortMap<Spell>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .Add("level", entry => entry.Level)
    .Add("school", entry => RuleMapper.SchoolKey(entry));

  private static readonly SortMap<Item> ItemSorts = new SortMap<Item>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .Add("kind", entry => entry.Kind.ToString())
    .Add("cost", entry => entry.CostCp)
    .Add("weight", entry => entry.Weight);

  #endregion

  #region Methods

  public static RouteGroupBuilder MapRulesEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes.MapGroup("/api/rules").WithTags("Rules").WithRuleProblems();

    group.MapGet("/species", ListSpecies).WithName("ListSpecies").WithSummary("List species");
    group.MapGet("/species/{key}", GetSpecies).WithName("GetSpecies").WithSummary("Species with its traits and choices");
    group.MapGet("/backgrounds", ListBackgrounds).WithName("ListBackgrounds").WithSummary("List backgrounds");
    group.MapGet("/backgrounds/{key}", GetBackground).WithName("GetBackground").WithSummary("Background with its features and choices");
    group.MapGet("/classes", ListClasses).WithName("ListClasses").WithSummary("List classes with their subclasses");
    group.MapGet("/classes/{key}", GetClass).WithName("GetClass").WithSummary("Class with its features per level and subclasses");
    group.MapGet("/feats", ListFeats).WithName("ListFeats").WithSummary("List feats");
    group.MapGet("/feats/{key}", GetFeat).WithName("GetFeat").WithSummary("Feat with its benefits and choices");
    group.MapGet("/spells", ListSpells).WithName("ListSpells").WithSummary("List spells");
    group.MapGet("/spells/{key}", GetSpell).WithName("GetSpell").WithSummary("Spell with its full text");
    group.MapGet("/items", ListItems).WithName("ListItems").WithSummary("List items (weapons, armor, tools, gear, magic items)");
    group.MapGet("/items/{key}", GetItem).WithName("GetItem").WithSummary("Item with its properties");
    return group;
  }

  private static PagedResult<SpeciesSummaryDto> ListSpecies(RulesCatalog catalog, [AsParameters] ListQuery query)
    => catalog.Species.Values.ToPaged(query, SpeciesSorts, Search).Select(RuleMapper.Summary);

  private static Results<Ok<RuleDetailDto>, NotFound> GetSpecies(RulesCatalog catalog, string key)
    => catalog.Species.Values.FirstOrDefault(entry => entry.Key == key) is { } species
      ? TypedResults.Ok(RuleMapper.Detail(species, catalog.Features.Values.Where(entry => entry.SpeciesId == species.Id)))
      : TypedResults.NotFound();

  private static PagedResult<BackgroundSummaryDto> ListBackgrounds(RulesCatalog catalog, [AsParameters] ListQuery query)
    => catalog.Backgrounds.Values.ToPaged(query, BackgroundSorts, Search).Select(entry => RuleMapper.Summary(entry, catalog));

  private static Results<Ok<RuleDetailDto>, NotFound> GetBackground(RulesCatalog catalog, string key)
    => catalog.Backgrounds.Values.FirstOrDefault(entry => entry.Key == key) is { } background
      ? TypedResults.Ok(RuleMapper.Detail(background, catalog.Features.Values.Where(entry => entry.BackgroundId == background.Id)))
      : TypedResults.NotFound();

  private static PagedResult<ClassSummaryDto> ListClasses(RulesCatalog catalog, [AsParameters] ListQuery query)
    => catalog.Classes.Values.ToPaged(query, ClassSorts, Search).Select(entry => RuleMapper.Summary(entry, catalog));

  private static Results<Ok<ClassDetailDto>, NotFound> GetClass(RulesCatalog catalog, string key)
    => catalog.ClassesByKey.TryGetValue(key, out CharacterClass? @class)
      ? TypedResults.Ok(RuleMapper.Detail(@class, catalog))
      : TypedResults.NotFound();

  private static PagedResult<FeatSummaryDto> ListFeats(RulesCatalog catalog, [AsParameters] ListQuery query, [AsParameters] FeatFilter filter)
    => catalog.Feats.Values
      .WhereIf(filter.Category != null, entry => string.Equals(ChoiceService.FeatCategoryKey(entry.Category), filter.Category, StringComparison.OrdinalIgnoreCase))
      .WhereIf(filter.Repeatable != null, entry => entry.Repeatable == filter.Repeatable)
      .WhereIf(filter.HasPrerequisite != null, entry => (entry.PrerequisiteExpr != null) == filter.HasPrerequisite)
      .ToPaged(query, FeatSorts, Search)
      .Select(RuleMapper.Summary);

  private static Results<Ok<RuleDetailDto>, NotFound> GetFeat(RulesCatalog catalog, string key)
    => catalog.FeatsByKey.TryGetValue(key, out Feat? feat)
      ? TypedResults.Ok(RuleMapper.Detail(feat, catalog.Features.Values.Where(entry => entry.FeatId == feat.Id)))
      : TypedResults.NotFound();

  private static PagedResult<SpellSummaryDto> ListSpells(RulesCatalog catalog, [AsParameters] ListQuery query, [AsParameters] SpellFilter filter)
    => catalog.Spells.Values
      .WhereIf(filter.Class != null, entry => entry.Lists.Contains(filter.Class!, StringComparer.OrdinalIgnoreCase))
      .WhereIf(filter.Level != null, entry => entry.Level == filter.Level)
      .WhereIf(filter.MinLevel != null, entry => entry.Level >= filter.MinLevel)
      .WhereIf(filter.MaxLevel != null, entry => entry.Level <= filter.MaxLevel)
      .WhereIf(filter.School != null, entry => string.Equals(RuleMapper.SchoolKey(entry), filter.School, StringComparison.OrdinalIgnoreCase))
      .WhereIf(filter.Ritual != null, entry => entry.Ritual == filter.Ritual)
      .WhereIf(filter.Concentration != null, entry => entry.Concentration == filter.Concentration)
      .WhereIf(filter.Damage != null, entry => (entry.Damage != null) == filter.Damage)
      .WhereIf(filter.DamageType != null, entry => entry.DamageType == filter.DamageType)
      .WhereIf(filter.CastingTime != null, entry => entry.CastingTime.Contains(filter.CastingTime!, StringComparison.OrdinalIgnoreCase))
      .ToPaged(query, SpellSorts, Search)
      .Select(RuleMapper.Summary);

  private static Results<Ok<SpellDetailDto>, NotFound> GetSpell(RulesCatalog catalog, string key)
    => catalog.SpellsByKey.TryGetValue(key, out Spell? spell) ? TypedResults.Ok(RuleMapper.Detail(spell)) : TypedResults.NotFound();

  private static PagedResult<ItemSummaryDto> ListItems(RulesCatalog catalog, [AsParameters] ListQuery query, [AsParameters] ItemFilter filter)
    => catalog.Items.Values
      .WhereIf(filter.Kind != null, entry => entry.Kind == filter.Kind)
      .WhereIf(filter.Category != null, entry => string.Equals(RuleMapper.ItemCategory(entry), filter.Category, StringComparison.OrdinalIgnoreCase))
      .WhereIf(filter.Magic != null, entry => entry.Magic == filter.Magic)
      .WhereIf(filter.Attunement != null, entry => entry.RequiresAttunement == filter.Attunement)
      .ToPaged(query, ItemSorts, Search)
      .Select(RuleMapper.Summary);

  private static Results<Ok<ItemDetailDto>, NotFound> GetItem(RulesCatalog catalog, string key)
    => catalog.ItemsByKey.TryGetValue(key, out Item? item) ? TypedResults.Ok(RuleMapper.Detail(item, catalog)) : TypedResults.NotFound();

  private static IEnumerable<string?> Search(RuleEntity entity) => [entity.Name, entity.Key];

  #endregion
}
