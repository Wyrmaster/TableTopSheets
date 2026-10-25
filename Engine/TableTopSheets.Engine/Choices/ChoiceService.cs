using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Expressions;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Choices;

/// <summary>
///   Answers which choices are open, which options are available and whether a selection is valid
/// </summary>
public static class ChoiceService
{
  #region Constants

  /// <summary>
  ///   Reason of options the filter of the choice excludes (e.g. spells of another class list); these are not part of the choice
  ///   at all, unlike options that are merely unavailable right now
  /// </summary>
  public const string NotPartOfChoice = "Not part of this choice";

  #endregion

  #region Methods

  /// <summary>
  ///   Total number of selections a choice instance allows right now
  /// </summary>
  public static int Total(CharacterEvaluation evaluation, Choice choice)
    => (int)Expression.Parse(choice.CountExpr).EvaluateNumber(evaluation);

  /// <summary>
  ///   Choice instances with fewer selections than allowed
  /// </summary>
  public static IEnumerable<PendingChoice> Pending(CharacterEvaluation evaluation)
  {
    foreach (FeatureInstance owner in evaluation.Features)
    {
      foreach (Choice choice in owner.Feature.Choices)
      {
        int total = Total(evaluation, choice);
        int selected = evaluation.SelectionsOf(choice, owner).Count();
        if (total > selected)
        {
          yield return new(choice, owner, total, selected);
        }
      }
    }
  }

  /// <summary>
  ///   Feature instance owning a choice within a selection scope
  /// </summary>
  public static FeatureInstance? FindOwner(CharacterEvaluation evaluation, Choice choice, Guid? scope)
    => evaluation.Features.FirstOrDefault(entry => entry.Feature.Id == choice.FeatureId && entry.SelectionScope == scope);

  /// <summary>
  ///   Every option of a choice before filtering (authored order for static options, otherwise ordered by name)
  /// </summary>
  public static IEnumerable<OptionDescriptor> AllOptions(RulesCatalog catalog, Choice choice)
    => choice.OptionSource is OptionSourceEnum.Static or OptionSourceEnum.Ability
      ? UnorderedOptions(catalog, choice)
      : UnorderedOptions(catalog, choice).OrderBy(entry => entry.Name, StringComparer.Ordinal);

  private static IEnumerable<OptionDescriptor> UnorderedOptions(RulesCatalog catalog, Choice choice) => choice.OptionSource switch
  {
    OptionSourceEnum.Static => choice.Options.Select(entry => new OptionDescriptor(entry.Key, entry.Name, Value: entry.Value, PrerequisiteExpr: entry.PrerequisiteExpr)),
    OptionSourceEnum.Ability => RuleKeys.Abilities.Select(entry => new OptionDescriptor(entry, Naming.Title(entry))),
    OptionSourceEnum.Skill => RuleKeys.Skills.Select(entry => new OptionDescriptor(entry, Naming.Title(entry))),
    OptionSourceEnum.Tool => catalog.Items.Values.OfType<Tool>().Select(entry => new OptionDescriptor(entry.Key, entry.Name, Category: entry.Group)),
    OptionSourceEnum.Language => catalog.Languages.Values.Select(entry => new OptionDescriptor(entry.Key, entry.Name, Category: entry.Rare ? "rare" : "standard")),
    OptionSourceEnum.Spell => catalog.Spells.Values.Select(entry => new OptionDescriptor
    (
      entry.Key,
      entry.Name,
      entry.Level,
      entry.School.ToString().ToLowerInvariant(),
      entry.Ritual,
      Damage: entry.Damage != null
    ) { Lists = entry.Lists }),
    OptionSourceEnum.Feat => catalog.Feats.Values.Select(entry => new OptionDescriptor
    (
      entry.Key,
      entry.Name,
      Category: FeatCategoryKey(entry.Category),
      Repeatable: entry.Repeatable,
      PrerequisiteExpr: entry.PrerequisiteExpr
    )),
    OptionSourceEnum.FeatureCategory => catalog.FeaturesByCategory.GetValueOrDefault(choice.Category ?? string.Empty, [])
      .Select(entry => new OptionDescriptor(entry.Key, entry.Name, entry.Level ?? 0, Category: entry.Category, PrerequisiteExpr: entry.PrerequisiteExpr)),
    OptionSourceEnum.Item => catalog.Items.Values.Select(entry => new OptionDescriptor(entry.Key, entry.Name, Category: entry.Kind.ToString().ToLowerInvariant())),
    OptionSourceEnum.Weapon => catalog.Items.Values.OfType<Weapon>().Select(entry => new OptionDescriptor(entry.Key, entry.Name, Category: entry.Category.ToString().ToLowerInvariant())),
    _ => [],
  };

  /// <summary>
  ///   Options that may be selected for a choice instance right now
  /// </summary>
  public static IEnumerable<OptionDescriptor> AvailableOptions(CharacterEvaluation evaluation, Choice choice, FeatureInstance owner)
    => EvaluateOptions(evaluation, choice, owner).Where(entry => entry.Available).Select(entry => entry.Option);

  /// <summary>
  ///   Every option of a choice instance with the reason it can't be selected right now (null when it can)
  /// </summary>
  public static IEnumerable<OptionAvailability> EvaluateOptions(CharacterEvaluation evaluation, Choice choice, FeatureInstance owner)
  {
    List<string> selected = evaluation.SelectionsOf(choice, owner).Select(entry => entry.Selection.OptionKey).ToList();
    HashSet<string> feats = KnownFeats(evaluation);
    HashSet<string> knownSpells = choice.OptionSource == OptionSourceEnum.Spell ? KnownSpellKeys(evaluation, choice, owner) : [];
    Expression? filter = choice.FilterExpr == null ? null : Expression.Parse(choice.FilterExpr);

    foreach (OptionDescriptor option in AllOptions(evaluation.Catalog, choice))
    {
      yield return new(option, Unavailable(evaluation, choice, option, selected, feats, knownSpells, filter));
    }
  }

  private static string? Unavailable
  (
    CharacterEvaluation evaluation,
    Choice choice,
    OptionDescriptor option,
    List<string> selected,
    HashSet<string> feats,
    HashSet<string> knownSpells,
    Expression? filter
  )
  {
    // the filter decides what belongs to the choice at all, so it is checked first
    OptionContext context = new(evaluation, option);
    if (filter != null && !filter.EvaluateBoolean(context))
    {
      return NotPartOfChoice;
    }

    int count = selected.Count(entry => entry == option.Key);
    int maximum = choice.AllowDuplicates ? Math.Max((byte)1, choice.MaxPerOption) : 1;
    if (count >= maximum)
    {
      return maximum == 1 ? "Already selected" : $"Already selected {count} time(s)";
    }

    if (choice.OptionSource == OptionSourceEnum.Feat && !option.Repeatable && feats.Contains(option.Key))
    {
      return "The character already has this feat";
    }

    // an option feature granted elsewhere (another instance of the choice) can't be taken twice
    if (choice.OptionSource == OptionSourceEnum.FeatureCategory && count == 0 && evaluation.Features.Any(entry => entry.Feature.Key == option.Key))
    {
      return "The character already has this feature";
    }

    if (count == 0 && knownSpells.Contains(option.Key))
    {
      return "The spell is already known";
    }

    if (option.PrerequisiteExpr != null && !Expression.Parse(option.PrerequisiteExpr).EvaluateBoolean(context))
    {
      return "Prerequisite not met";
    }

    return null;
  }

  /// <summary>
  ///   Validate a new selection; returns an error message or null
  /// </summary>
  public static string? Validate(CharacterEvaluation evaluation, Choice choice, FeatureInstance owner, string optionKey)
  {
    int total = Total(evaluation, choice);
    int selected = evaluation.SelectionsOf(choice, owner).Count();
    if (selected >= total)
    {
      return $"'{choice.Name}' allows only {total} selection(s)";
    }

    OptionAvailability? option = EvaluateOptions(evaluation, choice, owner).FirstOrDefault(entry => entry.Option.Key == optionKey);
    if (option is not { Available: true })
    {
      string reason = option?.Reason ?? "Unknown option";
      return $"'{optionKey}' is not an available option for '{choice.Name}' ({reason})";
    }

    return null;
  }

  /// <summary>
  ///   Spells that can't be chosen again for a spell choice: the same spell can't be learned twice for the same class and purpose
  ///   (a spell already in the spellbook can't be added again, a known cantrip can't be learned twice)
  /// </summary>
  private static HashSet<string> KnownSpellKeys(CharacterEvaluation evaluation, Choice choice, FeatureInstance owner)
  {
    Guid? classId = evaluation.OwningClass(owner)?.Id;
    HashSet<string> known = evaluation.Selections
      .Where(entry => entry.Choice.OptionSource == OptionSourceEnum.Spell
                      && entry.Choice.Category == choice.Category
                      && evaluation.OwningClass(entry.Owner)?.Id == classId)
      .Select(entry => entry.Selection.OptionKey)
      .ToHashSet();
    if (choice.Category == CharacterEvaluation.SpellbookCategory)
    {
      known.UnionWith(evaluation.Character.SpellbookEntries.Select(entry => evaluation.Catalog.Spells[entry.SpellId].Key));
    }

    return known;
  }

  /// <summary>
  ///   Keys of all feats the character has (origin feat of the background and selected feats)
  /// </summary>
  public static HashSet<string> KnownFeats(CharacterEvaluation evaluation)
    => evaluation.Features
      .Where(entry => entry.Feature.FeatId != null)
      .Select(entry => evaluation.Catalog.Feats[entry.Feature.FeatId!.Value].Key)
      .ToHashSet();

  public static string FeatCategoryKey(FeatCategoryEnum category) => category switch
  {
    FeatCategoryEnum.Origin => "origin",
    FeatCategoryEnum.General => "general",
    FeatCategoryEnum.FightingStyle => "fighting-style",
    FeatCategoryEnum.EpicBoon => "epic-boon",
    _ => category.ToString().ToLowerInvariant(),
  };

  #endregion
}
