using System.Globalization;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Maps rule entities to API contracts; related rows (subclasses, origin feat, mastery) have to be loaded with the entity
/// </summary>
public static class RuleMapper
{
  #region Summaries

  public static SpeciesSummaryDto Summary(Species species) => new(species.Key, species.Name, species.CreatureType);

  public static BackgroundSummaryDto Summary(Background background)
    => new(background.Key, background.Name, background.AbilityOptions, background.OriginFeat?.Key);

  public static ClassSummaryDto Summary(CharacterClass @class) => new
  (
    @class.Key,
    @class.Name,
    @class.HitDie,
    @class.PrimaryAbilities,
    @class.PrimaryAbilityMode,
    @class.SubclassLevel,
    Subclasses(@class)
  );

  public static IReadOnlyList<SubclassSummaryDto> Subclasses(CharacterClass @class)
    => @class.Subclasses
      .OrderBy(entry => entry.Name, StringComparer.Ordinal)
      .Select(entry => new SubclassSummaryDto(entry.Key, entry.Name))
      .ToList();

  public static FeatSummaryDto Summary(Feat feat)
    => new(feat.Key, feat.Name, ChoiceService.FeatCategoryKey(feat.Category), feat.Repeatable, feat.PrerequisiteExpr != null);

  public static SpellSummaryDto Summary(Spell spell) => new
  (
    spell.Key,
    spell.Name,
    spell.Level,
    SchoolKey(spell),
    spell.CastingTime,
    spell.Range,
    spell.Ritual,
    spell.Concentration,
    spell.Damage,
    spell.DamageType,
    spell.Lists
  );

  public static ItemSummaryDto Summary(Item item)
    => new(item.Key, item.Name, item.Kind, ItemCategory(item), item.CostCp, item.Weight, item.Magic, item.RequiresAttunement, item.Bundle);

  public static string SchoolKey(Spell spell) => spell.School
    .ToString()
    .ToLowerInvariant();

  public static string? ItemCategory(Item item) => item switch
  {
    Weapon weapon => weapon.Category
      .ToString()
      .ToLowerInvariant(),
    Armor armor => armor.Category
      .ToString()
      .ToLowerInvariant(),
    _ => item.Group,
  };

  #endregion

  #region Details

  public static FeatureDto Feature(Feature feature) => new
  (
    feature.Key,
    feature.Name,
    feature.Level,
    HtmlText.ToPlain(feature.DescriptionHtml),
    feature.Choices
      .Select(Choice)
      .ToList()
  );

  public static ChoiceDefinitionDto Choice(Choice choice) => new
  (
    choice.Key,
    choice.Name,
    choice.CountExpr,
    choice.OptionSource,
    choice.Optional,
    choice.AllowDuplicates,
    choice.MaxPerOption,
    choice.ReplacePolicy
  );

  /// <param name="entity">species, background, feat or subclass</param>
  /// <param name="features">its features</param>
  /// <param name="startingEquipment">starting equipment of a background</param>
  public static RuleDetailDto Detail(RuleEntity entity, IEnumerable<Feature> features, IReadOnlyList<StartingEquipmentOptionDto>? startingEquipment = null)
    => new(entity.Key, entity.Name, HtmlText.ToPlain(entity.DescriptionHtml), Features(features), startingEquipment ?? []);

  /// <param name="class">class with its subclasses</param>
  /// <param name="features">features of the class and of its subclasses</param>
  /// <param name="startingEquipment">starting equipment of the class</param>
  public static ClassDetailDto Detail(CharacterClass @class, IReadOnlyList<Feature> features, IReadOnlyList<StartingEquipmentOptionDto> startingEquipment) => new
  (
    Summary(@class),
    HtmlText.ToPlain(@class.DescriptionHtml),
    Features(features.Where(entry => entry.ClassId == @class.Id && entry.SubclassId == null)),
    @class.Subclasses
      .OrderBy(entry => entry.Name, StringComparer.Ordinal)
      .Select(entry => Detail(entry, features.Where(feature => feature.SubclassId == entry.Id)))
      .ToList(),
    startingEquipment
  );

  /// <summary>
  ///   Items named by starting equipment (fixed items and picks), to load their names
  /// </summary>
  public static IReadOnlyList<string> ItemKeys(IEnumerable<StartingEquipmentOption> options)
    => options
      .SelectMany(option => option.Items)
      .SelectMany(entry => entry.Options.Append(entry.ItemKey))
      .OfType<string>()
      .Distinct()
      .ToList();

  /// <param name="options">alternatives of a background or class</param>
  /// <param name="names">item names by key (see <see cref="ItemKeys"/>)</param>
  /// <param name="sameAs">item already selected per "same as above" choice; such picks show that item</param>
  public static IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
  (
    IEnumerable<StartingEquipmentOption> options,
    IReadOnlyDictionary<string, string> names,
    IReadOnlyDictionary<string, string>? sameAs = null
  )
    => options
      .Select(option => new StartingEquipmentOptionDto
      (
        option.Key,
        option.Items
          .Select(entry =>
          {
            string? chosen = entry.SameAs != null && sameAs?.GetValueOrDefault(entry.SameAs) is { } same && entry.Options.Contains(same) ? same : null;
            string? itemKey = entry.ItemKey ?? chosen;
            return new StartingEquipmentItemDto
            (
              entry.Key,
              itemKey != null && entry.ItemKey != null ? names.GetValueOrDefault(itemKey, itemKey) : entry.Name ?? entry.Key,
              itemKey,
              entry.Quantity,
              entry.Note,
              entry.Options
                .Select(key => new ItemOptionDto(key, names.GetValueOrDefault(key, key)))
                .ToList(),
              entry.SameAs
            );
          })
          .ToList(),
        option.CoinsCp
      ))
      .ToList();

  public static SpellDetailDto Detail(Spell spell) => new
  (
    Summary(spell),
    spell.Components,
    spell.Material,
    spell.Duration,
    spell.AttackRoll,
    spell.SaveAbility,
    HtmlText.ToPlain(spell.DescriptionHtml)
  );

  /// <param name="item">item (with the mastery of a weapon)</param>
  /// <param name="features">features of the item</param>
  public static ItemDetailDto Detail(Item item, IEnumerable<Feature> features) => new
  (
    Summary(item),
    HtmlText.ToPlain(item.DescriptionHtml),
    Properties(item),
    Features(features)
  );

  /// <summary>
  ///   Features ordered by level and authored order
  /// </summary>
  public static IReadOnlyList<FeatureDto> Features(IEnumerable<Feature> features)
    => features
      .OrderBy(entry => entry.Level ?? 0)
      .ThenBy(entry => entry.Order)
      .Select(Feature)
      .ToList();

  private static Dictionary<string, string> Properties(Item item)
  {
    Dictionary<string, string> properties = new();
    switch (item)
    {
      case Weapon weapon:
        properties["damage"] = weapon.Damage;
        properties["damageType"] = weapon.DamageType.ToString();
        properties["properties"] = weapon.Properties.ToString();
        properties["ranged"] = weapon.Ranged.ToString(CultureInfo.InvariantCulture);
        if (weapon.VersatileDamage != null)
        {
          properties["versatileDamage"] = weapon.VersatileDamage;
        }

        if (weapon.RangeNormal != null)
        {
          properties["range"] = $"{weapon.RangeNormal}/{weapon.RangeLong}";
        }

        if (weapon.Mastery is { } mastery)
        {
          properties["mastery"] = mastery.Name;
        }

        break;
      case Armor armor:
        properties["baseAc"] = armor.BaseAc.ToString(CultureInfo.InvariantCulture);
        if (armor.DexCap != null)
        {
          properties["dexCap"] = armor.DexCap.Value.ToString(CultureInfo.InvariantCulture);
        }

        if (armor.StrengthRequirement != null)
        {
          properties["strengthRequirement"] = armor.StrengthRequirement.Value.ToString(CultureInfo.InvariantCulture);
        }

        properties["stealthDisadvantage"] = armor.StealthDisadvantage.ToString(CultureInfo.InvariantCulture);
        break;
    }

    return properties;
  }

  #endregion
}
