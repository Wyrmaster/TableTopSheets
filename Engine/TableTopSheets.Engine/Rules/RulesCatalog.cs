using Microsoft.EntityFrameworkCore;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Complete, read only snapshot of the rule tables. Loaded once and shared by every evaluation
/// </summary>
public sealed class RulesCatalog
{
  #region Constructor

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
    IEnumerable<WeaponMastery> masteries
  )
  {
    this.Classes = classes.ToDictionary(entry => entry.Id);
    this.Subclasses = this.Classes.Values.SelectMany(entry => entry.Subclasses).ToDictionary(entry => entry.Id);
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
    this.Choices = this.Features.Values.SelectMany(entry => entry.Choices).ToDictionary(entry => entry.Id);
    this.ChoicesByKey = this.Choices.Values.ToDictionary(entry => entry.Key);
    this.Resources = this.Features.Values.SelectMany(entry => entry.Resources).ToList();
    this.FeaturesByCategory = this.Features.Values
      .Where(entry => entry.Category != null)
      .GroupBy(entry => entry.Category!)
      .ToDictionary(group => group.Key, group => (IReadOnlyList<Feature>)group.OrderBy(entry => entry.Name).ToList());

    this.ScaleValues = scaleValues
      .GroupBy(entry => entry.Key)
      .ToDictionary(group => group.Key, group => (IReadOnlyList<ScaleValue>)group.OrderBy(entry => entry.Level).ToList());

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
  ///   Load the complete rule set from the database
  /// </summary>
  public static async Task<RulesCatalog> LoadAsync(TableTopSheetsContext context, CancellationToken cancellationToken = default)
  {
    List<CharacterClass> classes = await context.Classes.AsNoTracking().Include(entry => entry.Subclasses).ToListAsync(cancellationToken);
    List<Feature> features = await context.Features
      .AsNoTracking()
      .AsSplitQuery()
      .Include(entry => entry.Effects)
      .Include(entry => entry.Choices).ThenInclude(entry => entry.Options)
      .Include(entry => entry.Choices).ThenInclude(entry => entry.Effects)
      .Include(entry => entry.Resources)
      .Include(entry => entry.Spellcasting)
      .ToListAsync(cancellationToken);

    return new RulesCatalog
    (
      classes,
      await context.Species.AsNoTracking().ToListAsync(cancellationToken),
      await context.Backgrounds.AsNoTracking().ToListAsync(cancellationToken),
      await context.Feats.AsNoTracking().ToListAsync(cancellationToken),
      features,
      await context.ScaleValues.AsNoTracking().ToListAsync(cancellationToken),
      await context.SpellSlotRows.AsNoTracking().ToListAsync(cancellationToken),
      await context.Spells.AsNoTracking().ToListAsync(cancellationToken),
      await context.Items.AsNoTracking().ToListAsync(cancellationToken),
      await context.Languages.AsNoTracking().ToListAsync(cancellationToken),
      await context.WeaponMasteries.AsNoTracking().ToListAsync(cancellationToken)
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

  #endregion
}
