using System.Linq.Expressions;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Choices;

/// <summary>
///   Options of the choices whose options come from a rule table. The expressions are translated to SQL by the loader and
///   compiled for catalogs built in memory, so both produce the same options
/// </summary>
public static class OptionProjections
{
  #region Projections

  public static readonly Expression<Func<Spell, OptionDescriptor>> Spell = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    entry.Level,
    entry.School.ToString().ToLowerInvariant(),
    entry.Ritual,
    null,
    null,
    false,
    null,
    entry.Damage != null
  ) { Lists = entry.Lists };

  public static readonly Expression<Func<Feat, OptionDescriptor>> Feat = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    0,
    null,
    false,
    ChoiceService.FeatCategoryKey(entry.Category),
    null,
    entry.Repeatable,
    entry.PrerequisiteExpr,
    false
  );

  public static readonly Expression<Func<Feature, OptionDescriptor>> Feature = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    entry.Level ?? 0,
    null,
    false,
    entry.Category,
    null,
    false,
    entry.PrerequisiteExpr,
    false
  );

  public static readonly Expression<Func<Language, OptionDescriptor>> Language = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    0,
    null,
    false,
    entry.Rare ? "rare" : "standard",
    null,
    false,
    null,
    false
  );

  public static readonly Expression<Func<Item, OptionDescriptor>> Item = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    0,
    null,
    false,
    entry.Kind.ToString().ToLowerInvariant(),
    null,
    false,
    null,
    false
  );

  public static readonly Expression<Func<Tool, OptionDescriptor>> Tool = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    0,
    null,
    false,
    entry.Group,
    null,
    false,
    null,
    false
  );

  public static readonly Expression<Func<Weapon, OptionDescriptor>> Weapon = entry => new OptionDescriptor
  (
    entry.Key,
    entry.Name,
    0,
    null,
    false,
    entry.Category.ToString().ToLowerInvariant(),
    null,
    false,
    null,
    false
  );

  #endregion

  #region Compiled

  public static readonly Func<Spell, OptionDescriptor> SpellOption = Spell.Compile();
  public static readonly Func<Feat, OptionDescriptor> FeatOption = Feat.Compile();
  public static readonly Func<Feature, OptionDescriptor> FeatureOption = Feature.Compile();
  public static readonly Func<Language, OptionDescriptor> LanguageOption = Language.Compile();
  public static readonly Func<Item, OptionDescriptor> ItemOption = Item.Compile();
  public static readonly Func<Tool, OptionDescriptor> ToolOption = Tool.Compile();
  public static readonly Func<Weapon, OptionDescriptor> WeaponOption = Weapon.Compile();

  #endregion

  #region Methods

  /// <summary>
  ///   Option sources whose options are read from a rule table (the others are part of the choice or fixed keys)
  /// </summary>
  public static bool FromTable(OptionSourceEnum source)
    => source is OptionSourceEnum.Tool or OptionSourceEnum.Language or OptionSourceEnum.Spell or OptionSourceEnum.Feat
      or OptionSourceEnum.FeatureCategory or OptionSourceEnum.Item or OptionSourceEnum.Weapon;

  /// <summary>
  ///   Choices with the same list key share their options (every spell choice offers every spell before filtering)
  /// </summary>
  public static string ListKey(Choice choice)
    => choice.OptionSource == OptionSourceEnum.FeatureCategory ? $"{choice.OptionSource}:{choice.Category}" : choice.OptionSource.ToString();

  #endregion
}
