using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Api.Mapping;
using TableTopSheets.Api.Querying;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Endpoints;

/// <summary>
///   Read-only access to the rule tables (what can be chosen when building a character); filtered, sorted and paged by the database
/// </summary>
public static class RulesEndpoints
{
  #region Sort Fields

  private static readonly QuerySortMap<Species> SpeciesSorts = new QuerySortMap<Species>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key);
  private static readonly QuerySortMap<Background> BackgroundSorts = new QuerySortMap<Background>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key);
  private static readonly QuerySortMap<CharacterClass> ClassSorts = new QuerySortMap<CharacterClass>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key);

  private static readonly QuerySortMap<Feat> FeatSorts = new QuerySortMap<Feat>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .AddKey("category", entry => entry.Category, ChoiceService.FeatCategoryKey);

  private static readonly QuerySortMap<Spell> SpellSorts = new QuerySortMap<Spell>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .Add("level", entry => entry.Level)
    .AddKey("school", entry => entry.School, SchoolKey);

  private static readonly QuerySortMap<Item> ItemSorts = new QuerySortMap<Item>()
    .Add("name", entry => entry.Name)
    .Add("key", entry => entry.Key)
    .AddKey("kind", entry => entry.Kind, kind => kind.ToString())
    .Add("cost", entry => entry.CostCp)
    .Add("weight", entry => entry.Weight);

  #endregion

  #region Methods

  public static RouteGroupBuilder MapRulesEndpoints(this IEndpointRouteBuilder routes)
  {
    RouteGroupBuilder group = routes
      .MapGroup("/api/rules")
      .WithTags("Rules")
      .WithRuleProblems();

    group
      .MapGet("/species", ListSpecies)
      .WithName("ListSpecies")
      .WithSummary("List species");
    group
      .MapGet("/species/{key}", GetSpecies)
      .WithName("GetSpecies")
      .WithSummary("Species with its traits and choices");
    group
      .MapGet("/backgrounds", ListBackgrounds)
      .WithName("ListBackgrounds")
      .WithSummary("List backgrounds");
    group
      .MapGet("/backgrounds/{key}", GetBackground)
      .WithName("GetBackground")
      .WithSummary("Background with its features and choices");
    group
      .MapGet("/classes", ListClasses)
      .WithName("ListClasses")
      .WithSummary("List classes with their subclasses");
    group
      .MapGet("/classes/{key}", GetClass)
      .WithName("GetClass")
      .WithSummary("Class with its features per level and subclasses");
    group
      .MapGet("/feats", ListFeats)
      .WithName("ListFeats")
      .WithSummary("List feats");
    group
      .MapGet("/feats/{key}", GetFeat)
      .WithName("GetFeat")
      .WithSummary("Feat with its benefits and choices");
    group
      .MapGet("/spells", ListSpells)
      .WithName("ListSpells")
      .WithSummary("List spells");
    group
      .MapGet("/spells/{key}", GetSpell)
      .WithName("GetSpell")
      .WithSummary("Spell with its full text");
    group
      .MapGet("/items", ListItems)
      .WithName("ListItems")
      .WithSummary("List items (weapons, armor, tools, gear, magic items)");
    group
      .MapGet("/items/{key}", GetItem)
      .WithName("GetItem")
      .WithSummary("Item with its properties");
    return group;
  }

  private static Task<PagedResult<SpeciesSummaryDto>> ListSpecies(TableTopSheetsContext context, [AsParameters] ListQuery query, CancellationToken cancellationToken)
    => context.Species
      .ToPagedAsync(query, SpeciesSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);

  private static async Task<Results<Ok<RuleDetailDto>, NotFound>> GetSpecies(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
    => await context.Species
      .AsNoTracking()
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken) is { } species
      ? TypedResults.Ok(RuleMapper.Detail(species, await Features(context)
        .Where(entry => entry.SpeciesId == species.Id)
        .ToListAsync(cancellationToken)))
      : TypedResults.NotFound();

  private static Task<PagedResult<BackgroundSummaryDto>> ListBackgrounds(TableTopSheetsContext context, [AsParameters] ListQuery query, CancellationToken cancellationToken)
    => context.Backgrounds
      .Include(entry => entry.OriginFeat)
      .ToPagedAsync(query, BackgroundSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);

  private static async Task<Results<Ok<RuleDetailDto>, NotFound>> GetBackground(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
  {
    Background? background = await context.Backgrounds
      .AsNoTracking()
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken);
    if (background == null)
    {
      return TypedResults.NotFound();
    }

    List<Feature> features = await Features(context)
      .Where(entry => entry.BackgroundId == background.Id)
      .ToListAsync(cancellationToken);
    IReadOnlyDictionary<string, string> names = await ItemNamesAsync(context, background.StartingEquipment, cancellationToken);
    return TypedResults.Ok(RuleMapper.Detail(background, features, RuleMapper.StartingEquipment(background.StartingEquipment, names)));
  }

  private static Task<PagedResult<ClassSummaryDto>> ListClasses(TableTopSheetsContext context, [AsParameters] ListQuery query, CancellationToken cancellationToken)
    => context.Classes
      .Include(entry => entry.Subclasses)
      .ToPagedAsync(query, ClassSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);

  private static async Task<Results<Ok<ClassDetailDto>, NotFound>> GetClass(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
  {
    CharacterClass? @class = await context.Classes
      .AsNoTracking()
      .Include(entry => entry.Subclasses)
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken);
    if (@class == null)
    {
      return TypedResults.NotFound();
    }

    List<Guid?> subclasses = @class.Subclasses
      .Select(entry => (Guid?)entry.Id)
      .ToList();
    List<Feature> features = await Features(context)
      .Where(entry => entry.ClassId == @class.Id || subclasses.Contains(entry.SubclassId))
      .ToListAsync(cancellationToken);
    IReadOnlyDictionary<string, string> names = await ItemNamesAsync(context, @class.StartingEquipment, cancellationToken);
    return TypedResults.Ok(RuleMapper.Detail(@class, features, RuleMapper.StartingEquipment(@class.StartingEquipment, names)));
  }

  private static Task<PagedResult<FeatSummaryDto>> ListFeats(TableTopSheetsContext context, [AsParameters] ListQuery query, [AsParameters] FeatFilter filter, CancellationToken cancellationToken)
  {
    List<FeatCategoryEnum> categories = Matching<FeatCategoryEnum>(filter.Category, ChoiceService.FeatCategoryKey);
    return context.Feats
      .WhereIf(filter.Category != null, entry => categories.Contains(entry.Category))
      .WhereIf(filter.Repeatable != null, entry => entry.Repeatable == filter.Repeatable)
      .WhereIf(filter.HasPrerequisite != null, entry => (entry.PrerequisiteExpr != null) == filter.HasPrerequisite)
      .ToPagedAsync(query, FeatSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);
  }

  private static async Task<Results<Ok<RuleDetailDto>, NotFound>> GetFeat(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
    => await context.Feats
      .AsNoTracking()
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken) is { } feat
      ? TypedResults.Ok(RuleMapper.Detail(feat, await Features(context)
        .Where(entry => entry.FeatId == feat.Id)
        .ToListAsync(cancellationToken)))
      : TypedResults.NotFound();

  private static Task<PagedResult<SpellSummaryDto>> ListSpells(TableTopSheetsContext context, [AsParameters] ListQuery query, [AsParameters] SpellFilter filter, CancellationToken cancellationToken)
  {
    // spell list keys are stored in lower case
    string? list = filter.Class?.ToLowerInvariant();
    string? castingTime = filter.CastingTime?.ToLowerInvariant();
    List<SpellSchoolEnum> schools = Matching<SpellSchoolEnum>(filter.School, SchoolKey);
    return context.Spells
      .WhereIf(list != null, entry => entry.Lists.Contains(list!))
      .WhereIf(filter.Level != null, entry => entry.Level == filter.Level)
      .WhereIf(filter.MinLevel != null, entry => entry.Level >= filter.MinLevel)
      .WhereIf(filter.MaxLevel != null, entry => entry.Level <= filter.MaxLevel)
      .WhereIf(filter.School != null, entry => schools.Contains(entry.School))
      .WhereIf(filter.Ritual != null, entry => entry.Ritual == filter.Ritual)
      .WhereIf(filter.Concentration != null, entry => entry.Concentration == filter.Concentration)
      .WhereIf(filter.Damage != null, entry => (entry.Damage != null) == filter.Damage)
      .WhereIf(filter.DamageType != null, entry => entry.DamageType == filter.DamageType)
      .WhereIf(castingTime != null, entry => entry.CastingTime
        .ToLower()
        .Contains(castingTime!))
      .ToPagedAsync(query, SpellSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);
  }

  private static async Task<Results<Ok<SpellDetailDto>, NotFound>> GetSpell(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
    => await context.Spells
      .AsNoTracking()
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken) is { } spell
      ? TypedResults.Ok(RuleMapper.Detail(spell))
      : TypedResults.NotFound();

  private static Task<PagedResult<ItemSummaryDto>> ListItems(TableTopSheetsContext context, [AsParameters] ListQuery query, [AsParameters] ItemFilter filter, CancellationToken cancellationToken)
  {
    // the category is the weapon category, the armor category, the tool group or the gear category (see RuleMapper.ItemCategory)
    string? category = filter.Category?.ToLowerInvariant();
    List<WeaponCategoryEnum> weapons = Matching<WeaponCategoryEnum>(category, entry => entry
      .ToString()
      .ToLowerInvariant());
    List<ArmorEnum> armors = Matching<ArmorEnum>(category, entry => entry
      .ToString()
      .ToLowerInvariant());
    return context.Items
      .WhereIf(filter.Kind != null, entry => entry.Kind == filter.Kind)
      .WhereIf(category != null, entry => (entry is Weapon && weapons.Contains(((Weapon)entry).Category))
                                          || (entry is Armor && armors.Contains(((Armor)entry).Category))
                                          || (entry.Group != null && entry.Group.ToLower() == category))
      .WhereIf(filter.Magic != null, entry => entry.Magic == filter.Magic)
      .WhereIf(filter.Attunement != null, entry => entry.RequiresAttunement == filter.Attunement)
      .ToPagedAsync(query, ItemSorts, cancellationToken: cancellationToken)
      .Select(RuleMapper.Summary);
  }

  private static async Task<Results<Ok<ItemDetailDto>, NotFound>> GetItem(TableTopSheetsContext context, string key, CancellationToken cancellationToken)
    => await context.Items
      .AsNoTracking()
      .Include(entry => ((Weapon)entry).Mastery)
      .FirstOrDefaultAsync(entry => entry.Key == key, cancellationToken) is { } item
      ? TypedResults.Ok(RuleMapper.Detail(item, await Features(context)
        .Where(entry => entry.ItemId == item.Id)
        .ToListAsync(cancellationToken)))
      : TypedResults.NotFound();

  /// <summary>
  ///   Names of the items named by starting equipment, by key
  /// </summary>
  public static async Task<IReadOnlyDictionary<string, string>> ItemNamesAsync
  (
    TableTopSheetsContext context,
    IEnumerable<StartingEquipmentOption> options,
    CancellationToken cancellationToken
  )
  {
    IReadOnlyList<string> keys = RuleMapper.ItemKeys(options);
    return await context.Items
      .AsNoTracking()
      .Where(entry => keys.Contains(entry.Key))
      .ToDictionaryAsync(entry => entry.Key, entry => entry.Name, cancellationToken);
  }

  /// <summary>
  ///   Features with their choices, as the detail endpoints show them
  /// </summary>
  private static IQueryable<Feature> Features(TableTopSheetsContext context) => context.Features
    .AsNoTracking()
    .Include(entry => entry.Choices);

  /// <summary>
  ///   Enum values whose key matches a filter (ignoring case), so the filter can run in the database
  /// </summary>
  private static List<TEnum> Matching<TEnum>(string? filter, Func<TEnum, string> key) where TEnum : struct, Enum
    => Enum
      .GetValues<TEnum>()
      .Where(entry => string.Equals(key(entry), filter, StringComparison.OrdinalIgnoreCase))
      .ToList();

  private static string SchoolKey(SpellSchoolEnum school) => school
    .ToString()
    .ToLowerInvariant();

  #endregion
}
