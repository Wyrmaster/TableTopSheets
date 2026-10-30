using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Reads the rules a character needs from the database. Rows that were read are reused for the lifetime of the loader
///   (one request), so evaluating the character again after a change only reads what the change added
/// </summary>
public sealed class RulesCatalogLoader(TableTopSheetsContext context): IRulesCatalogLoader
{
  #region Fields

  private SmallTables? _small;
  private bool _coreFeaturesRead;

  private readonly Dictionary<Guid, Feature> _features = new();
  private readonly HashSet<Guid> _featureOwnersRead = new();
  private readonly HashSet<string> _featureKeysRead = new(StringComparer.Ordinal);

  private readonly RowCache<Species> _species = new();
  private readonly RowCache<Background> _backgrounds = new();
  private readonly RowCache<Feat> _feats = new();
  private readonly RowCache<Spell> _spells = new();
  private readonly RowCache<Item> _items = new();

  private readonly Dictionary<Guid, Choice> _choices = new();
  private readonly Dictionary<string, IReadOnlyList<OptionDescriptor>> _optionLists = new();

  #endregion

  #region Methods

  /// <inheritdoc />
  public async Task<RulesCatalog> LoadAsync(Character character, CancellationToken cancellationToken = default)
  {
    SmallTables small = this._small ??= await SmallTables.LoadAsync(context, cancellationToken);
    HashSet<string> optionKeys = character.Choices
      .Select(entry => entry.OptionKey)
      .ToHashSet(StringComparer.Ordinal);

    List<Species> species = await this._species.GetAsync(context.Species, Ids(character.SpeciesId), [], cancellationToken);
    List<Background> backgrounds = await this._backgrounds.GetAsync(context.Backgrounds, Ids(character.BackgroundId), [], cancellationToken);

    // feats are owned through the background or chosen by key
    List<Feat> ownedFeats = await this._feats.GetAsync
    (
      context.Feats,
      backgrounds
        .Select(entry => entry.OriginFeatId)
        .OfType<Guid>()
        .ToHashSet(),
      optionKeys,
      cancellationToken
    );

    HashSet<Guid> owners =
    [
      .. Ids(character.SpeciesId),
      .. Ids(character.BackgroundId),
      .. ownedFeats.Select(entry => entry.Id),
      .. character.Levels.Select(entry => entry.ClassId),
      .. character.Levels
        .Select(entry => entry.SubclassId)
        .OfType<Guid>(),
      .. character.Items.Select(entry => entry.ItemId),
    ];
    HashSet<string> featureKeys = [.. optionKeys, .. character.ActiveToggles];
    HashSet<Guid> grantedIds = new();
    HashSet<(Guid ChoiceId, string Key)> selected = character.Choices
      .Select(entry => (entry.ChoiceId, entry.OptionKey))
      .ToHashSet();

    // static options grant further features whose choices can grant more, so read until nothing new is granted
    List<Feature> features;
    while (true)
    {
      await this.ReadFeaturesAsync(owners, featureKeys, grantedIds, cancellationToken);
      features = this._features.Values
        .Where(entry => entry.OwnerType == OwnerTypeEnum.Core
                        || Owners(entry).Any(owners.Contains)
                        || featureKeys.Contains(entry.Key)
                        || grantedIds.Contains(entry.Id))
        .OrderBy(entry => entry.Order)
        .ToList();

      List<Guid> granted = features
        .SelectMany(entry => entry.Choices)
        .SelectMany(choice => choice.Options
          .Where(option => option.GrantsFeatureId != null && selected.Contains((choice.Id, option.Key)))
          .Select(option => option.GrantsFeatureId!.Value))
        .Where(entry => !grantedIds.Contains(entry))
        .ToList();
      if (granted.Count == 0)
      {
        break;
      }

      grantedIds.UnionWith(granted);
    }

    // keys named by effects (spell.fireball, proficiency.tool.thieves-tools ...) and the owners of the features
    HashSet<string> referencedKeys = features
      .SelectMany(entry => entry.Effects.Concat(entry.Choices.SelectMany(choice => choice.Effects)))
      .Select(entry => entry.Target[(entry.Target.LastIndexOf('.') + 1)..])
      .Concat(optionKeys)
      .ToHashSet(StringComparer.Ordinal);

    List<Feat> feats = await this._feats.GetAsync
    (
      context.Feats,
      [.. ownedFeats.Select(entry => entry.Id), .. features
        .Select(entry => entry.FeatId)
        .OfType<Guid>()],
      optionKeys,
      cancellationToken
    );
    List<Spell> spells = await this._spells.GetAsync
    (
      context.Spells,
      [
        .. character.SpellbookEntries.Select(entry => entry.SpellId),
        .. character.PreparedSpells.Select(entry => entry.SpellId),
        .. features
          .Select(entry => entry.SpellId)
          .OfType<Guid>(),
      ],
      referencedKeys,
      cancellationToken
    );
    List<Item> items = await this._items.GetAsync
    (
      context.Items,
      [.. character.Items.Select(entry => entry.ItemId), .. features
        .Select(entry => entry.ItemId)
        .OfType<Guid>()],
      referencedKeys,
      cancellationToken
    );

    List<Choice> choices = features
      .SelectMany(entry => entry.Choices)
      .ToList();
    List<Choice> extraChoices = await this.ReadChoicesAsync
    (
      character.Choices
        .Select(entry => entry.ChoiceId)
        .Where(id => choices.All(choice => choice.Id != id))
        .ToHashSet(),
      cancellationToken
    );

    return new RulesCatalog
    (
      small.Classes,
      species,
      backgrounds,
      feats,
      features,
      small.ScaleValues,
      small.SpellSlotRows,
      spells,
      items,
      small.Languages,
      small.Masteries,
      await this.OptionListsAsync(choices, small, cancellationToken),
      extraChoices
    );
  }

  /// <inheritdoc />
  public async Task DescribeAsync(RulesCatalog catalog, IEnumerable<(Choice Choice, string Key)> options, CancellationToken cancellationToken = default)
  {
    List<(Choice Choice, string Key)> missing = options
      .Distinct()
      .Where(entry => !catalog.TryDescribe(entry.Choice, entry.Key, out _))
      .ToList();
    if (missing.Count == 0)
    {
      return;
    }

    foreach (IGrouping<OptionSourceEnum, (Choice Choice, string Key)> group in missing.GroupBy(entry => entry.Choice.OptionSource))
    {
      if (group.Key == OptionSourceEnum.Static)
      {
        static Guid? GrantOf(Choice choice, string key) => choice.Options.FirstOrDefault(option => option.Key == key)?.GrantsFeatureId;

        List<Guid> ids = group
          .Select(entry => GrantOf(entry.Choice, entry.Key))
          .OfType<Guid>()
          .Distinct()
          .ToList();
        Dictionary<Guid, OptionText> granted = (await context.Features
            .Where(entry => ids.Contains(entry.Id))
            .Select(entry => new { entry.Id, entry.Name, entry.DescriptionHtml })
            .ToListAsync(cancellationToken))
          .ToDictionary(entry => entry.Id, entry => new OptionText(entry.DescriptionHtml, [entry.Name]));
        foreach ((Choice choice, string key) in group)
        {
          catalog.AddText(choice.Id, key, GrantOf(choice, key) is { } id && granted.TryGetValue(id, out OptionText? text) ? text : new OptionText(null, []));
        }

        continue;
      }

      List<string> keys = group
        .Select(entry => entry.Key)
        .Distinct()
        .ToList();
      IQueryable<KeyValuePair<string, string>>? query = group.Key switch
      {
        OptionSourceEnum.Spell => context.Spells
          .Where(entry => keys.Contains(entry.Key))
          .Select(entry => new KeyValuePair<string, string>(entry.Key, entry.DescriptionHtml)),
        OptionSourceEnum.Feat => context.Feats
          .Where(entry => keys.Contains(entry.Key))
          .Select(entry => new KeyValuePair<string, string>(entry.Key, entry.DescriptionHtml)),
        OptionSourceEnum.FeatureCategory => context.Features
          .Where(entry => keys.Contains(entry.Key))
          .Select(entry => new KeyValuePair<string, string>(entry.Key, entry.DescriptionHtml)),
        OptionSourceEnum.Tool or OptionSourceEnum.Item or OptionSourceEnum.Weapon
          => context.Items
            .Where(entry => keys.Contains(entry.Key))
            .Select(entry => new KeyValuePair<string, string>(entry.Key, entry.DescriptionHtml)),
        OptionSourceEnum.Language => context.Languages
          .Where(entry => keys.Contains(entry.Key))
          .Select(entry => new KeyValuePair<string, string>(entry.Key, entry.DescriptionHtml)),
        _ => null,
      };
      Dictionary<string, string> html = query == null ? new() : await query.ToDictionaryAsync(entry => entry.Key, entry => entry.Value, cancellationToken);
      foreach ((Choice choice, string key) in group)
      {
        catalog.AddText(choice.Id, key, new OptionText(html.GetValueOrDefault(key), []));
      }
    }
  }

  /// <inheritdoc />
  public Task DescribePendingAsync(CharacterEvaluation evaluation, CancellationToken cancellationToken = default)
    => this.DescribeAsync
    (
      evaluation.Catalog,
      ChoiceService
        .Pending(evaluation)
        .SelectMany(pending =>
        {
          List<OptionDescriptor> available = ChoiceService
            .AvailableOptions(evaluation, pending.Choice, pending.Owner)
            .ToList();
          return available.Count <= ChoiceService.EmbeddedOptionLimit ? available.Select(option => (pending.Choice, option.Key)) : [];
        })
        .ToList(),
      cancellationToken
    );

  #endregion

  #region Private Methods

  private static IEnumerable<Guid> Ids(Guid? id) => id is { } value ? [value] : [];

  private static IEnumerable<Guid> Owners(Feature feature)
    => new[] { feature.ClassId, feature.SubclassId, feature.SpeciesId, feature.BackgroundId, feature.FeatId, feature.ItemId }.OfType<Guid>();

  /// <summary>
  ///   Read the features of owners, by key and by id that were not read yet (the rule wide features with the first call)
  /// </summary>
  private async Task ReadFeaturesAsync(IEnumerable<Guid> owners, IEnumerable<string> keys, IEnumerable<Guid> ids, CancellationToken cancellationToken)
  {
    List<Guid?> newOwners = owners
      .Where(this._featureOwnersRead.Add)
      .Select(entry => (Guid?)entry)
      .ToList();
    List<string> newKeys = keys
      .Where(this._featureKeysRead.Add)
      .ToList();
    List<Guid> newIds = ids
      .Where(entry => !this._features.ContainsKey(entry))
      .ToList();
    bool core = !this._coreFeaturesRead;
    if (newOwners.Count == 0 && newKeys.Count == 0 && newIds.Count == 0 && !core)
    {
      return;
    }

    List<Feature> features = await context.Features
      .AsNoTracking()
      .AsSplitQuery()
      .Include(entry => entry.Effects)
      .Include(entry => entry.Choices)
      .ThenInclude(entry => entry.Options)
      .Include(entry => entry.Choices)
      .ThenInclude(entry => entry.Effects)
      .Include(entry => entry.Resources)
      .Include(entry => entry.Spellcasting)
      .Where(entry => (core && entry.OwnerType == OwnerTypeEnum.Core)
                      || newOwners.Contains(entry.ClassId)
                      || newOwners.Contains(entry.SubclassId)
                      || newOwners.Contains(entry.SpeciesId)
                      || newOwners.Contains(entry.BackgroundId)
                      || newOwners.Contains(entry.FeatId)
                      || newOwners.Contains(entry.ItemId)
                      || newKeys.Contains(entry.Key)
                      || newIds.Contains(entry.Id))
      .ToListAsync(cancellationToken);
    foreach (Feature feature in features)
    {
      this._features.TryAdd(feature.Id, feature);
    }

    this._coreFeaturesRead = true;
  }

  private async Task<List<Choice>> ReadChoicesAsync(HashSet<Guid> ids, CancellationToken cancellationToken)
  {
    List<Guid> missing = ids
      .Where(id => !this._choices.ContainsKey(id))
      .ToList();
    if (missing.Count > 0)
    {
      foreach (Choice choice in await context.Choices
        .AsNoTracking()
        .Where(entry => missing.Contains(entry.Id))
        .ToListAsync(cancellationToken))
      {
        this._choices[choice.Id] = choice;
      }
    }

    return ids
      .Select(id => this._choices.GetValueOrDefault(id))
      .OfType<Choice>()
      .ToList();
  }

  private async Task<Dictionary<string, IReadOnlyList<OptionDescriptor>>> OptionListsAsync(IEnumerable<Choice> choices, SmallTables small, CancellationToken cancellationToken)
  {
    Dictionary<string, IReadOnlyList<OptionDescriptor>> lists = new();
    foreach (Choice choice in choices.Where(entry => OptionProjections.FromTable(entry.OptionSource)))
    {
      string key = OptionProjections.ListKey(choice);
      if (lists.ContainsKey(key))
      {
        continue;
      }

      if (!this._optionLists.TryGetValue(key, out IReadOnlyList<OptionDescriptor>? options))
      {
        options = choice.OptionSource switch
        {
          OptionSourceEnum.Tool => await context.Items
            .OfType<Tool>()
            .Select(OptionProjections.Tool)
            .ToListAsync(cancellationToken),
          OptionSourceEnum.Language => small.Languages
            .Select(OptionProjections.LanguageOption)
            .ToList(),
          OptionSourceEnum.Spell => await context.Spells
            .Select(OptionProjections.Spell)
            .ToListAsync(cancellationToken),
          OptionSourceEnum.Feat => await context.Feats
            .Select(OptionProjections.Feat)
            .ToListAsync(cancellationToken),
          OptionSourceEnum.FeatureCategory => await context.Features
            .Where(entry => entry.Category != null && entry.Category == choice.Category)
            .Select(OptionProjections.Feature)
            .ToListAsync(cancellationToken),
          OptionSourceEnum.Item => await context.Items
            .Select(OptionProjections.Item)
            .ToListAsync(cancellationToken),
          OptionSourceEnum.Weapon => await context.Items
            .OfType<Weapon>()
            .Select(OptionProjections.Weapon)
            .ToListAsync(cancellationToken),
          _ => [],
        };
        this._optionLists[key] = options;
      }

      lists[key] = options;
    }

    return lists;
  }

  #endregion

  #region Nested Types

  /// <summary>
  ///   Tables that are small and referenced by key from expressions (classes, scale values, slot tables ...) are read whole
  /// </summary>
  private sealed record SmallTables
  (
    List<CharacterClass> Classes,
    List<ScaleValue> ScaleValues,
    List<SpellSlotRow> SpellSlotRows,
    List<Language> Languages,
    List<WeaponMastery> Masteries
  )
  {
    public static async Task<SmallTables> LoadAsync(TableTopSheetsContext context, CancellationToken cancellationToken) => new
    (
      await context.Classes
        .AsNoTracking()
        .Include(entry => entry.Subclasses)
        .ToListAsync(cancellationToken),
      await context.ScaleValues
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.SpellSlotRows
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.Languages
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.WeaponMasteries
        .AsNoTracking()
        .ToListAsync(cancellationToken)
    );
  }

  /// <summary>
  ///   Rule rows read by id or key; keys that matched nothing are remembered so they are not asked for again
  /// </summary>
  private sealed class RowCache<T> where T : RuleEntity
  {
    private readonly Dictionary<Guid, T> _rows = new();
    private readonly HashSet<string> _keysRead = new(StringComparer.Ordinal);

    public async Task<List<T>> GetAsync(IQueryable<T> source, IEnumerable<Guid> idList, IEnumerable<string> keyList, CancellationToken cancellationToken)
    {
      HashSet<Guid> ids = idList.ToHashSet();
      HashSet<string> keys = keyList.ToHashSet(StringComparer.Ordinal);
      List<Guid> newIds = ids
        .Where(id => !this._rows.ContainsKey(id))
        .ToList();
      List<string> newKeys = keys
        .Where(this._keysRead.Add)
        .ToList();
      if (newIds.Count > 0 || newKeys.Count > 0)
      {
        foreach (T row in await source
          .AsNoTracking()
          .Where(entry => newIds.Contains(entry.Id) || newKeys.Contains(entry.Key))
          .ToListAsync(cancellationToken))
        {
          this._rows.TryAdd(row.Id, row);
          this._keysRead.Add(row.Key);
        }
      }

      return this._rows.Values
        .Where(entry => ids.Contains(entry.Id) || keys.Contains(entry.Key))
        .ToList();
    }
  }

  #endregion
}
