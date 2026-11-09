using System.Text.Json;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Turns item templates (<c>+1 Weapon</c>, <c>Ioun Stone</c>) into their concrete variants (<c>+1 Longsword</c>,
///   <c>Ioun Stone, Agility</c>). A variant starts as a copy of its base item, takes the template's values and features and
///   finally its own; the description is the template's followed by the variant's. A base item may itself be made from a
///   mundane item (a staff focus is a Quarterstaff): the variant copies the base item but names the mundane one as its base,
///   so proficiency and mastery follow it
/// </summary>
public static class SeedVariantExpander
{
  #region Methods

  /// <summary>
  ///   Items with every template replaced by its variants and the values of their base items filled in; errors name variants
  ///   whose base item doesn't exist. An item with the key of a variant (a Spell Scroll of the Player's Handbook) is merged into
  ///   the variant
  /// </summary>
  public static List<SeedItem> Expand(List<SeedItem> items, List<string> errors)
  {
    Dictionary<string, SeedItem> overrides = items
      .SelectMany(entry => entry.Variants)
      .Select(entry => entry.Key)
      .Distinct()
      .Join(items, key => key, entry => entry.Key, (_, entry) => entry)
      .Where(entry => entry.Variants.Count == 0)
      .ToDictionary(entry => entry.Key);
    Dictionary<string, SeedItem> bases = items
      .Where(entry => entry.Variants.Count == 0)
      .ToDictionary(entry => entry.Key);
    List<SeedItem> expanded = new();
    foreach (SeedItem item in items.Where(entry => !overrides.ContainsKey(entry.Key)))
    {
      // an item made from one base item (Sun Blade) is its own only variant
      List<SeedItem> variants = item.Variants.Count > 0 || item.Base == null ? item.Variants : [new SeedItem { Key = item.Key }];
      if (variants.Count == 0)
      {
        expanded.Add(item);
        continue;
      }

      foreach (SeedItem variant in variants)
      {
        string? baseKey = variant.Base ?? item.Base;
        SeedItem? baseItem = null;
        if (baseKey != null && !bases.TryGetValue(baseKey, out baseItem))
        {
          errors.Add($"Item '{variant.Key}': base item '{baseKey}' doesn't exist");
          continue;
        }

        // a base item made from a mundane item hands that one on as the base (Silvered Staff: staff focus → Quarterstaff)
        string? rootKey = baseItem?.Base ?? baseItem?.Key;
        if (baseItem?.Base != null && (!bases.TryGetValue(baseItem.Base, out SeedItem? root) || root.Base != null))
        {
          errors.Add($"Item '{variant.Key}': base item '{baseItem.Key}' must be made from a mundane item without a base");
          continue;
        }

        SeedItem result = Variant(item, variant, baseItem, rootKey);
        if (overrides.TryGetValue(result.Key, out SeedItem? existing))
        {
          SeedMerger.MergeItem(result, Clone(existing));
        }

        expanded.Add(result);
      }
    }

    return expanded;
  }

  /// <param name="rootKey">key of the mundane item the variant is made from, the base item's own base when it has one</param>
  private static SeedItem Variant(SeedItem template, SeedItem variant, SeedItem? baseItem, string? rootKey)
  {
    SeedItem result = new() { Key = variant.Key };
    int baseCost = 0;
    if (baseItem != null)
    {
      SeedItem mundane = Clone(baseItem);
      int bundle = mundane.Bundle ?? 1;
      baseCost = (mundane.Cost ?? 0) / bundle;
      mundane.Name = null;
      mundane.Description = null;
      mundane.Cost = null;
      mundane.Weight = mundane.Weight / bundle;
      mundane.Bundle = null;
      SeedMerger.MergeItem(result, mundane);
    }

    SeedItem shared = Clone(template);
    shared.Key = variant.Key;
    shared.Variants = [];
    shared.Cost = null;
    SeedMerger.MergeItem(result, shared);
    SeedMerger.MergeItem(result, Clone(variant));

    result.Base = rootKey;
    result.Description = string.Concat(template.Description, variant.Description);
    result.Cost = variant.Cost ?? (template.Variants.Count == 0 ? template.Cost : null) ?? baseCost + (MagicItemPrice.Of(result.Rarity, baseItem?.Category ?? result.Category) ?? 0);
    // keys (and so the ids) of features and their choices are made unique per variant
    foreach (SeedFeature feature in result.Features)
    {
      feature.Key = VariantKey(feature.Key, template.Key, variant.Key);
      foreach (SeedChoice choice in feature.Choices)
      {
        choice.Key = VariantKey(choice.Key, template.Key, variant.Key);
      }
    }

    return result;
  }

  /// <summary>
  ///   <c>plus1-weapon.bonus</c> → <c>plus1-longsword.bonus</c>; keys of the variant itself stay
  /// </summary>
  private static string VariantKey(string key, string templateKey, string variantKey)
    => key.StartsWith($"{variantKey}.", StringComparison.Ordinal)
      ? key
      : key.StartsWith($"{templateKey}.", StringComparison.Ordinal)
        ? $"{variantKey}.{key[(templateKey.Length + 1)..]}"
        : $"{variantKey}.{key}";

  private static SeedItem Clone(SeedItem item)
    => JsonSerializer.Deserialize<SeedItem>(JsonSerializer.Serialize(item, SeedJson.Options), SeedJson.Options)!;

  #endregion
}
