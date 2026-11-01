using System.Globalization;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Content of the printable cards of spells and items; the mastery of a weapon has to be loaded with it
/// </summary>
public static class CardMapper
{
  #region Fields

  private static readonly Dictionary<string, string> Groups = new()
  {
    ["wondrous-item"] = "Wondrous Item",
    ["potion"] = "Potion",
    ["ring"] = "Ring",
    ["rod"] = "Rod",
    ["scroll"] = "Scroll",
    ["staff"] = "Staff",
    ["wand"] = "Wand",
    ["ammunition"] = "Ammunition",
    ["arcane-focus"] = "Arcane Focus",
    ["druidic-focus"] = "Druidic Focus",
    ["holy-symbol"] = "Holy Symbol",
    ["food-and-drink"] = "Food and Drink",
  };

  #endregion

  #region Methods

  /// <param name="spell">the spell</param>
  /// <param name="image">its picture as JPEG, null when there is none</param>
  public static CardData Card(Spell spell, byte[]? image)
  {
    string school = spell.School.ToString();
    List<CardFact> facts =
    [
      new("Casting Time", spell.Ritual ? $"{spell.CastingTime} or Ritual" : spell.CastingTime),
      new("Range", spell.Range),
      new("Components", spell.Material == null ? spell.Components : $"{spell.Components} *"),
      new("Duration", spell.Duration),
    ];
    if (spell.SaveAbility != null)
    {
      facts.Add(new("Save", Naming.Title(spell.SaveAbility)));
    }
    else if (spell.AttackRoll)
    {
      facts.Add(new("Attack", "Spell attack"));
    }

    if (spell.Damage != null)
    {
      facts.Add(new("Damage", spell.DamageType == null ? spell.Damage : $"{spell.Damage} {spell.DamageType}"));
    }

    // the material can be long; it goes below the text, marked like the components
    List<TextBlock> description = HtmlText
      .ToBlocks(spell.DescriptionHtml)
      .ToList();
    if (spell.Material != null)
    {
      description.Add(new TextBlock(TextBlockKindEnum.Paragraph)
      {
        Spans = [new TextSpan("* Material: ", Bold: true), new TextSpan(spell.Material, Italic: true)],
      });
    }

    return new CardData
    {
      Kind = CardKindEnum.Spell,
      Name = spell.Name,
      Subtitle = spell.Level == 0 ? $"{school} Cantrip" : $"Level {spell.Level} {school}",
      Facts = facts,
      Description = description,
      Image = image,
      Footer = spell.Lists.Count == 0
        ? "Spell"
        : $"Spell · {string.Join(", ", spell.Lists.Select(Naming.Title))}",
    };
  }

  /// <param name="item">the item (with the mastery of a weapon)</param>
  /// <param name="image">its picture as JPEG, null when there is none</param>
  public static CardData Card(Item item, byte[]? image)
  {
    string type = Type(item);
    List<CardFact> facts = [new("Type", type)];
    if (item.Rarity is { } rarity)
    {
      facts.Add(new("Rarity", Rarity(rarity)));
    }

    if (item.RequiresAttunement)
    {
      facts.Add(new("Attunement", "Required"));
    }

    switch (item)
    {
      case Weapon weapon:
        facts.Add(new("Damage", weapon.VersatileDamage == null
          ? $"{weapon.Damage} {weapon.DamageType}"
          : $"{weapon.Damage} {weapon.DamageType} ({weapon.VersatileDamage} two-handed)"));
        string properties = Properties(weapon.Properties);
        if (properties.Length > 0)
        {
          facts.Add(new("Properties", properties));
        }

        if (weapon.RangeNormal != null)
        {
          facts.Add(new("Range", $"{weapon.RangeNormal}/{weapon.RangeLong} ft."));
        }

        if (weapon.Mastery is { } mastery)
        {
          facts.Add(new("Mastery", mastery.Name));
        }

        break;
      case Armor armor:
        facts.Add(new("Armor Class", armor.Category == ArmorEnum.Shield
          ? $"+{armor.BaseAc}"
          : armor.DexCap switch
          {
            0 => armor.BaseAc.ToString(CultureInfo.InvariantCulture),
            null => $"{armor.BaseAc} + Dex",
            { } cap => $"{armor.BaseAc} + Dex (max {cap})",
          }));
        if (armor.StrengthRequirement is > 0)
        {
          facts.Add(new("Strength", armor.StrengthRequirement.Value.ToString(CultureInfo.InvariantCulture)));
        }

        if (armor.StealthDisadvantage)
        {
          facts.Add(new("Stealth", "Disadvantage"));
        }

        break;
    }

    facts.Add(new("Cost", item.Rarity == RarityEnum.Artifact ? "Priceless" : Cost(item.CostCp, item.Bundle)));
    if (item.Weight > 0)
    {
      facts.Add(new("Weight", $"{item.Weight.ToString("0.##", CultureInfo.InvariantCulture)} lb."));
    }

    return new CardData
    {
      Kind = CardKindEnum.Item,
      Name = item.Name,
      Subtitle = item.Rarity is { } subtitleRarity ? $"{type}, {Rarity(subtitleRarity)}" : type,
      Facts = facts,
      Description = HtmlText.ToBlocks(item.DescriptionHtml),
      Image = image,
      Footer = item.Magic ? "Magic Item" : "Item",
    };
  }

  /// <summary>
  ///   <c>Martial Weapon</c>, <c>Heavy Armor</c>, <c>Shield</c>, <c>Wondrous Item</c>, <c>Tool</c>, <c>Gear</c>
  /// </summary>
  private static string Type(Item item) => item switch
  {
    Weapon weapon when item.Group == "staff" => $"Staff ({weapon.Category} Weapon)",
    Weapon weapon => $"{weapon.Category} {(weapon.Ranged ? "Ranged" : "Melee")} Weapon",
    Armor { Category: ArmorEnum.Shield } => "Shield",
    Armor armor => $"{armor.Category} Armor",
    Tool => "Tool",
    _ => item.Group != null && Groups.TryGetValue(item.Group, out string? group) ? group : "Gear",
  };

  private static string Rarity(RarityEnum rarity) => rarity == RarityEnum.VeryRare ? "Very Rare" : rarity.ToString();

  private static string Properties(WeaponPropertyEnum properties) => string.Join(", ", System.Enum
    .GetValues<WeaponPropertyEnum>()
    .Where(flag => flag is not (WeaponPropertyEnum.None or WeaponPropertyEnum.Melee or WeaponPropertyEnum.Range) && properties.HasFlag(flag))
    .Select(flag => flag == WeaponPropertyEnum.TwoHanded ? "Two-Handed" : flag.ToString()));

  /// <summary>
  ///   <c>15 gp</c>, <c>5 sp</c>, <c>1 gp for 20</c>; <c>—</c> without a price
  /// </summary>
  private static string Cost(int copper, int bundle)
  {
    string price = copper switch
    {
      0 => "—",
      _ when copper % 100 == 0 => $"{copper / 100:N0} gp",
      _ when copper % 10 == 0 => $"{copper / 10:N0} sp",
      _ => $"{copper:N0} cp",
    };
    return bundle > 1 && copper > 0 ? $"{price} for {bundle}" : price;
  }

  #endregion
}
