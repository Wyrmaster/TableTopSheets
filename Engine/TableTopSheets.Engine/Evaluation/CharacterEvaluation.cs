using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Computes everything about a character from the rules catalog and the character journal.
///   Nothing is stored: a new evaluation is created whenever the character changes
/// </summary>
public sealed partial class CharacterEvaluation
{
  #region Fields

  private readonly List<FeatureInstance> _features = new();
  private readonly List<SelectionInstance> _selections = new();
  private readonly Dictionary<string, List<AppliedEffect>> _effects = new();

  #endregion

  #region Constructor

  public CharacterEvaluation(RulesCatalog catalog, Character character)
  {
    this.Catalog = catalog;
    this.Character = character;
    this.Levels = character.Levels.OrderBy(entry => entry.Number).ToList();

    this.ClassLevels = this.Levels
      .GroupBy(entry => entry.ClassId)
      .ToDictionary(group => group.Key, group => group.Count());
    this.Subclasses = this.Levels
      .Where(entry => entry.SubclassId != null)
      .ToDictionary(entry => entry.ClassId, entry => catalog.Subclasses[entry.SubclassId!.Value]);
    this.FirstClass = this.Levels.Count > 0 ? catalog.Classes[this.Levels[0].ClassId] : null;

    this.CollectFeatures();
    this.CollectEffects();
  }

  #endregion

  #region Properties

  public RulesCatalog Catalog { get; }

  public Character Character { get; }

  /// <summary>
  ///   Level journal ordered by character level
  /// </summary>
  public IReadOnlyList<CharacterLevel> Levels { get; }

  public int TotalLevel => this.Levels.Count;

  /// <summary>
  ///   Level per class id
  /// </summary>
  public IReadOnlyDictionary<Guid, int> ClassLevels { get; }

  /// <summary>
  ///   Chosen subclass per class id
  /// </summary>
  public IReadOnlyDictionary<Guid, Subclass> Subclasses { get; }

  public CharacterClass? FirstClass { get; }

  public int ProficiencyBonus => this.TotalLevel == 0 ? 2 : 2 + (this.TotalLevel - 1) / 4;

  public IReadOnlyList<FeatureInstance> Features => this._features;

  public IReadOnlyList<SelectionInstance> Selections => this._selections;

  public IEnumerable<AppliedEffect> Effects => this._effects.Values.SelectMany(entry => entry);

  /// <summary>
  ///   Stored selections that are not reachable from an active feature (e.g. the feature was lost)
  /// </summary>
  public IEnumerable<CharacterChoice> OrphanedSelections => this.Character.Choices
    .Where(entry => entry.SupersededByLevelId == null)
    .Where(entry => this._selections.All(selection => selection.Selection.Id != entry.Id));

  #endregion

  #region Public Methods

  public int ClassLevel(Guid classId) => this.ClassLevels.GetValueOrDefault(classId);

  public int ClassLevel(string classKey)
    => this.Catalog.ClassesByKey.TryGetValue(classKey, out CharacterClass? @class) ? this.ClassLevel(@class.Id) : 0;

  /// <summary>
  ///   Active selections of a choice within the scope of a feature instance
  /// </summary>
  public IEnumerable<SelectionInstance> SelectionsOf(Choice choice, FeatureInstance owner)
    => this._selections.Where(entry => entry.Choice.Id == choice.Id && entry.Owner == owner);

  public bool HasFeature(string key) => this._features.Any(entry => entry.Active && entry.Feature.Key == key);

  #endregion

  #region Feature Collection

  private void CollectFeatures()
  {
    Queue<FeatureInstance> queue = new();

    void Add(Feature feature, CharacterChoice? grantedBy, string source)
    {
      bool active = !feature.Toggleable || this.Character.ActiveToggles.Contains(feature.Key);
      FeatureInstance instance = new(feature, grantedBy, active, source);
      this._features.Add(instance);
      queue.Enqueue(instance);
    }

    if (this.Character.SpeciesId is { } speciesId)
    {
      Species species = this.Catalog.Species[speciesId];
      foreach (Feature feature in this.Catalog.FeaturesOf(speciesId))
      {
        Add(feature, null, species.Name);
      }
    }

    if (this.Character.BackgroundId is { } backgroundId)
    {
      Background background = this.Catalog.Backgrounds[backgroundId];
      foreach (Feature feature in this.Catalog.FeaturesOf(backgroundId))
      {
        Add(feature, null, background.Name);
      }

      if (background.OriginFeatId is { } featId)
      {
        foreach (Feature feature in this.Catalog.FeaturesOf(featId))
        {
          Add(feature, null, $"{background.Name} (Origin Feat)");
        }
      }
    }

    foreach ((Guid classId, int level) in this.ClassLevels.OrderBy(entry => this.Levels.First(level => level.ClassId == entry.Key).Number))
    {
      CharacterClass @class = this.Catalog.Classes[classId];
      bool firstClass = this.FirstClass?.Id == classId;
      foreach (Feature feature in this.Catalog.FeaturesOf(classId).Where(entry => (entry.Level ?? 1) <= level))
      {
        bool granted = feature.GrantMode switch
        {
          GrantModeEnum.FirstClassOnly => firstClass,
          GrantModeEnum.MulticlassOnly => !firstClass,
          _ => true,
        };
        if (granted)
        {
          Add(feature, null, $"{@class.Name} {feature.Level ?? 1}");
        }
      }

      if (this.Subclasses.TryGetValue(classId, out Subclass? subclass))
      {
        foreach (Feature feature in this.Catalog.FeaturesOf(subclass.Id).Where(entry => (entry.Level ?? 1) <= level))
        {
          Add(feature, null, $"{subclass.Name} {feature.Level ?? 1}");
        }
      }
    }

    foreach (CharacterItem item in this.Character.Items.Where(entry => entry.Equipped))
    {
      Persistence.Entity.Rules.Items.Item rule = this.Catalog.Items[item.ItemId];
      if (rule.RequiresAttunement && !item.Attuned)
      {
        continue;
      }

      foreach (Feature feature in this.Catalog.FeaturesOf(rule.Id))
      {
        Add(feature, null, rule.Name);
      }
    }

    foreach (Feature feature in this.Catalog.Features.Values.Where(entry => entry.OwnerType == OwnerTypeEnum.Core))
    {
      Add(feature, null, "Rules");
    }

    // effects of spells (Mage Armor, Shield of Faith ...) only exist while toggled on
    foreach (Feature feature in this.Catalog.Features.Values.Where(entry => entry.OwnerType == OwnerTypeEnum.Spell && this.Character.ActiveToggles.Contains(entry.Key)))
    {
      Add(feature, null, "Spell");
    }

    List<CharacterChoice> stored = this.Character.Choices.Where(entry => entry.SupersededByLevelId == null).ToList();
    while (queue.TryDequeue(out FeatureInstance? instance))
    {
      foreach (Choice choice in instance.Feature.Choices)
      {
        foreach (CharacterChoice selection in stored.Where(entry => entry.ChoiceId == choice.Id && entry.ParentId == instance.SelectionScope).OrderBy(entry => entry.CreatedAt))
        {
          ChoiceOption? option = choice.OptionSource == OptionSourceEnum.Static
            ? choice.Options.FirstOrDefault(entry => entry.Key == selection.OptionKey)
            : null;
          this._selections.Add(new(selection, choice, instance, option));

          foreach ((Feature feature, string source) in this.GrantedFeatures(choice, selection, option))
          {
            Add(feature, selection, source);
          }
        }
      }
    }
  }

  private IEnumerable<(Feature, string)> GrantedFeatures(Choice choice, CharacterChoice selection, ChoiceOption? option)
  {
    switch (choice.OptionSource)
    {
      case OptionSourceEnum.FeatureCategory when this.Catalog.FeaturesByKey.TryGetValue(selection.OptionKey, out Feature? feature):
        yield return (feature, choice.Name);
        break;
      case OptionSourceEnum.Feat when this.Catalog.FeatsByKey.TryGetValue(selection.OptionKey, out Feat? feat):
        foreach (Feature feature in this.Catalog.FeaturesOf(feat.Id))
        {
          yield return (feature, $"Feat ({choice.Name})");
        }

        break;
      case OptionSourceEnum.Static when option?.GrantsFeatureId is { } featureId:
        yield return (this.Catalog.Features[featureId], choice.Name);
        break;
    }
  }

  private void CollectEffects()
  {
    void Add(AppliedEffect effect)
    {
      if (!this._effects.TryGetValue(effect.Target, out List<AppliedEffect>? list))
      {
        this._effects[effect.Target] = list = new();
      }

      list.Add(effect);
    }

    foreach (FeatureInstance instance in this._features.Where(entry => entry.Active))
    {
      foreach (Effect effect in instance.Feature.Effects.OrderBy(entry => entry.Order))
      {
        Add(new(effect, effect.Target, effect.ValueExpr, effect.Label ?? instance.Feature.Name, instance));
      }
    }

    foreach (SelectionInstance selection in this._selections.Where(entry => entry.Owner.Active))
    {
      foreach (Effect effect in selection.Choice.Effects.OrderBy(entry => entry.Order))
      {
        string Substitute(string text) => text
          .Replace("$option", selection.Selection.OptionKey)
          .Replace("$value", selection.StaticOption?.Value ?? selection.Selection.OptionKey);
        Add(new
        (
          effect,
          Substitute(effect.Target),
          Substitute(effect.ValueExpr),
          effect.Label ?? $"{selection.Owner.Feature.Name}: {this.OptionName(selection.Choice, selection.Selection.OptionKey)}",
          selection.Owner
        ));
      }
    }
  }

  /// <summary>
  ///   Display name of an option key
  /// </summary>
  public string OptionName(Choice choice, string optionKey) => choice.OptionSource switch
  {
    OptionSourceEnum.Static => choice.Options.FirstOrDefault(entry => entry.Key == optionKey)?.Name ?? optionKey,
    OptionSourceEnum.Spell => this.Catalog.SpellsByKey.TryGetValue(optionKey, out Spell? spell) ? spell.Name : optionKey,
    OptionSourceEnum.Feat => this.Catalog.FeatsByKey.TryGetValue(optionKey, out Feat? feat) ? feat.Name : optionKey,
    OptionSourceEnum.FeatureCategory => this.Catalog.FeaturesByKey.TryGetValue(optionKey, out Feature? feature) ? feature.Name : optionKey,
    OptionSourceEnum.Language => this.Catalog.Languages.TryGetValue(optionKey, out Language? language) ? language.Name : optionKey,
    OptionSourceEnum.Item or OptionSourceEnum.Weapon or OptionSourceEnum.Tool
      => this.Catalog.ItemsByKey.TryGetValue(optionKey, out Persistence.Entity.Rules.Items.Item? item) ? item.Name : optionKey,
    _ => Naming.Title(optionKey),
  };

  #endregion
}
