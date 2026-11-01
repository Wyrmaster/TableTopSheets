using System.Text.Json;
using TableTopSheets.Common.Card;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Seed;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   Card data of real spells and magic items from the extracted rule book, built by hand like the API does
/// </summary>
public static class CardSamples
{
  #region Fields

  private static readonly Lazy<Dictionary<string, JsonElement>> Spells = new(() => Load("spells.json", "spells"));

  private static readonly Lazy<Dictionary<string, JsonElement>> Items = new(() => Load("magic-items.json", "items"));

  #endregion

  #region Methods

  /// <summary>
  ///   Card of a spell, e.g. <c>fireball</c>
  /// </summary>
  public static CardData Spell(string key, byte[]? image = null)
  {
    JsonElement spell = Spells.Value[key];
    string level = Text(spell, "level") ?? "0";
    string school = Text(spell, "school") ?? string.Empty;
    string subtitle = level == "0" ? $"{school} Cantrip" : $"Level {level} {school}";
    string components = Text(spell, "components") ?? string.Empty;
    if (Text(spell, "material") is { } material)
    {
      components += material.Length > 30 ? $" ({material[..27].TrimEnd()}…)" : $" ({material})";
    }

    string duration = Text(spell, "duration") ?? string.Empty;
    if (Text(spell, "concentration") == "True")
    {
      duration = $"Concentration, {duration}";
    }

    List<CardFact> facts =
    [
      new("Level", level == "0" ? "Cantrip" : $"Level {level} {school}"),
      new("Casting Time", Text(spell, "castingTime") ?? string.Empty),
      new("Range", Text(spell, "range") ?? string.Empty),
      new("Components", components),
      new("Duration", duration),
    ];
    if (Text(spell, "ritual") == "True")
    {
      facts.Add(new("Ritual", "Yes"));
    }

    if (Text(spell, "saveAbility") is { } save)
    {
      facts.Add(new("Save", save.ToUpperInvariant()));
    }

    string lists = spell.TryGetProperty("lists", out JsonElement entries)
      ? string.Join(", ", entries
        .EnumerateArray()
        .Select(entry => Capitalize(entry.GetString()!)))
      : string.Empty;

    return new CardData
    {
      Kind = CardKindEnum.Spell,
      Name = Text(spell, "name")!,
      Subtitle = subtitle,
      Facts = facts,
      Description = HtmlText.ToBlocks(Text(spell, "description")),
      Image = image,
      Footer = $"Spell · {lists}",
    };
  }

  /// <summary>
  ///   Card of a magic item, e.g. <c>deck-of-many-things</c>; extra facts (weapon damage, …) go after the generic ones
  /// </summary>
  public static CardData Item(string key, byte[]? image = null, string? name = null, params CardFact[] extraFacts)
  {
    JsonElement item = Items.Value[key];
    string rarity = Text(item, "rarity") ?? string.Empty;
    string category = Capitalize((Text(item, "category") ?? "weapon").Replace('-', ' '));
    List<CardFact> facts = [new("Type", category), new("Rarity", rarity)];
    if (Text(item, "attunement") == "True")
    {
      facts.Add(new("Attunement", "Required"));
    }

    if (Text(item, "weight") is { } weight)
    {
      facts.Add(new("Weight", $"{weight} lb."));
    }

    facts.AddRange(extraFacts);

    // the first paragraph repeats type and rarity in italics
    List<TextBlock> blocks = HtmlText
      .ToBlocks(Text(item, "description"))
      .ToList();
    if (blocks is [{ Kind: TextBlockKindEnum.Paragraph, Spans: [{ Italic: true }] }, ..])
    {
      blocks.RemoveAt(0);
    }

    return new CardData
    {
      Kind = CardKindEnum.Item,
      Name = name ?? Text(item, "name")!,
      Subtitle = $"{category}, {rarity}",
      Facts = facts,
      Description = blocks,
      Image = image,
      Footer = $"Item · {rarity}",
    };
  }

  /// <summary>
  ///   Artwork of the seed data (<c>Images/&lt;folder&gt;/&lt;key&gt;.jpg</c>), or <paramref name="fallback"/> when it isn't there
  /// </summary>
  public static byte[] Artwork(string folder, string key, byte[] fallback)
  {
    string path = Path.Combine(SeedLoader.DefaultDirectory, "Images", folder, $"{key}.jpg");
    return File.Exists(path) ? File.ReadAllBytes(path) : fallback;
  }

  /// <summary>
  ///   Every word of a description in reading order (table cells row by row)
  /// </summary>
  public static List<string> Words(IReadOnlyList<TextBlock> blocks) => blocks
    .SelectMany(block => block.Kind switch
    {
      TextBlockKindEnum.List => block.Items.Select(Join),
      TextBlockKindEnum.Table => block.Rows.SelectMany(row => row.Select(Join)),
      _ => [Join(block.Spans)],
    })
    .SelectMany(Split)
    .ToList();

  /// <summary>
  ///   Words of a text
  /// </summary>
  public static IEnumerable<string> Split(string text) => text.Split([' ', '\n', '\r', '\t'], StringSplitOptions.RemoveEmptyEntries);

  #endregion

  #region Helpers

  private static string Join(IReadOnlyList<TextSpan> spans) => string.Concat(spans.Select(span => span.Text));

  private static string Capitalize(string text) => text.Length == 0 ? text : char.ToUpperInvariant(text[0]) + text[1..];

  private static string? Text(JsonElement element, string name)
  {
    if (!element.TryGetProperty(name, out JsonElement value))
    {
      return null;
    }

    return value.ValueKind switch
    {
      JsonValueKind.String => value.GetString(),
      JsonValueKind.True => "True",
      JsonValueKind.False => "False",
      JsonValueKind.Null => null,
      _ => value.GetRawText(),
    };
  }

  private static Dictionary<string, JsonElement> Load(string file, string property)
  {
    string path = Path.Combine(SeedLoader.DefaultDirectory, "Extracted", file);
    using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
    return document.RootElement
      .GetProperty(property)
      .EnumerateArray()
      .ToDictionary(KeyOf, entry => entry.Clone());
  }

  private static string KeyOf(JsonElement entry) => entry
    .GetProperty("key")
    .GetString()!;

  #endregion
}
