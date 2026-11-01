using System.Globalization;
using System.Text.RegularExpressions;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Seed;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Tools.Extractor;

/// <summary>
///   Reads the magic items of the Dungeon Master's Guide markdown (Magic Items A-Z). A supplement in the same format fills
///   what the markdown lacks: an entry with the heading of a main entry replaces it, any other entry is a variant
///   (<c>Ioun Stone, Agility</c>, <c>Belt of Hill Giant Strength</c>) or an item of its own
/// </summary>
internal sealed partial class MagicItemParser
{
  #region Fields

  /// <summary>
  ///   Base item names of the guide that are named differently in the seed
  /// </summary>
  private static readonly Dictionary<string, string> BaseKeys = new()
  {
    ["Arrow"] = "arrows",
    ["Bolt"] = "bolts",
    ["Firearm Bullet"] = "bullets-firearm",
    ["Needle"] = "needles",
    ["Sling Bullet"] = "bullets-sling",
    ["Staff"] = "arcane-focus-staff",
    ["Wooden Staff"] = "druidic-focus-wooden-staff",
  };

  private static readonly Dictionary<string, string> Categories = new()
  {
    ["Wondrous Item"] = "wondrous-item",
    ["Potion"] = "potion",
    ["Ring"] = "ring",
    ["Rod"] = "rod",
    ["Staff"] = "staff",
    ["Wand"] = "wand",
    ["Scroll"] = "scroll",
    ["Weapon"] = "weapon",
    ["Shield"] = "armor",
    ["Light Armor"] = "armor",
    ["Medium Armor"] = "armor",
    ["Heavy Armor"] = "armor",
  };

  /// <summary>
  ///   Rules of weapon properties and masteries the markdown repeats in the entries of magic weapons
  /// </summary>
  private static readonly HashSet<string> WeaponRules =
  [
    "Ammunition", "Finesse", "Heavy", "Light", "Loading", "Range", "Reach", "Thrown", "Two-Handed", "Two‑Handed", "Versatile",
    "Cleave", "Graze", "Nick", "Push", "Sap", "Slow", "Topple", "Vex",
  ];

  /// <summary>
  ///   Generic variants the guide gives no base items: they apply to the base items of another one, named by the pattern
  /// </summary>
  private static readonly Dictionary<string, (string Other, string Name)> BasesOf = new()
  {
    ["Armor of Resistance"] = ("+1 Armor", "{0} of Resistance"),
    ["Enspelled Armor"] = ("+1 Armor", "Enspelled {0}"),
    ["Enspelled Weapon"] = ("+1 Weapon", "Enspelled {0}"),
  };

  /// <summary>
  ///   Generic variants whose variations (by damage type) each apply to the base items of another one: every variation and
  ///   base item is an item, named by the pattern (<c>{0}</c> the base item, <c>{1}</c> the variation in parentheses)
  /// </summary>
  private static readonly Dictionary<string, (string Other, string Name)> VariationsTimesBases = new()
  {
    ["Armor of Vulnerability"] = ("+1 Armor", "{0} of Vulnerability ({1})"),
  };

  /// <summary>
  ///   Items whose variations (by spell level) only differ by a row of their table, so they stay one item
  /// </summary>
  private static readonly HashSet<string> TableOnly = ["Enspelled Armor", "Enspelled Staff", "Enspelled Weapon"];

  /// <summary>
  ///   Items whose stronger kinds are items of their own; the item itself stays (Potion of Healing of the Player's Handbook)
  /// </summary>
  private static readonly HashSet<string> Standalone = ["Potion of Healing"];

  private readonly Dictionary<string, Entry> _entries;
  private readonly Dictionary<string, Entry> _supplement;
  private readonly HashSet<string> _baseKeys;

  #endregion

  #region Constructor

  /// <param name="guide">lines of the Dungeon Master's Guide markdown</param>
  /// <param name="supplement">lines of the supplement (may be empty)</param>
  /// <param name="baseKeys">keys of the mundane items of the seed (base items of the variants)</param>
  public MagicItemParser(string[] guide, string[] supplement, IEnumerable<string> baseKeys)
  {
    int start = Array.FindIndex(guide, line => line.TrimEnd() == "## Magic Items A-Z");
    int end = Array.FindIndex(guide, start + 1, line => line.TrimEnd() == "# Random Magic Items");
    if (start < 0 || end < 0)
    {
      throw new InvalidOperationException("Section 'Magic Items A-Z' not found");
    }

    this._entries = Entries(guide[start..end]).ToDictionary(entry => entry.Name, StringComparer.OrdinalIgnoreCase);
    this._supplement = Entries(supplement).ToDictionary(entry => entry.Name, StringComparer.OrdinalIgnoreCase);
    this._baseKeys = baseKeys.ToHashSet();
  }

  #endregion

  #region Properties

  /// <summary>
  ///   Problems found while reading: unknown base items, variants without text, supplement entries without a template
  /// </summary>
  public List<string> Warnings { get; } = new();

  #endregion

  #region Methods

  public List<SeedItem> Items()
  {
    // a corrected entry keeps the base items and variations of the guide unless it lists them itself
    foreach ((string name, Entry entry) in this._supplement.Where(pair => this._entries.ContainsKey(pair.Key)))
    {
      Entry main = this._entries[name];
      this._entries[name] = entry with
      {
        Bases = entry.Bases.Count > 0 ? entry.Bases : main.Bases,
        Variations = entry.Variations.Count > 0 ? entry.Variations : main.Variations,
      };
    }

    foreach ((string name, (string other, string pattern)) in BasesOf.Where(pair => this._entries.ContainsKey(pair.Key) && this._entries.ContainsKey(pair.Value.Other)))
    {
      this._entries[name] = this._entries[name] with
      {
        Bases = this._entries[other].Bases
          .Select(entry => (entry.Base, string.Format(CultureInfo.InvariantCulture, pattern, entry.Base)))
          .ToList(),
      };
    }

    HashSet<string> tableRows = new(StringComparer.OrdinalIgnoreCase);
    foreach (string name in TableOnly.Where(this._entries.ContainsKey))
    {
      tableRows.UnionWith(this._entries[name].Variations);
      this._entries[name] = this._entries[name] with { Variations = [] };
    }

    HashSet<string> listed = this._entries.Values
      .SelectMany(entry => entry.Variations)
      .ToHashSet(StringComparer.OrdinalIgnoreCase);
    List<Entry> loose = this._supplement.Values
      .Where(entry => !this._entries.ContainsKey(entry.Name) && !listed.Contains(entry.Name) && !tableRows.Contains(entry.Name))
      .ToList();

    List<SeedItem> items = new();
    foreach (Entry entry in this._entries.Values)
    {
      SeedItem item = this.Item(entry);
      items.Add(item);

      // the rarity of these varies with the spell level of their table; the lowest one sets the price
      if (TableOnly.Contains(entry.Name))
      {
        item.Rarity ??= RarityEnum.Uncommon;
      }

      List<Entry> tableVariants = loose
        .Where(candidate => IsTableVariant(entry.Name, candidate.Name))
        .ToList();
      loose.RemoveAll(tableVariants.Contains);

      // a template with a rarity of its own (Potion of Healing) stays an item; its stronger kinds are items of their own
      if (Standalone.Contains(entry.Name) || (item.Rarity != null && entry.Variations.Count == 0 && entry.Bases.Count == 0))
      {
        items.AddRange(entry.Variations
          .Where(this._supplement.ContainsKey)
          .Select(name => this.Item(this._supplement[name]))
          .Concat(tableVariants.Select(this.Item)));
        continue;
      }

      this.AddVariants(item, entry, tableVariants);
    }

    foreach (Entry entry in loose)
    {
      this.Warnings.Add($"supplement entry '{entry.Name}' belongs to no item; it is an item of its own");
      items.Add(this.Item(entry));
    }

    return items
      .OrderBy(item => item.Name, StringComparer.Ordinal)
      .ToList();
  }

  private void AddVariants(SeedItem item, Entry entry, List<Entry> tableVariants)
  {
    foreach ((string baseName, string variantName) in entry.Bases)
    {
      string baseKey = BaseKeys.GetValueOrDefault(baseName) ?? SeedKeys.Slug(baseName);
      if (!this._baseKeys.Contains(baseKey))
      {
        this.Warnings.Add($"{entry.Name}: base item '{baseName}' is not in the seed, '{variantName}' left out");
        continue;
      }

      item.Variants.Add(new SeedItem { Key = Key(variantName), Name = variantName, Base = baseKey });
    }

    // variations of a generic variant (Enspelled Armor by spell level) only differ by its table; the base items are the items
    if (entry.Bases.Count > 0)
    {
      return;
    }

    if (VariationsTimesBases.TryGetValue(entry.Name, out (string Other, string Name) times) && this._entries.TryGetValue(times.Other, out Entry? other))
    {
      this.AddVariationsTimesBases(item, entry, other, times.Name);
      return;
    }

    List<Entry> variants = entry.Variations
      .Select(name =>
      {
        if (this._supplement.TryGetValue(name, out Entry? variant))
        {
          return variant;
        }

        this.Warnings.Add($"{entry.Name}: variant '{name}' has no text in the supplement");
        return new Entry(name, entry.TypeLine, [], [], [], []);
      })
      .Concat(tableVariants)
      .ToList();
    if (variants.Count == 0)
    {
      return;
    }

    // every variant carries its own type line and text, so the shared text is part of each variant's description
    string shared = Html(entry.Body);
    foreach (Entry variant in variants)
    {
      SeedItem seed = this.Item(variant);
      seed.Description = Html([TypeParagraph(variant.TypeLine)]) + shared + Html(variant.Body);
      item.Variants.Add(seed);
    }

    item.Description = null;
  }

  /// <summary>
  ///   One item per variation and base item (<c>Plate Armor of Vulnerability (Bludgeoning)</c>)
  /// </summary>
  private void AddVariationsTimesBases(SeedItem item, Entry entry, Entry other, string pattern)
  {
    string shared = Html(entry.Body);
    foreach (string name in entry.Variations)
    {
      if (!this._supplement.TryGetValue(name, out Entry? variation))
      {
        this.Warnings.Add($"{entry.Name}: variant '{name}' has no text in the supplement");
        continue;
      }

      Match kind = VariationRegex().Match(name);
      foreach ((string baseName, _) in other.Bases)
      {
        string baseKey = BaseKeys.GetValueOrDefault(baseName) ?? SeedKeys.Slug(baseName);
        if (!this._baseKeys.Contains(baseKey))
        {
          continue;
        }

        string variantName = string.Format(CultureInfo.InvariantCulture, pattern, baseName, kind.Success ? kind.Groups[1].Value : name);
        SeedItem seed = this.Item(variation);
        seed.Key = Key(variantName);
        seed.Name = variantName;
        seed.Base = baseKey;
        seed.Description = Html([TypeParagraph(variation.TypeLine)]) + shared + Html(variation.Body);
        item.Variants.Add(seed);
      }
    }

    item.Description = null;
  }

  private SeedItem Item(Entry entry)
  {
    string type = Markdown.Plain(entry.TypeLine);
    string first = entry.TypeLine
      .Split(',')[0]
      .Trim();
    string kind = ParenthesesRegex()
      .Replace(first, string.Empty)
      .Trim();
    Match weight = WeightRegex().Match(type);
    Match cost = CostRegex().Match(type);
    Match baseItem = BaseTypeRegex().Match(first);

    SeedItem item = new()
    {
      Key = Key(entry.Name),
      Name = entry.Name,
      Magic = true,
      Rarity = Rarity(type),
      Attunement = type.Contains("Requires Attunement", StringComparison.OrdinalIgnoreCase) ? true : null,
      Category = Categories.GetValueOrDefault(kind),
      Weight = weight.Success ? double.Parse(weight.Groups[1].Value.Replace("½", "0.5"), CultureInfo.InvariantCulture) : null,
      Cost = cost.Success ? int.Parse(cost.Groups[1].Value.Replace(",", string.Empty), CultureInfo.InvariantCulture) * 100 : null,
      Description = Html(entry.Body.Prepend(string.Empty).Prepend(TypeParagraph(type))),
    };

    // a magic staff is a Quarterstaff; armor, shields and weapons name their base item in the type line
    string? baseName = baseItem.Success ? baseItem.Groups[1].Value : kind == "Staff" && type.Contains("Weapon", StringComparison.Ordinal) ? "Quarterstaff" : null;
    if (baseName != null)
    {
      string key = BaseKeys.GetValueOrDefault(baseName) ?? SeedKeys.Slug(baseName);
      if (this._baseKeys.Contains(key))
      {
        item.Base = key;
      }
      else
      {
        this.Warnings.Add($"{entry.Name}: base item '{baseName}' is not in the seed");
      }
    }

    return item;
  }

  /// <summary>
  ///   Variant of a template that the guide only names in a table: <c>Horn of Valhalla, Silver</c>,
  ///   <c>Potion of Healing (Greater)</c>, <c>Belt of Hill Giant Strength</c> (of Belt of Giant Strength)
  /// </summary>
  private static bool IsTableVariant(string template, string name)
  {
    if (name.StartsWith($"{template}, ", StringComparison.OrdinalIgnoreCase) || name.StartsWith($"{template} (", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    string[] words = template.Split(' ');
    return words.Length >= 3
           && words[^2] == "Giant"
           && name.StartsWith($"{string.Join(' ', words[..^2])} ", StringComparison.Ordinal)
           && name.EndsWith($" Giant {words[^1]}", StringComparison.Ordinal);
  }

  /// <summary>
  ///   Key of an item name; a leading bonus becomes a word (<c>+1 Longsword</c> → <c>plus1-longsword</c>)
  /// </summary>
  private static string Key(string name) => SeedKeys.Slug(BonusRegex().Replace(name, "plus$1 "));

  private static RarityEnum? Rarity(string type)
  {
    Match match = RarityRegex().Match(type);
    return match.Success ? Enum.Parse<RarityEnum>(match.Value.Replace(" ", string.Empty)) : null;
  }

  private static string TypeParagraph(string type) => $"*{Markdown.Plain(type)}*";

  private static string Html(IEnumerable<string> lines) => Markdown.ToHtml(lines);

  /// <summary>
  ///   Entries of a markdown part, split at <c>####</c> headings; type line, base items and variations are taken out of the text
  /// </summary>
  private static IEnumerable<Entry> Entries(string[] lines)
  {
    string? name = null;
    List<string> block = new();
    foreach (string line in lines.Append("#### "))
    {
      if (!line.StartsWith("#### ", StringComparison.Ordinal) && !(line.StartsWith("### ", StringComparison.Ordinal) && name != null))
      {
        block.Add(line);
        continue;
      }

      if (name != null)
      {
        yield return Parse(name, block);
      }

      name = line.StartsWith("#### ", StringComparison.Ordinal) && line.Length > 5 ? line[5..].Trim() : null;
      block = new();
    }
  }

  private static Entry Parse(string name, List<string> block)
  {
    List<string> content = block
      .SkipWhile(string.IsNullOrWhiteSpace)
      .ToList();
    string type = content.Count > 0 ? content[0].Trim() : string.Empty;
    int separator = content.IndexOf("---");
    List<string> stats = separator > 0
      ? content[1..separator]
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToList()
      : [];
    List<string> body = separator >= 0 ? content[(separator + 1)..] : content.Skip(1).ToList();

    List<(string, string)> bases = new();
    List<string> variations = new();
    List<string> text = new();
    string? list = null;
    foreach (string line in body)
    {
      if (line.StartsWith("***Base items.***", StringComparison.Ordinal))
      {
        list = "bases";
        continue;
      }

      if (line.StartsWith("Multiple variations of this item exist", StringComparison.Ordinal))
      {
        list = "variations";
        continue;
      }

      if (list != null && (string.IsNullOrWhiteSpace(line) || line.StartsWith("- ", StringComparison.Ordinal)))
      {
        Match pair = BaseItemRegex().Match(line);
        if (list == "bases" && pair.Success)
        {
          bases.Add((pair.Groups[1].Value, pair.Groups[2].Value));
        }
        else if (list == "variations" && line.StartsWith("- ", StringComparison.Ordinal))
        {
          variations.Add(Markdown.Plain(line[2..]));
        }

        continue;
      }

      list = null;
      Match rule = RuleParagraphRegex().Match(line);
      if (rule.Success && WeaponRules.Contains(rule.Groups[1].Value))
      {
        continue;
      }

      text.Add(line);
    }

    return new Entry(name, type, stats, TrimBlank(text), bases, variations);
  }

  private static List<string> TrimBlank(List<string> lines)
    => lines
      .SkipWhile(string.IsNullOrWhiteSpace)
      .Reverse()
      .SkipWhile(string.IsNullOrWhiteSpace)
      .Reverse()
      .ToList();

  [GeneratedRegex(@"\bVery Rare\b|\bUncommon\b|\bCommon\b|\bRare\b|\bLegendary\b|\bArtifact\b")]
  private static partial Regex RarityRegex();

  [GeneratedRegex(@"([\d.½]+) lb\.")]
  private static partial Regex WeightRegex();

  [GeneratedRegex(@"([\d,]+) GP\b")]
  private static partial Regex CostRegex();

  [GeneratedRegex(@"\(\*([^*]+)\*\)")]
  private static partial Regex BaseTypeRegex();

  [GeneratedRegex(@"\s*\(.*$")]
  private static partial Regex ParenthesesRegex();

  [GeneratedRegex(@"^- \*([^*]+)\* \(\*([^*]+)\*\)")]
  private static partial Regex BaseItemRegex();

  [GeneratedRegex(@"^\*\*\*([^.*]+)\.\*\*\*")]
  private static partial Regex RuleParagraphRegex();

  [GeneratedRegex(@"\(([^)]+)\)\s*$")]
  private static partial Regex VariationRegex();

  [GeneratedRegex(@"^\+(\d) ")]
  private static partial Regex BonusRegex();

  #endregion

  #region Nested Types

  /// <param name="Name">heading</param>
  /// <param name="TypeLine">category, rarity, attunement and weight (<c>Wondrous Item, Rare (Requires Attunement), 1 lb.</c>)</param>
  /// <param name="Stats">lines between the type line and the text (weapon damage, armor class)</param>
  /// <param name="Body">text without the lists of base items and variations</param>
  /// <param name="Bases">base item and variant name of a generic variant (<c>Chain Mail</c>, <c>+1 Chain Mail</c>)</param>
  /// <param name="Variations">names of the variations</param>
  private sealed record Entry(string Name, string TypeLine, List<string> Stats, List<string> Body, List<(string Base, string Name)> Bases, List<string> Variations);

  #endregion
}
