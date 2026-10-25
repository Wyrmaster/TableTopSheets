using System.Globalization;
using System.Text.RegularExpressions;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Seed;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Extractor;

/// <summary>
///   Reads the sections of the Player's Handbook markdown
/// </summary>
internal sealed partial class HandbookParser(string[] lines)
{
  #region Fields

  private static readonly Dictionary<string, string> AbilityKeys = new()
  {
    ["Strength"] = "str",
    ["Dexterity"] = "dex",
    ["Constitution"] = "con",
    ["Intelligence"] = "int",
    ["Wisdom"] = "wis",
    ["Charisma"] = "cha",
  };

  #endregion

  #region Spells

  public List<SeedSpell> Spells()
  {
    List<SeedSpell> spells = new();
    foreach ((string name, List<string> block) in this.Blocks("## Spell Descriptions", "# Appendix A", "#### "))
    {
      List<string> content = block.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
      if (content.Count == 0)
      {
        continue;
      }

      Match header = SpellLevelRegex().Match(content[0]);
      Match cantrip = CantripRegex().Match(content[0]);
      if (!header.Success && !cantrip.Success)
      {
        continue;
      }

      string Field(string field) => Markdown.Plain(content.FirstOrDefault(line => line.StartsWith($"- **{field}:**", StringComparison.Ordinal))?[($"- **{field}:**".Length)..] ?? string.Empty);

      string castingTime = Field("Casting Time");
      string components = Field("Components");
      string duration = Field("Duration");
      Match material = MaterialRegex().Match(components);
      string classes = content.FirstOrDefault(line => line.StartsWith("**Classes:**", StringComparison.Ordinal))?["**Classes:**".Length..] ?? string.Empty;

      int descriptionStart = block.FindIndex(line => line.Trim() == "---") + 1;
      List<string> description = block.Skip(descriptionStart).Where(line => !line.StartsWith("**Classes:**", StringComparison.Ordinal)).ToList();

      (string? damage, DamageTypeEnum? damageType, List<string>? tiers) = header.Success ? (null, null, null) : CantripDamage(description);
      string plain = Markdown.Plain(string.Join(" ", description));
      Match save = SaveRegex().Match(plain);

      spells.Add(new SeedSpell
      {
        Damage = damage,
        DamageType = damageType,
        CantripDamage = tiers,
        AttackRoll = plain.Contains("spell attack", StringComparison.OrdinalIgnoreCase) ? true : null,
        SaveAbility = save.Success ? AbilityKeys[save.Groups[1].Value] : null,
        Key = SeedKeys.Slug(name),
        Name = name,
        Level = header.Success ? byte.Parse(header.Groups[1].Value) : (byte)0,
        School = Enum.Parse<SpellSchoolEnum>(header.Success ? header.Groups[2].Value : cantrip.Groups[1].Value),
        CastingTime = castingTime.Replace(" or Ritual", string.Empty),
        Ritual = castingTime.Contains("Ritual"),
        Range = Field("Range"),
        Components = material.Success ? components[..material.Index] + "M" : components,
        Material = material.Success ? material.Groups[1].Value : null,
        Concentration = duration.StartsWith("Concentration", StringComparison.Ordinal),
        Duration = duration,
        Lists = classes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(SeedKeys.Slug).ToList(),
        Description = Markdown.ToHtml(description),
      });
    }

    return spells;
  }

  /// <summary>
  ///   Damage of a cantrip and the damage at character levels 1, 5, 11 and 17 from its "Cantrip Upgrade" text
  /// </summary>
  private static (string? Damage, DamageTypeEnum? Type, List<string>? Tiers) CantripDamage(List<string> description)
  {
    string text = Markdown.Plain(string.Join(" ", description));
    Match damage = CantripDamageRegex().Match(text);
    if (!damage.Success)
    {
      return (null, null, null);
    }

    string dice = damage.Groups[1].Value;
    DamageTypeEnum? type = Enum.TryParse(damage.Groups[2].Value, out DamageTypeEnum parsed) ? parsed : null;

    Match tiers = UpgradeTiersRegex().Match(text);
    if (tiers.Success)
    {
      // "2d8 or 2d12" at level 5 means "1d8 or 1d12" at level 1
      string first = tiers.Groups[1].Value.Contains(" or ") ? tiers.Groups[1].Value.Replace("2d", "1d") : dice;
      return (first, type, [first, tiers.Groups[1].Value, tiers.Groups[2].Value, tiers.Groups[3].Value]);
    }

    // Eldritch Blast: more beams instead of more dice
    if (text.Contains("two beams at level 5"))
    {
      return (dice, type, [dice, $"2 × {dice}", $"3 × {dice}", $"4 × {dice}"]);
    }

    return (dice, type, null);
  }

  #endregion

  #region Feats

  public List<SeedFeat> Feats()
  {
    List<SeedFeat> feats = new();
    (string Start, string End)[] sections =
    [
      ("## Origin Feats", "# General Feats"),
      ("# General Feats", "# Fighting Style Feats"),
      ("# Fighting Style Feats", "# Epic Boon Feats"),
      ("# Epic Boon Feats", "# Chapter 6: Equipment"),
    ];

    foreach ((string start, string end) in sections)
    {
      foreach ((string name, List<string> block) in this.Blocks(start, end, "## "))
      {
        string? header = block.FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));
        Match match = header == null ? Match.Empty : FeatHeaderRegex().Match(header);
        if (!match.Success)
        {
          continue;
        }

        FeatCategoryEnum category = match.Groups[1].Value switch
        {
          "Origin" => FeatCategoryEnum.Origin,
          "General" => FeatCategoryEnum.General,
          "Fighting Style" => FeatCategoryEnum.FightingStyle,
          _ => FeatCategoryEnum.EpicBoon,
        };
        string prerequisite = match.Groups[2].Value;
        bool repeatable = block.Any(line => line.StartsWith("**Repeatable:** Yes", StringComparison.Ordinal) || line.StartsWith("***Repeatable.***", StringComparison.Ordinal));

        feats.Add(new SeedFeat
        {
          Key = SeedKeys.Slug(name),
          Name = name,
          Category = category,
          Repeatable = repeatable,
          Prerequisite = PrerequisiteExpression(prerequisite),
          Description = Markdown.ToHtml(block
            .SkipWhile(string.IsNullOrWhiteSpace)
            .Skip(1)
            .Where(line => !line.StartsWith("**Repeatable:**", StringComparison.Ordinal))
            .Prepend(prerequisite.Length > 0 ? $"*Prerequisite: {prerequisite}*" : string.Empty)),
        });
      }
    }

    return feats;
  }

  /// <summary>
  ///   Turns "Level 4+, Charisma 13+" into <c>level &gt;= 4 &amp;&amp; score('cha') &gt;= 13</c>; unknown parts are left to the authored data
  /// </summary>
  private static string? PrerequisiteExpression(string text)
  {
    List<string> parts = new();
    Match level = LevelPrerequisiteRegex().Match(text);
    if (level.Success)
    {
      parts.Add($"level >= {level.Groups[1].Value}");
    }

    foreach (Match ability in AbilityPrerequisiteRegex().Matches(text))
    {
      string score = ability.Groups[2].Value;
      List<string> abilities = AbilityNameRegex().Matches(ability.Groups[1].Value).Select(entry => AbilityKeys[entry.Value]).ToList();
      string expression = string.Join(" || ", abilities.Select(entry => $"score('{entry}') >= {score}"));
      parts.Add(abilities.Count > 1 ? $"({expression})" : expression);
    }

    foreach (Match training in ArmorTrainingRegex().Matches(text))
    {
      parts.Add($"proficient('armor.{training.Groups[1].Value.ToLowerInvariant()}')");
    }

    if (text.Contains("Spellcasting or Pact Magic Feature"))
    {
      parts.Add("spellcaster");
    }

    return parts.Count == 0 ? null : string.Join(" && ", parts);
  }

  #endregion

  #region Species

  public List<SeedSpecies> Species()
  {
    List<SeedSpecies> species = new();
    foreach ((string name, List<string> block) in this.Blocks("# Species Descriptions", "# Chapter 5: Feats", "## "))
    {
      string key = SeedKeys.Slug(name);
      List<string> bullets = block.Where(line => line.StartsWith("- **", StringComparison.Ordinal)).TakeWhile(_ => true).ToList();
      string Field(string field) => Markdown.Plain(bullets.FirstOrDefault(line => line.StartsWith($"- **{field}:**", StringComparison.Ordinal))?[($"- **{field}:**".Length)..] ?? string.Empty);

      SeedSpecies entry = new()
      {
        Key = key,
        Name = name,
        CreatureType = Field("Creature Type"),
        Description = Markdown.ToHtml(bullets.Where(line => line.Contains(":**"))),
      };

      // base traits that every species has: speed and size
      SeedFeature basics = new()
      {
        Key = $"{key}.basics",
        Name = $"{name} Traits",
        SheetSection = SheetSectionEnum.Hidden,
      };
      Match speed = FeetRegex().Match(Field("Speed"));
      if (speed.Success)
      {
        basics.Effects.Add(new SeedEffect { Target = "speed.walk", Op = EffectOperationEnum.Set, Value = speed.Groups[1].Value, Label = name });
      }

      string size = Field("Size");
      bool medium = size.Contains("Medium");
      bool small = size.Contains("Small");
      if (medium && small)
      {
        basics.Choices.Add(new SeedChoice
        {
          Key = $"{key}.size",
          Name = "Size",
          Source = OptionSourceEnum.Static,
          Options =
          [
            new SeedOption { Key = "medium", Name = "Medium", Value = ((int)SizeEnum.Medium).ToString() },
            new SeedOption { Key = "small", Name = "Small", Value = ((int)SizeEnum.Small).ToString() },
          ],
          Effects = [new SeedEffect { Target = "size", Op = EffectOperationEnum.Set, Value = "$value" }],
        });
      }
      else if (small)
      {
        basics.Effects.Add(new SeedEffect { Target = "size", Op = EffectOperationEnum.Set, Value = ((int)SizeEnum.Small).ToString() });
      }

      entry.Features.Add(basics);

      foreach ((string trait, List<string> text) in Traits(block))
      {
        SeedFeature feature = new()
        {
          Key = $"{key}.{SeedKeys.Slug(trait)}",
          Name = trait,
          SheetSection = SheetSectionEnum.SpeciesTraits,
          Description = Markdown.ToHtml(text),
        };

        Match darkvision = DarkvisionRegex().Match(string.Join(" ", text));
        if (trait == "Darkvision" && darkvision.Success)
        {
          feature.Effects.Add(new SeedEffect { Target = "sense.darkvision", Op = EffectOperationEnum.Set, Value = darkvision.Groups[1].Value });
        }

        entry.Features.Add(feature);
      }

      species.Add(entry);
    }

    return species;
  }

  /// <summary>
  ///   Split a block into <c>***Trait.*** text</c> sections
  /// </summary>
  private static IEnumerable<(string Name, List<string> Text)> Traits(List<string> block)
  {
    string? name = null;
    List<string> text = new();
    foreach (string line in block)
    {
      Match match = TraitRegex().Match(line);
      if (match.Success && match.Groups[1].Value != "Repeatable")
      {
        if (name != null)
        {
          yield return (name, text);
        }

        name = match.Groups[1].Value;
        text = [line[match.Length..].Trim()];
        continue;
      }

      if (name != null)
      {
        text.Add(line);
      }
    }

    if (name != null)
    {
      yield return (name, text);
    }
  }

  #endregion

  #region Backgrounds

  public List<SeedBackground> Backgrounds()
  {
    List<SeedBackground> backgrounds = new();
    foreach ((string name, List<string> block) in this.Blocks("## Background Descriptions", "# Species Descriptions", "## "))
    {
      string Field(string field) => Markdown.Plain(block.FirstOrDefault(line => line.StartsWith($"- **{field}:**", StringComparison.Ordinal))?[($"- **{field}:**".Length)..] ?? string.Empty);

      string abilities = Field("Ability Scores");
      if (abilities.Length == 0)
      {
        continue;
      }

      string key = SeedKeys.Slug(name);
      List<string> abilityKeys = abilities.Split(',', StringSplitOptions.TrimEntries).Select(entry => AbilityKeys[entry]).ToList();
      string feat = Field("Feat");
      List<string> skills = Field("Skill Proficiencies").Split(',', StringSplitOptions.TrimEntries).Select(SeedKeys.Slug).ToList();
      string tool = Field("Tool Proficiency");

      SeedBackground background = new()
      {
        Key = key,
        Name = name,
        Abilities = abilityKeys,
        OriginFeat = SeedKeys.Slug(FeatNameRegex().Match(feat).Groups[1].Value),
        Description = Markdown.ToHtml(block.Where(line => line.StartsWith("- **", StringComparison.Ordinal))),
      };

      background.Features.Add(new SeedFeature
      {
        Key = $"{key}.ability-scores",
        Name = "Ability Scores",
        Choices =
        [
          new SeedChoice
          {
            Key = $"{key}.ability-scores",
            Name = "Ability Score Increases",
            Count = "3",
            Source = OptionSourceEnum.Ability,
            Filter = $"oneOf(option.key, {string.Join(", ", abilityKeys.Select(entry => $"'{entry}'"))})",
            AllowDuplicates = true,
            MaxPerOption = 2,
            Effects = [new SeedEffect { Target = "ability.$option.score", Op = EffectOperationEnum.Add, Value = "1", Label = name }],
          },
        ],
      });

      background.Features.Add(new SeedFeature
      {
        Key = $"{key}.skills",
        Name = "Skill Proficiencies",
        Effects = skills.Select(entry => new SeedEffect { Target = $"skill.{entry}.prof", Op = EffectOperationEnum.Proficiency, Value = "proficient", Label = name }).ToList(),
      });

      Match chooseTool = ChooseToolRegex().Match(tool);
      SeedFeature toolFeature = new() { Key = $"{key}.tool", Name = "Tool Proficiency" };
      if (chooseTool.Success)
      {
        toolFeature.Choices.Add(new SeedChoice
        {
          Key = $"{key}.tool",
          Name = "Tool Proficiency",
          Source = OptionSourceEnum.Tool,
          Filter = $"option.category == '{SeedKeys.Slug(ToolGroup(chooseTool.Groups[1].Value))}'",
          Effects = [new SeedEffect { Target = "proficiency.tool.$option", Op = EffectOperationEnum.Proficiency, Value = "proficient", Label = name }],
        });
      }
      else if (tool.Length > 0)
      {
        toolFeature.Effects.Add(new SeedEffect { Target = $"proficiency.tool.{SeedKeys.Slug(tool)}", Op = EffectOperationEnum.Proficiency, Value = "proficient", Label = name });
      }

      background.Features.Add(toolFeature);

      // chapter 2 "Choose Languages": Common plus two standard languages, chosen together with the origin
      background.Features.Add(new SeedFeature
      {
        Key = $"{key}.languages",
        Name = "Languages",
        Effects = [new SeedEffect { Target = "language.common", Op = EffectOperationEnum.Grant, Label = "Common" }],
        Choices =
        [
          new SeedChoice
          {
            Key = $"{key}.languages",
            Name = "Standard Languages",
            Count = "2",
            Source = OptionSourceEnum.Language,
            Filter = "option.category == 'standard' && option.key != 'common'",
            Effects = [new SeedEffect { Target = "language.$option", Op = EffectOperationEnum.Grant }],
          },
        ],
      });
      backgrounds.Add(background);
    }

    return backgrounds;
  }

  private static string ToolGroup(string name) => name switch
  {
    "Artisan's Tools" => "artisan",
    _ => name,
  };

  #endregion

  #region Classes

  public (List<SeedClassFeatureText> Texts, List<SeedFeature> Options) ClassFeatures()
  {
    List<SeedClassFeatureText> texts = new();
    List<SeedFeature> options = new();
    int start = this.Find("# Chapter 3: Character Classes");
    int end = this.Find("# Chapter 4: Character Origins");

    string? @class = null;
    for (int index = start; index < end; index++)
    {
      string line = lines[index];
      if (line.StartsWith("## ", StringComparison.Ordinal))
      {
        @class = SeedKeys.Slug(line[3..]);
        continue;
      }

      Match match = ClassFeatureRegex().Match(line);
      if (@class == null || !match.Success)
      {
        continue;
      }

      List<string> block = lines
        .Skip(index + 1)
        .Take(end - index - 1)
        .TakeWhile(entry => !entry.StartsWith("#### ", StringComparison.Ordinal) && !entry.StartsWith("## ", StringComparison.Ordinal))
        .ToList();
      byte level = byte.Parse(match.Groups[1].Value);
      string name = match.Groups[2].Value.Trim();

      if (name.EndsWith(" Options", StringComparison.Ordinal))
      {
        string category = SeedKeys.Slug(name[..^" Options".Length]);
        List<(string Name, List<string> Text)> entries = new();
        bool collectingBenefits = false;
        foreach ((string option, List<string> text) in Traits(block))
        {
          // titled paragraphs after "the following benefits" belong to the previous option until the next option with a prerequisite
          bool hasPrerequisite = text.Count > 0 && text[0].StartsWith("*Prerequisite", StringComparison.Ordinal);
          if (collectingBenefits && !hasPrerequisite)
          {
            entries[^1].Text.Add($"***{option}.*** {string.Join(Environment.NewLine, text)}");
            entries[^1].Text.Add(string.Empty);
            continue;
          }

          string joined = string.Join(" ", text);
          collectingBenefits = joined.Contains("following benefits") || joined.Contains("benefits below");
          entries.Add((option, text.ToList()));
        }

        foreach ((string option, List<string> text) in entries)
        {
          if (text.Count > 0)
          {
            text[0] = EmptyPrerequisiteRegex().Replace(text[0], string.Empty);
          }

          options.Add(new SeedFeature
          {
            Key = $"{category}.{SeedKeys.Slug(option)}",
            Name = option,
            OwnerType = OwnerTypeEnum.Option,
            Category = category,
            SheetSection = SheetSectionEnum.ClassFeatures,
            Description = Markdown.ToHtml(text),
          });
        }

        continue;
      }

      texts.Add(new SeedClassFeatureText { Class = @class, Level = level, Name = name, Description = Markdown.ToHtml(block) });
    }

    return (texts, options);
  }

  #endregion

  #region Equipment

  public List<SeedItem> Items()
  {
    List<SeedItem> items = new();
    items.AddRange(this.Weapons());
    items.AddRange(this.Armor());
    items.AddRange(this.Tools());
    items.AddRange(this.Gear(items.Select(entry => entry.Key).ToHashSet()));
    return items;
  }

  private IEnumerable<SeedItem> Weapons()
  {
    WeaponCategoryEnum category = WeaponCategoryEnum.Simple;
    bool ranged = false;
    foreach (string[] cells in this.TableRows("##### Weapons"))
    {
      if (cells.Length == 1 || cells.Skip(1).All(string.IsNullOrEmpty))
      {
        string group = Markdown.Plain(cells[0]);
        category = group.StartsWith("Martial", StringComparison.Ordinal) ? WeaponCategoryEnum.Martial : WeaponCategoryEnum.Simple;
        ranged = group.Contains("Ranged");
        continue;
      }

      string name = Markdown.Plain(cells[0]);
      string[] damage = cells[1].Split(' ', 2);
      WeaponPropertyEnum properties = ranged ? WeaponPropertyEnum.None : WeaponPropertyEnum.Melee;
      string? versatile = null;
      short? rangeNormal = null;
      short? rangeLong = null;
      foreach (string property in PropertySplitRegex().Split(cells[2]).Select(entry => entry.Trim()).Where(entry => entry is not ("—" or "")))
      {
        Match range = RangeRegex().Match(property);
        if (range.Success)
        {
          rangeNormal = short.Parse(range.Groups[1].Value);
          rangeLong = short.Parse(range.Groups[2].Value);
          properties |= WeaponPropertyEnum.Range;
        }

        string word = property.Split(' ', '(')[0];
        properties |= word switch
        {
          "Ammunition" => WeaponPropertyEnum.Ammunition,
          "Finesse" => WeaponPropertyEnum.Finesse,
          "Heavy" => WeaponPropertyEnum.Heavy,
          "Light" => WeaponPropertyEnum.Light,
          "Loading" => WeaponPropertyEnum.Loading,
          "Reach" => WeaponPropertyEnum.Reach,
          "Thrown" => WeaponPropertyEnum.Thrown,
          "Two-Handed" => WeaponPropertyEnum.TwoHanded,
          "Versatile" => WeaponPropertyEnum.Versatile,
          _ => WeaponPropertyEnum.None,
        };

        if (word == "Versatile")
        {
          versatile = DiceRegex().Match(property).Value;
        }
      }

      yield return new SeedItem
      {
        Key = SeedKeys.Slug(name),
        Name = name,
        Kind = ItemKindEnum.Weapon,
        WeaponCategory = category,
        Ranged = ranged,
        Damage = damage[0],
        DamageType = Enum.Parse<DamageTypeEnum>(damage[1]),
        Properties = properties,
        Versatile = versatile,
        RangeNormal = rangeNormal,
        RangeLong = rangeLong,
        Mastery = SeedKeys.Slug(cells[3]),
        Weight = Weight(cells[4]),
        Cost = Cost(cells[5]),
      };
    }
  }

  private IEnumerable<SeedItem> Armor()
  {
    ArmorEnum category = ArmorEnum.Light;
    foreach (string[] cells in this.TableRows("##### Armor"))
    {
      if (cells.Length == 1 || cells.Skip(1).All(string.IsNullOrEmpty))
      {
        string group = Markdown.Plain(cells[0]);
        category = group.StartsWith("Light", StringComparison.Ordinal) ? ArmorEnum.Light
          : group.StartsWith("Medium", StringComparison.Ordinal) ? ArmorEnum.Medium
          : group.StartsWith("Heavy", StringComparison.Ordinal) ? ArmorEnum.Heavy
          : ArmorEnum.Shield;
        continue;
      }

      string name = Markdown.Plain(cells[0]);
      Match ac = ArmorClassRegex().Match(cells[1]);
      Match strength = StrengthRegex().Match(cells[2]);
      yield return new SeedItem
      {
        Key = SeedKeys.Slug(name),
        Name = name,
        Kind = ItemKindEnum.Armor,
        ArmorCategory = category,
        BaseAc = byte.Parse(ac.Groups[1].Value),
        DexCap = !cells[1].Contains("Dex") ? (byte)0 : ac.Groups[2].Success ? byte.Parse(ac.Groups[2].Value) : null,
        StrengthRequirement = strength.Success ? byte.Parse(strength.Groups[1].Value) : null,
        StealthDisadvantage = cells[3].Contains("Disadvantage"),
        Weight = Weight(cells[4]),
        Cost = Cost(cells[5]),
      };
    }
  }

  private IEnumerable<SeedItem> Tools()
  {
    string group = "artisan";
    int start = this.Find("## Tools");
    int end = this.Find("## Adventuring Gear");
    for (int index = start; index < end; index++)
    {
      string line = lines[index];
      if (line.StartsWith("### ", StringComparison.Ordinal))
      {
        group = line.Contains("Artisan") ? "artisan" : "other";
        continue;
      }

      Match match = ItemHeadingRegex().Match(line);
      if (!line.StartsWith("#### ", StringComparison.Ordinal) || !match.Success)
      {
        continue;
      }

      List<string> block = lines.Skip(index + 1).TakeWhile(entry => !entry.StartsWith("#", StringComparison.Ordinal)).ToList();
      string name = match.Groups[1].Value.Trim();
      string? variants = block.FirstOrDefault(entry => entry.StartsWith("- **Variants:**", StringComparison.Ordinal));
      if (variants != null)
      {
        string variantGroup = SeedKeys.Slug(name);
        foreach (Match variant in VariantRegex().Matches(variants))
        {
          string variantName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(variant.Groups[1].Value);
          yield return new SeedItem
          {
            Key = SeedKeys.Slug(variantName),
            Name = variantName,
            Kind = ItemKindEnum.Tool,
            ToolGroup = variantGroup,
            Cost = Cost(variant.Groups[2].Value),
            Description = Markdown.ToHtml(block.Where(entry => !entry.StartsWith("- **Variants:**", StringComparison.Ordinal)).Prepend($"*{name}*")),
          };
        }

        continue;
      }

      yield return new SeedItem
      {
        Key = SeedKeys.Slug(name),
        Name = name,
        Kind = ItemKindEnum.Tool,
        ToolGroup = group,
        Cost = Cost(match.Groups[2].Value),
        Weight = Weight(block.FirstOrDefault(entry => entry.StartsWith("- **Weight:**", StringComparison.Ordinal))?["- **Weight:**".Length..] ?? "—"),
        Description = Markdown.ToHtml(block),
      };
    }
  }

  private IEnumerable<SeedItem> Gear(HashSet<string> existing)
  {
    int start = this.Find("## Adventuring Gear");
    Dictionary<string, List<string>> descriptions = new();
    for (int index = start; index < lines.Length && !lines[index].StartsWith("## Mounts", StringComparison.Ordinal) && !lines[index].StartsWith("# ", StringComparison.Ordinal); index++)
    {
      Match match = ItemHeadingRegex().Match(lines[index]);
      if (lines[index].StartsWith("#### ", StringComparison.Ordinal) && match.Success)
      {
        descriptions[SeedKeys.Slug(match.Groups[1].Value)] = lines.Skip(index + 1).TakeWhile(entry => !entry.StartsWith("#### ", StringComparison.Ordinal)).ToList();
      }
    }

    foreach (string[] cells in this.TableRows("##### Adventuring Gear"))
    {
      string name = Markdown.Plain(cells[0]);
      string key = SeedKeys.Slug(name);
      if (cells.Length < 3 || existing.Contains(key))
      {
        continue;
      }

      yield return new SeedItem
      {
        Key = key,
        Name = name,
        Kind = ItemKindEnum.Gear,
        Weight = Weight(cells[1]),
        Cost = Cost(cells[2]),
        Description = descriptions.TryGetValue(key, out List<string>? description) ? Markdown.ToHtml(description) : null,
      };
    }
  }

  public List<SeedNamed> Masteries()
    => this.Blocks("### Mastery Properties", "## Armor", "#### ")
      .Select(entry => new SeedNamed { Key = SeedKeys.Slug(entry.Name), Name = entry.Name, Description = Markdown.ToHtml(entry.Block) })
      .ToList();

  public List<SeedLanguage> Languages()
  {
    List<SeedLanguage> languages = new();
    foreach (string[] cells in this.TableRows("##### Standard Languages"))
    {
      languages.Add(new SeedLanguage { Key = SeedKeys.Slug(cells[1]), Name = cells[1] });
    }

    foreach (string[] cells in this.TableRows("##### Rare Languages"))
    {
      string name = cells[0].TrimEnd('*');
      languages.Add(new SeedLanguage { Key = SeedKeys.Slug(name), Name = name, Rare = true });
    }

    return languages;
  }

  private static int Cost(string text)
  {
    Match match = CostRegex().Match(text.Replace(",", string.Empty));
    if (!match.Success)
    {
      return 0;
    }

    int value = int.Parse(match.Groups[1].Value);
    return match.Groups[2].Value switch
    {
      "CP" => value,
      "SP" => value * 10,
      "EP" => value * 50,
      "GP" => value * 100,
      "PP" => value * 1000,
      _ => 0,
    };
  }

  private static double Weight(string text)
  {
    Match match = WeightRegex().Match(text);
    if (!match.Success)
    {
      return 0;
    }

    string value = match.Groups[1].Value;
    if (value.Contains('/'))
    {
      string[] parts = value.Split('/');
      return double.Parse(parts[0], CultureInfo.InvariantCulture) / double.Parse(parts[1], CultureInfo.InvariantCulture);
    }

    return double.Parse(value, CultureInfo.InvariantCulture);
  }

  #endregion

  #region Helpers

  private int Find(string heading, int from = 0)
  {
    for (int index = from; index < lines.Length; index++)
    {
      if (lines[index].TrimEnd().StartsWith(heading, StringComparison.Ordinal))
      {
        return index;
      }
    }

    throw new InvalidOperationException($"Heading '{heading}' not found");
  }

  /// <summary>
  ///   Blocks between two headings, split at every line starting with the block prefix (exact heading level)
  /// </summary>
  private IEnumerable<(string Name, List<string> Block)> Blocks(string startHeading, string endHeading, string prefix)
  {
    int start = this.Find(startHeading);
    int end = this.Find(endHeading, start + 1);
    string? name = null;
    List<string> block = new();
    for (int index = start + 1; index < end; index++)
    {
      string line = lines[index];
      bool heading = line.StartsWith(prefix, StringComparison.Ordinal);
      bool higherHeading = !heading && line.StartsWith('#') && line.TakeWhile(character => character == '#').Count() < prefix.Trim().Length;
      if (heading || higherHeading)
      {
        if (name != null)
        {
          yield return (name, block);
        }

        name = heading ? line[prefix.Length..].Trim() : null;
        block = new();
        continue;
      }

      block.Add(line);
    }

    if (name != null)
    {
      yield return (name, block);
    }
  }

  /// <summary>
  ///   Data rows of the table following a heading
  /// </summary>
  private IEnumerable<string[]> TableRows(string heading)
  {
    int index = this.Find(heading) + 1;
    bool header = true;
    for (; index < lines.Length && lines[index].TrimStart().StartsWith('|'); index++)
    {
      string[] cells = lines[index].Trim().Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();
      if (header)
      {
        header = false;
        continue;
      }

      if (cells.All(cell => cell.Length == 0 || cell.All(character => character is '-' or ':')))
      {
        continue;
      }

      yield return cells;
    }
  }

  [GeneratedRegex(@"(\d+d\d+) (?:(Acid|Bludgeoning|Cold|Fire|Force|Lightning|Necrotic|Piercing|Poison|Psychic|Radiant|Slashing|Thunder) )?damage")]
  private static partial Regex CantripDamageRegex();

  [GeneratedRegex(@"levels 5 \(([^)]+)\), 11 \(([^)]+)\), and 17 \(([^)]+)\)")]
  private static partial Regex UpgradeTiersRegex();

  [GeneratedRegex("(Strength|Dexterity|Constitution|Intelligence|Wisdom|Charisma) saving throw")]
  private static partial Regex SaveRegex();

  [GeneratedRegex(@"^\*Level (\d) (\w+)")]
  private static partial Regex SpellLevelRegex();

  [GeneratedRegex(@"^\*(\w+) Cantrip")]
  private static partial Regex CantripRegex();

  [GeneratedRegex(@"M \((.*)\)$")]
  private static partial Regex MaterialRegex();

  [GeneratedRegex(@"^(Origin|General|Fighting Style|Epic Boon) Feat(?: \(Prerequisite: (.*)\))?\s*$")]
  private static partial Regex FeatHeaderRegex();

  [GeneratedRegex(@"Level (\d+)\+")]
  private static partial Regex LevelPrerequisiteRegex();

  [GeneratedRegex(@"((?:(?:Strength|Dexterity|Constitution|Intelligence|Wisdom|Charisma)(?:, | or |, or )?)+) (\d+)\+")]
  private static partial Regex AbilityPrerequisiteRegex();

  [GeneratedRegex("Strength|Dexterity|Constitution|Intelligence|Wisdom|Charisma")]
  private static partial Regex AbilityNameRegex();

  [GeneratedRegex(@"(Light|Medium|Heavy) Armor Training")]
  private static partial Regex ArmorTrainingRegex();

  [GeneratedRegex(@"(\d+) feet")]
  private static partial Regex FeetRegex();

  [GeneratedRegex(@"Darkvision with a range of (\d+) feet")]
  private static partial Regex DarkvisionRegex();

  [GeneratedRegex(@"^\*\*\*([^*]+?)\.\*\*\*")]
  private static partial Regex TraitRegex();

  [GeneratedRegex(@"^([^(]+)")]
  private static partial Regex FeatNameRegex();

  [GeneratedRegex(@"Choose one kind of (.+)$")]
  private static partial Regex ChooseToolRegex();

  [GeneratedRegex(@"^#### Level (\d+): (.+)$")]
  private static partial Regex ClassFeatureRegex();

  [GeneratedRegex(@"^\*Prerequisite: \*\s*")]
  private static partial Regex EmptyPrerequisiteRegex();

  [GeneratedRegex(@",\s*(?![^()]*\))")]
  private static partial Regex PropertySplitRegex();

  [GeneratedRegex(@"Range (\d+)/(\d+)")]
  private static partial Regex RangeRegex();

  [GeneratedRegex(@"\d+d\d+")]
  private static partial Regex DiceRegex();

  [GeneratedRegex(@"^\+?(\d+)(?: \+ Dex modifier(?: \(max (\d)\))?)?")]
  private static partial Regex ArmorClassRegex();

  [GeneratedRegex(@"Str (\d+)")]
  private static partial Regex StrengthRegex();

  [GeneratedRegex(@"^#### (.+?) \(([^)]*)\)\s*$")]
  private static partial Regex ItemHeadingRegex();

  [GeneratedRegex(@"\*([^*]+)\* \(([^)]+)\)")]
  private static partial Regex VariantRegex();

  [GeneratedRegex(@"(\d+) (CP|SP|EP|GP|PP)")]
  private static partial Regex CostRegex();

  [GeneratedRegex(@"([\d/.]+) lb")]
  private static partial Regex WeightRegex();

  #endregion
}
