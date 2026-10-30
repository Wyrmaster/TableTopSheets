using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Rules an evaluation works with. Either the rules one character needs (read per request by <see cref="RulesCatalogLoader"/>)
///   or every rule (seed validation, <see cref="LoadAsync"/>)
/// </summary>
public sealed class RulesCatalog
{
  #region Fields

  private static readonly OptionText NoText = new(null, []);

  private readonly bool _complete;
  private readonly Dictionary<string, IReadOnlyList<OptionDescriptor>> _optionLists;
  private readonly Dictionary<(Guid ChoiceId, string Key), OptionText> _texts = new();

  #endregion

  #region Constructor

  /// <param name="optionLists">
  ///   options of the choices whose options come from a rule table (see <see cref="OptionProjections.ListKey"/>); null when the
  ///   catalog holds every rule, the options are then built from the given entities
  /// </param>
  /// <param name="extraChoices">choices that are not part of the given features (names of orphaned selections)</param>
  public RulesCatalog
  (
    IEnumerable<CharacterClass> classes,
    IEnumerable<Species> species,
    IEnumerable<Background> backgrounds,
    IEnumerable<Feat> feats,
    IEnumerable<Feature> features,
    IEnumerable<ScaleValue> scaleValues,
    IEnumerable<SpellSlotRow> spellSlotRows,
    IEnumerable<Spell> spells,
    IEnumerable<Item> items,
    IEnumerable<Language> languages,
    IEnumerable<WeaponMastery> masteries,
    IReadOnlyDictionary<string, IReadOnlyList<OptionDescriptor>>? optionLists = null,
    IEnumerable<Choice>? extraChoices = null
  )
  {
    this.Classes = classes.ToDictionary(entry => entry.Id);
    this.Subclasses = this.Classes.Values
      .SelectMany(entry => entry.Subclasses)
      .ToDictionary(entry => entry.Id);
    this.Species = species.ToDictionary(entry => entry.Id);
    this.Backgrounds = backgrounds.ToDictionary(entry => entry.Id);
    this.Feats = feats.ToDictionary(entry => entry.Id);
    this.Features = features.ToDictionary(entry => entry.Id);
    this.Spells = spells.ToDictionary(entry => entry.Id);
    this.Items = items.ToDictionary(entry => entry.Id);
    this.Languages = languages.ToDictionary(entry => entry.Key);
    this.Masteries = masteries.ToDictionary(entry => entry.Id);

    this.ClassesByKey = this.Classes.Values.ToDictionary(entry => entry.Key);
    this.SubclassesByKey = this.Subclasses.Values.ToDictionary(entry => entry.Key);
    this.FeatsByKey = this.Feats.Values.ToDictionary(entry => entry.Key);
    this.FeaturesByKey = this.Features.Values.ToDictionary(entry => entry.Key);
    this.SpellsByKey = this.Spells.Values.ToDictionary(entry => entry.Key);
    this.ItemsByKey = this.Items.Values.ToDictionary(entry => entry.Key);
    Dictionary<Guid, Choice> choices = this.Features.Values
      .SelectMany(entry => entry.Choices)
      .ToDictionary(entry => entry.Id);
    foreach (Choice choice in extraChoices ?? [])
    {
      choices.TryAdd(choice.Id, choice);
    }

    this.Choices = choices;
    this.ChoicesByKey = this.Choices.Values.ToDictionary(entry => entry.Key);
    this.Resources = this.Features.Values
      .SelectMany(entry => entry.Resources)
      .ToList();
    this.FeaturesByCategory = this.Features.Values
      .Where(entry => entry.Category != null)
      .GroupBy(entry => entry.Category!)
      .ToDictionary(group => group.Key, group => (IReadOnlyList<Feature>)group
        .OrderBy(entry => entry.Name)
        .ToList());

    this.ScaleValues = scaleValues
      .GroupBy(entry => entry.Key)
      .ToDictionary(group => group.Key, group => (IReadOnlyList<ScaleValue>)group
        .OrderBy(entry => entry.Level)
        .ToList());

    this.SpellSlotTables = spellSlotRows
      .GroupBy(entry => entry.TableKey)
      .ToDictionary
      (
        group => group.Key,
        group => group
          .GroupBy(entry => entry.CasterLevel)
          .ToDictionary
          (
            levelGroup => (int)levelGroup.Key,
            levelGroup =>
            {
              int[] slots = new int[10];
              foreach (SpellSlotRow row in levelGroup)
              {
                slots[row.SpellLevel] = row.Count;
              }

              return slots;
            }
          )
      );

    this._complete = optionLists == null;
    this._optionLists = optionLists?.ToDictionary() ?? this.BuildOptionLists();
  }

  #endregion

  #region Properties

  public IReadOnlyDictionary<Guid, CharacterClass> Classes { get; }

  public IReadOnlyDictionary<string, CharacterClass> ClassesByKey { get; }

  public IReadOnlyDictionary<Guid, Subclass> Subclasses { get; }

  public IReadOnlyDictionary<string, Subclass> SubclassesByKey { get; }

  public IReadOnlyDictionary<Guid, Species> Species { get; }

  public IReadOnlyDictionary<Guid, Background> Backgrounds { get; }

  public IReadOnlyDictionary<Guid, Feat> Feats { get; }

  public IReadOnlyDictionary<string, Feat> FeatsByKey { get; }

  public IReadOnlyDictionary<Guid, Feature> Features { get; }

  public IReadOnlyDictionary<string, Feature> FeaturesByKey { get; }

  public IReadOnlyDictionary<string, IReadOnlyList<Feature>> FeaturesByCategory { get; }

  public IReadOnlyDictionary<Guid, Choice> Choices { get; }

  public IReadOnlyDictionary<string, Choice> ChoicesByKey { get; }

  public IReadOnlyList<ResourceDefinition> Resources { get; }

  public IReadOnlyDictionary<string, IReadOnlyList<ScaleValue>> ScaleValues { get; }

  /// <summary>
  ///   Slot tables: table key → caster level → slots per spell level (index 1..9)
  /// </summary>
  public IReadOnlyDictionary<string, Dictionary<int, int[]>> SpellSlotTables { get; }

  public IReadOnlyDictionary<Guid, Spell> Spells { get; }

  public IReadOnlyDictionary<string, Spell> SpellsByKey { get; }

  public IReadOnlyDictionary<Guid, Item> Items { get; }

  public IReadOnlyDictionary<string, Item> ItemsByKey { get; }

  public IReadOnlyDictionary<string, Language> Languages { get; }

  public IReadOnlyDictionary<Guid, WeaponMastery> Masteries { get; }

  #endregion

  #region Methods

  /// <summary>
  ///   Load every rule from the database (seed validation; evaluations use <see cref="RulesCatalogLoader"/>)
  /// </summary>
  public static async Task<RulesCatalog> LoadAsync(TableTopSheetsContext context, CancellationToken cancellationToken = default)
  {
    List<CharacterClass> classes = await context
      .Classes
      .AsNoTracking()
      .Include(entry => entry.Subclasses)
      .ToListAsync(cancellationToken);
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
      .ToListAsync(cancellationToken);

    return new RulesCatalog
    (
      classes,
      await context.Species
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.Backgrounds
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.Feats
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      features,
      await context.ScaleValues
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.SpellSlotRows
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.Spells
        .AsNoTracking()
        .ToListAsync(cancellationToken),
      await context.Items
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
  ///   Features owned by a rule entity
  /// </summary>
  public IEnumerable<Feature> FeaturesOf(Guid ownerId) => this.Features.Values
    .Where(entry => entry.ClassId == ownerId
                    || entry.SubclassId == ownerId
                    || entry.SpeciesId == ownerId
                    || entry.BackgroundId == ownerId
                    || entry.FeatId == ownerId
                    || entry.ItemId == ownerId
                    || entry.SpellId == ownerId)
    .OrderBy(entry => entry.Level ?? 0)
    .ThenBy(entry => entry.Order);

  /// <summary>
  ///   Class a class or subclass feature counts for (null for other owners)
  /// </summary>
  public CharacterClass? OwningClass(Feature feature)
  {
    if (feature.ClassId is { } classId)
    {
      return this.Classes[classId];
    }

    return feature.SubclassId is { } subclassId ? this.Classes[this.Subclasses[subclassId].ClassId] : null;
  }

  /// <summary>
  ///   Every option of a choice whose options come from a rule table (see <see cref="OptionProjections.FromTable"/>)
  /// </summary>
  public IReadOnlyList<OptionDescriptor> OptionList(Choice choice)
    => this._optionLists.TryGetValue(OptionProjections.ListKey(choice), out IReadOnlyList<OptionDescriptor>? options)
      ? options
      : this._complete
        ? []
        : throw new InvalidOperationException($"The options of '{choice.Key}' were not loaded");

  /// <summary>
  ///   Option lists the catalog holds
  /// </summary>
  public bool HasOptionList(string listKey) => this._optionLists.ContainsKey(listKey);

  /// <summary>
  ///   Description and granted feature names of an option; false when the catalog doesn't know them yet
  ///   (<see cref="RulesCatalogLoader.DescribeAsync"/> reads them)
  /// </summary>
  public bool TryDescribe(Choice choice, string optionKey, out OptionText text)
  {
    if (this._texts.TryGetValue((choice.Id, optionKey), out OptionText? stored))
    {
      text = stored;
      return true;
    }

    OptionText? found = choice.OptionSource switch
    {
      OptionSourceEnum.Static => choice.Options.FirstOrDefault(entry => entry.Key == optionKey)?.GrantsFeatureId is { } featureId
        ? this.Features.TryGetValue(featureId, out Feature? granted) ? new OptionText(granted.DescriptionHtml, [granted.Name]) : null
        : NoText,
      OptionSourceEnum.Spell => this.SpellsByKey.TryGetValue(optionKey, out Spell? spell) ? new OptionText(spell.DescriptionHtml, []) : null,
      OptionSourceEnum.Feat => this.FeatsByKey.TryGetValue(optionKey, out Feat? feat) ? new OptionText(feat.DescriptionHtml, []) : null,
      OptionSourceEnum.FeatureCategory => this.FeaturesByKey.TryGetValue(optionKey, out Feature? feature) ? new OptionText(feature.DescriptionHtml, []) : null,
      OptionSourceEnum.Tool or OptionSourceEnum.Item or OptionSourceEnum.Weapon
        => this.ItemsByKey.TryGetValue(optionKey, out Item? item) ? new OptionText(item.DescriptionHtml, []) : null,
      OptionSourceEnum.Language => this.Languages.TryGetValue(optionKey, out Language? language) ? new OptionText(language.DescriptionHtml, []) : null,
      _ => NoText,
    };

    text = found ?? NoText;
    return found != null || this._complete;
  }

  /// <summary>
  ///   Description and granted feature names of an option (empty when unknown)
  /// </summary>
  public OptionText Describe(Choice choice, string optionKey) => this.TryDescribe(choice, optionKey, out OptionText text) ? text : NoText;

  internal void AddText(Guid choiceId, string optionKey, OptionText text) => this._texts[(choiceId, optionKey)] = text;

  private Dictionary<string, IReadOnlyList<OptionDescriptor>> BuildOptionLists()
  {
    Dictionary<string, IReadOnlyList<OptionDescriptor>> lists = new()
    {
      [nameof(OptionSourceEnum.Tool)] = this.Items.Values
        .OfType<Tool>()
        .Select(OptionProjections.ToolOption)
        .ToList(),
      [nameof(OptionSourceEnum.Language)] = this.Languages.Values
        .Select(OptionProjections.LanguageOption)
        .ToList(),
      [nameof(OptionSourceEnum.Spell)] = this.Spells.Values
        .Select(OptionProjections.SpellOption)
        .ToList(),
      [nameof(OptionSourceEnum.Feat)] = this.Feats.Values
        .Select(OptionProjections.FeatOption)
        .ToList(),
      [nameof(OptionSourceEnum.Item)] = this.Items.Values
        .Select(OptionProjections.ItemOption)
        .ToList(),
      [nameof(OptionSourceEnum.Weapon)] = this.Items.Values
        .OfType<Weapon>()
        .Select(OptionProjections.WeaponOption)
        .ToList(),
    };
    foreach ((string category, IReadOnlyList<Feature> features) in this.FeaturesByCategory)
    {
      lists[$"{OptionSourceEnum.FeatureCategory}:{category}"] = features
        .Select(OptionProjections.FeatureOption)
        .ToList();
    }

    return lists;
  }

  #endregion
}
