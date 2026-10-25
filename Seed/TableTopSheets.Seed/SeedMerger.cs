using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Merges seed documents by key: values of the later document win when set, missing values are kept
/// </summary>
public static class SeedMerger
{
  #region Methods

  public static void Merge(SeedDocument target, SeedDocument source)
  {
    MergeList(target.Classes, source.Classes, MergeClass);
    MergeList(target.Species, source.Species, (into, from) =>
    {
      MergeNamed(into, from);
      into.CreatureType = from.CreatureType ?? into.CreatureType;
      MergeFeatures(into.Features, from.Features);
    });
    MergeList(target.Backgrounds, source.Backgrounds, (into, from) =>
    {
      MergeNamed(into, from);
      into.Abilities = from.Abilities ?? into.Abilities;
      into.OriginFeat = from.OriginFeat ?? into.OriginFeat;
      MergeFeatures(into.Features, from.Features);
    });
    MergeList(target.Feats, source.Feats, (into, from) =>
    {
      MergeNamed(into, from);
      into.Category = from.Category ?? into.Category;
      into.Repeatable = from.Repeatable ?? into.Repeatable;
      into.Prerequisite = from.Prerequisite ?? into.Prerequisite;
      MergeFeatures(into.Features, from.Features);
    });
    MergeFeatures(target.Features, source.Features);
    target.ClassFeatureTexts.AddRange(source.ClassFeatureTexts);
    MergeList(target.Spells, source.Spells, (into, from) =>
    {
      MergeNamed(into, from);
      into.Level = from.Level ?? into.Level;
      into.School = from.School ?? into.School;
      into.CastingTime = from.CastingTime ?? into.CastingTime;
      into.Ritual = from.Ritual ?? into.Ritual;
      into.Range = from.Range ?? into.Range;
      into.Components = from.Components ?? into.Components;
      into.Material = from.Material ?? into.Material;
      into.Concentration = from.Concentration ?? into.Concentration;
      into.Duration = from.Duration ?? into.Duration;
      into.Lists = from.Lists ?? into.Lists;
    });
    MergeList(target.Items, source.Items, MergeItem);
    MergeList(target.Masteries, source.Masteries, MergeNamed);
    MergeList(target.Languages, source.Languages, (into, from) =>
    {
      MergeNamed(into, from);
      into.Rare = from.Rare || into.Rare;
    });
    foreach ((string key, List<List<int>> rows) in source.SlotTables)
    {
      target.SlotTables[key] = rows;
    }
  }

  private static void MergeClass(SeedClass into, SeedClass from)
  {
    MergeNamed(into, from);
    into.HitDie = from.HitDie ?? into.HitDie;
    into.PrimaryAbilities = from.PrimaryAbilities ?? into.PrimaryAbilities;
    into.PrimaryAbilityMode = from.PrimaryAbilityMode ?? into.PrimaryAbilityMode;
    into.SubclassLevel = from.SubclassLevel ?? into.SubclassLevel;
    foreach ((string key, string value) in from.Scales)
    {
      into.Scales[key] = value;
    }

    MergeFeatures(into.Features, from.Features);
    MergeList(into.Subclasses, from.Subclasses, (subclassInto, subclassFrom) =>
    {
      MergeNamed(subclassInto, subclassFrom);
      foreach ((string key, string value) in subclassFrom.Scales)
      {
        subclassInto.Scales[key] = value;
      }

      MergeFeatures(subclassInto.Features, subclassFrom.Features);
    });
  }

  private static void MergeItem(SeedItem into, SeedItem from)
  {
    MergeNamed(into, from);
    into.Kind = from.Kind ?? into.Kind;
    into.Cost = from.Cost ?? into.Cost;
    into.Weight = from.Weight ?? into.Weight;
    into.Magic = from.Magic ?? into.Magic;
    into.Attunement = from.Attunement ?? into.Attunement;
    into.WeaponCategory = from.WeaponCategory ?? into.WeaponCategory;
    into.Ranged = from.Ranged ?? into.Ranged;
    into.Damage = from.Damage ?? into.Damage;
    into.DamageType = from.DamageType ?? into.DamageType;
    into.Properties = from.Properties ?? into.Properties;
    into.Versatile = from.Versatile ?? into.Versatile;
    into.RangeNormal = from.RangeNormal ?? into.RangeNormal;
    into.RangeLong = from.RangeLong ?? into.RangeLong;
    into.Mastery = from.Mastery ?? into.Mastery;
    into.ArmorCategory = from.ArmorCategory ?? into.ArmorCategory;
    into.BaseAc = from.BaseAc ?? into.BaseAc;
    into.DexCap = from.DexCap ?? into.DexCap;
    into.StrengthRequirement = from.StrengthRequirement ?? into.StrengthRequirement;
    into.StealthDisadvantage = from.StealthDisadvantage ?? into.StealthDisadvantage;
    into.ToolGroup = from.ToolGroup ?? into.ToolGroup;
    MergeFeatures(into.Features, from.Features);
  }

  /// <summary>
  ///   Features merge by key; mechanics (effects, choices, resources, spellcasting) of the later document replace earlier ones when present
  /// </summary>
  public static void MergeFeatures(List<SeedFeature> into, List<SeedFeature> from)
    => MergeList(into, from, (target, source) =>
    {
      MergeNamed(target, source);
      target.Level = source.Level ?? target.Level;
      target.GrantMode = source.GrantMode ?? target.GrantMode;
      target.OwnerType = source.OwnerType ?? target.OwnerType;
      target.Category = source.Category ?? target.Category;
      target.SheetSection = source.SheetSection ?? target.SheetSection;
      target.Prerequisite = source.Prerequisite ?? target.Prerequisite;
      target.Toggleable = source.Toggleable ?? target.Toggleable;
      target.Spell = source.Spell ?? target.Spell;
      target.Spellcasting = source.Spellcasting ?? target.Spellcasting;
      if (source.Effects.Count > 0)
      {
        target.Effects = source.Effects;
      }

      if (source.Choices.Count > 0)
      {
        target.Choices = source.Choices;
      }

      if (source.Resources.Count > 0)
      {
        target.Resources = source.Resources;
      }
    });

  private static void MergeNamed(SeedNamed into, SeedNamed from)
  {
    into.Name = from.Name ?? into.Name;
    into.Description = from.Description ?? into.Description;
  }

  private static void MergeList<T>(List<T> into, List<T> from, Action<T, T> merge) where T : SeedNamed
  {
    foreach (T item in from)
    {
      T? existing = into.FirstOrDefault(entry => entry.Key == item.Key);
      if (existing == null)
      {
        into.Add(item);
      }
      else
      {
        merge(existing, item);
      }
    }
  }

  #endregion
}
