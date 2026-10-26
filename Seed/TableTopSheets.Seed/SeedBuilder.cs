using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Converts a merged <see cref="SeedDocument"/> into entities with deterministic ids
/// </summary>
public sealed class SeedBuilder
{
  #region Fields

  private readonly SeedDocument _document;
  private readonly SeedResult _result;
  private readonly Dictionary<string, Feature> _features = new();
  private readonly List<(ChoiceOption Option, string FeatureKey)> _pendingGrants = new();
  private readonly List<(SpellcastingProfile Profile, string? ClassKey, Feature Feature)> _pendingProfiles = new();
  private readonly List<(string Owner, string ChoiceKey)> _pendingSameAs = new();
  private readonly List<string> _errors = new();

  #endregion

  #region Constructor

  private SeedBuilder(SeedDocument document)
  {
    this._document = document;
    this._result = new SeedResult
    {
      Source = new Source { Id = SeedKeys.Id("source", "phb-2024"), Key = "phb-2024", Name = "Player's Handbook (2024)" },
    };
  }

  #endregion

  #region Public Methods

  public static SeedResult Build(SeedDocument document)
  {
    SeedBuilder builder = new(document);
    builder.BuildAll();
    if (builder._errors.Count > 0)
    {
      throw new InvalidDataException("Seed data is invalid:" + Environment.NewLine + string.Join(Environment.NewLine, builder._errors));
    }

    return builder._result;
  }

  #endregion

  #region Build

  private void BuildAll()
  {
    foreach (SeedLanguage language in this._document.Languages)
    {
      this._result.Languages.Add(this.Rule(new Language { Key = language.Key, Name = language.Name ?? language.Key, Rare = language.Rare }, "language", language));
    }

    foreach (SeedNamed mastery in this._document.Masteries)
    {
      this._result.Masteries.Add(this.Rule(new WeaponMastery { Key = mastery.Key, Name = mastery.Name ?? mastery.Key }, "mastery", mastery));
    }

    foreach (SeedSpell spell in this._document.Spells)
    {
      this._result.Spells.Add(this.Rule(new Spell
      {
        Key = spell.Key,
        Name = spell.Name ?? spell.Key,
        Level = spell.Level ?? 0,
        School = spell.School ?? default,
        CastingTime = spell.CastingTime ?? string.Empty,
        Ritual = spell.Ritual ?? false,
        Range = spell.Range ?? string.Empty,
        Components = spell.Components ?? string.Empty,
        Material = spell.Material,
        Concentration = spell.Concentration ?? false,
        Duration = spell.Duration ?? string.Empty,
        Lists = spell.Lists ?? [],
        Damage = spell.Damage,
        DamageType = spell.DamageType,
        AttackRoll = spell.AttackRoll ?? false,
        SaveAbility = spell.SaveAbility,
        CantripDamage = spell.CantripDamage ?? [],
      }, "spell", spell));
    }

    foreach (SeedItem item in this._document.Items)
    {
      this._result.Items.Add(this.BuildItem(item));
    }

    foreach (SeedFeat seed in this._document.Feats)
    {
      Feat feat = this.Rule(new Feat
      {
        Key = seed.Key,
        Name = seed.Name ?? seed.Key,
        Category = seed.Category ?? FeatCategoryEnum.General,
        Repeatable = seed.Repeatable ?? false,
        PrerequisiteExpr = seed.Prerequisite,
      }, "feat", seed);
      this._result.Feats.Add(feat);

      // a feat without authored mechanics still shows its text
      List<SeedFeature> features = seed.Features.Count > 0
        ? seed.Features
        : [new SeedFeature { Key = $"feat.{seed.Key}", Name = feat.Name }];
      if (features[0].Description == null)
      {
        features[0].Description = seed.Description;
      }

      foreach (SeedFeature feature in features)
      {
        feat.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Feat, SheetSectionEnum.Feats, entity => entity.FeatId = feat.Id));
      }
    }

    foreach (SeedSpecies seed in this._document.Species)
    {
      Species species = this.Rule(new Species { Key = seed.Key, Name = seed.Name ?? seed.Key, CreatureType = seed.CreatureType ?? "Humanoid" }, "species", seed);
      this._result.Species.Add(species);
      foreach (SeedFeature feature in seed.Features)
      {
        species.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Species, SheetSectionEnum.SpeciesTraits, entity => entity.SpeciesId = species.Id));
      }
    }

    foreach (SeedBackground seed in this._document.Backgrounds)
    {
      Background background = this.Rule(new Background
      {
        Key = seed.Key,
        Name = seed.Name ?? seed.Key,
        AbilityOptions = seed.Abilities ?? [],
        StartingEquipment = this.BuildStartingEquipment($"background '{seed.Key}'", seed.StartingEquipment),
      }, "background", seed);
      if (seed.OriginFeat != null)
      {
        Feat? feat = this._result.Feats.FirstOrDefault(entry => entry.Key == seed.OriginFeat);
        if (feat == null)
        {
          this._errors.Add($"background '{seed.Key}': origin feat '{seed.OriginFeat}' doesn't exist");
        }

        background.OriginFeatId = feat?.Id;
      }

      this._result.Backgrounds.Add(background);
      foreach (SeedFeature feature in seed.Features)
      {
        background.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Background, SheetSectionEnum.Hidden, entity => entity.BackgroundId = background.Id));
      }
    }

    foreach (SeedFeature seed in this._document.Features)
    {
      OwnerTypeEnum owner = seed.OwnerType ?? OwnerTypeEnum.Option;
      this.BuildFeature(seed, owner, SheetSectionEnum.ClassFeatures, entity =>
      {
        if (owner == OwnerTypeEnum.Spell)
        {
          Spell? spell = this._result.Spells.FirstOrDefault(entry => entry.Key == seed.Spell);
          if (spell == null)
          {
            this._errors.Add($"feature '{seed.Key}': spell '{seed.Spell}' doesn't exist");
          }

          entity.SpellId = spell?.Id;
          entity.Toggleable = true;
        }
      });
    }

    foreach (SeedClass seed in this._document.Classes)
    {
      this._result.Classes.Add(this.BuildClass(seed));
    }

    this.AddScales(null, this._document.Scales);

    foreach ((string table, List<List<int>> rows) in this._document.SlotTables)
    {
      for (int level = 0; level < rows.Count; level++)
      {
        for (int spellLevel = 0; spellLevel < rows[level].Count; spellLevel++)
        {
          if (rows[level][spellLevel] == 0)
          {
            continue;
          }

          this._result.SpellSlotRows.Add(new SpellSlotRow
          {
            Id = SeedKeys.Id("slot", $"{table}:{level + 1}:{spellLevel + 1}"),
            TableKey = table,
            CasterLevel = (byte)(level + 1),
            SpellLevel = (byte)(spellLevel + 1),
            Count = (byte)rows[level][spellLevel],
          });
        }
      }
    }

    this.ResolveReferences();
    this.CheckUniqueIds();
  }

  /// <summary>
  ///   Two seed records producing the same id would only fail when written to the database
  /// </summary>
  private void CheckUniqueIds()
  {
    IEnumerable<(string Type, Guid Id)> ids = this._result.Features
      .SelectMany(feature => feature.Effects
        .Select(entry => ($"effect of '{feature.Key}'", entry.Id))
        .Concat(feature.Choices.Select(entry => ($"choice '{entry.Key}'", entry.Id)))
        .Concat(feature.Choices.SelectMany(choice => choice.Effects.Select(entry => ($"effect of choice '{choice.Key}'", entry.Id))))
        .Concat(feature.Choices.SelectMany(choice => choice.Options.Select(entry => ($"option '{choice.Key}:{entry.Key}'", entry.Id))))
        .Concat(feature.Resources.Select(entry => ($"resource '{entry.Key}'", entry.Id))));
    foreach (IGrouping<Guid, (string Type, Guid Id)> duplicate in ids
      .GroupBy(entry => entry.Id)
      .Where(group => group.Count() > 1))
    {
      this._errors.Add($"duplicate id: {string.Join(", ", duplicate.Select(entry => entry.Type))}");
    }
  }

  private CharacterClass BuildClass(SeedClass seed)
  {
    CharacterClass @class = this.Rule(new CharacterClass
    {
      Key = seed.Key,
      Name = seed.Name ?? seed.Key,
      HitDie = seed.HitDie ?? DiceEnum.D8,
      PrimaryAbilities = seed.PrimaryAbilities ?? [],
      PrimaryAbilityMode = seed.PrimaryAbilityMode ?? PrimaryAbilityModeEnum.All,
      SubclassLevel = seed.SubclassLevel ?? 3,
      StartingEquipment = this.BuildStartingEquipment($"class '{seed.Key}'", seed.StartingEquipment),
    }, "class", seed);

    this.AddScales(@class, seed.Scales);

    List<SeedClassFeatureText> texts = this._document.ClassFeatureTexts
      .Where(entry => entry.Class == seed.Key)
      .ToList();
    List<SeedFeature> features = seed.Features.ToList();
    HashSet<SeedClassFeatureText> used = new();

    // descriptions of the rule book fill authored features with the same level and name
    foreach (SeedFeature feature in features.Where(entry => entry.Description == null))
    {
      SeedClassFeatureText? text = texts.FirstOrDefault(entry => entry.Level == feature.Level && entry.Name == feature.Name);
      if (text != null)
      {
        feature.Description = text.Description;
        used.Add(text);
      }
    }

    used.UnionWith(texts.Where(text => features.Any(feature => feature.Level == text.Level && feature.Name == text.Name)));

    // every other feature of the rule book is added as text so the sheet is complete even without mechanics
    foreach (SeedClassFeatureText text in texts
      .Except(used)
      .Where(entry => entry.Name != "Subclass Feature" && !entry.Name.EndsWith(" Subclass", StringComparison.Ordinal)))
    {
      string key = $"{seed.Key}.{SeedKeys.Slug(text.Name)}";
      if (texts.Count(entry => entry.Name == text.Name) > 1 || features.Any(entry => entry.Key == key))
      {
        key += $"-{text.Level}";
      }

      features.Add(new SeedFeature { Key = key, Name = text.Name, Level = text.Level, Description = text.Description });
    }

    foreach (SeedFeature feature in features.OrderBy(entry => entry.Level ?? 0))
    {
      @class.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Class, SheetSectionEnum.ClassFeatures, entity => entity.ClassId = @class.Id, @class));
    }

    foreach (SeedSubclass subclassSeed in seed.Subclasses)
    {
      Subclass subclass = this.Rule(new Subclass { Key = subclassSeed.Key, Name = subclassSeed.Name ?? subclassSeed.Key, ClassId = @class.Id, Class = @class }, "subclass", subclassSeed);
      @class.Subclasses.Add(subclass);
      this.AddScales(@class, subclassSeed.Scales);
      foreach (SeedFeature feature in subclassSeed.Features)
      {
        subclass.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Subclass, SheetSectionEnum.ClassFeatures, entity => entity.SubclassId = subclass.Id, @class));
      }
    }

    return @class;
  }

  /// <summary>
  ///   Scale rows of a class (counted with the class level) or, without class, counted with the character level
  /// </summary>
  private void AddScales(CharacterClass? @class, Dictionary<string, string> scales)
  {
    foreach ((string key, string values) in scales)
    {
      string[] cells = values.Split(',', StringSplitOptions.TrimEntries);
      if (cells.Length != 20)
      {
        this._errors.Add($"scale '{key}' needs 20 values (level 1..20) but has {cells.Length}");
        continue;
      }

      for (int level = 0; level < cells.Length; level++)
      {
        this._result.ScaleValues.Add(new ScaleValue
        {
          Id = SeedKeys.Id("scale", $"{key}:{level + 1}"),
          Key = key,
          Level = (byte)(level + 1),
          Value = cells[level],
          ClassId = @class?.Id,
        });
      }
    }
  }

  private Feature BuildFeature(SeedFeature seed, OwnerTypeEnum ownerType, SheetSectionEnum defaultSection, Action<Feature> owner, CharacterClass? @class = null)
  {
    if (this._features.ContainsKey(seed.Key))
    {
      this._errors.Add($"feature '{seed.Key}' is defined twice");
    }

    Feature feature = new()
    {
      Id = SeedKeys.Id("feature", seed.Key),
      Key = seed.Key,
      Name = seed.Name ?? seed.Key,
      DescriptionHtml = seed.Description ?? string.Empty,
      OwnerType = ownerType,
      Level = seed.Level,
      GrantMode = seed.GrantMode ?? GrantModeEnum.Always,
      Category = seed.Category,
      SheetSection = seed.SheetSection ?? (seed.Effects.Count > 0 || seed.Choices.Count > 0 || seed.Description != null ? defaultSection : SheetSectionEnum.Hidden),
      PrerequisiteExpr = seed.Prerequisite,
      Toggleable = seed.Toggleable ?? false,
      Order = this._features.Count,
    };
    owner(feature);
    this._features[seed.Key] = feature;
    this._result.Features.Add(feature);

    int index = 0;
    foreach (SeedEffect effect in seed.Effects)
    {
      feature.Effects.Add(this.BuildEffect(effect, $"feature:{seed.Key}#{index++}", feature.Id, null));
    }

    foreach (SeedChoice choiceSeed in seed.Choices)
    {
      Choice choice = new()
      {
        Id = SeedKeys.Id("choice", choiceSeed.Key),
        Key = choiceSeed.Key,
        Name = choiceSeed.Name,
        FeatureId = feature.Id,
        CountExpr = choiceSeed.Count,
        OptionSource = choiceSeed.Source,
        Category = choiceSeed.Category,
        FilterExpr = choiceSeed.Filter,
        ReplacePolicy = choiceSeed.Replace,
        AllowDuplicates = choiceSeed.AllowDuplicates,
        MaxPerOption = choiceSeed.MaxPerOption,
        Optional = choiceSeed.Optional,
      };
      feature.Choices.Add(choice);

      foreach (SeedOption optionSeed in choiceSeed.Options)
      {
        ChoiceOption option = new()
        {
          Id = SeedKeys.Id("option", $"{choiceSeed.Key}:{optionSeed.Key}"),
          ChoiceId = choice.Id,
          Key = optionSeed.Key,
          Name = optionSeed.Name,
          Value = optionSeed.Value,
          PrerequisiteExpr = optionSeed.Prerequisite,
        };
        choice.Options.Add(option);

        if (optionSeed.Feature != null)
        {
          optionSeed.Feature.Name ??= optionSeed.Name;
          Feature granted = this.BuildFeature(optionSeed.Feature, OwnerTypeEnum.Option, defaultSection, _ => { }, @class);
          option.GrantsFeatureId = granted.Id;
          option.GrantsFeature = granted;
        }
        else if (optionSeed.Grants != null)
        {
          this._pendingGrants.Add((option, optionSeed.Grants));
        }
      }

      int effectIndex = 0;
      foreach (SeedEffect effect in choiceSeed.Effects)
      {
        choice.Effects.Add(this.BuildEffect(effect, $"choice:{choiceSeed.Key}#{effectIndex++}", null, choice.Id));
      }
    }

    foreach (SeedResource resource in seed.Resources)
    {
      feature.Resources.Add(new ResourceDefinition
      {
        Id = SeedKeys.Id("resource", resource.Key),
        Key = resource.Key,
        Name = resource.Name,
        FeatureId = feature.Id,
        MaxExpr = resource.Max,
        DieExpr = resource.Die,
        Recharge = resource.Recharge,
        ShortRestExpr = resource.ShortRest,
      });
    }

    if (seed.Spellcasting is { } spellcasting)
    {
      SpellcastingProfile profile = new()
      {
        Id = SeedKeys.Id("spellcasting", seed.Key),
        FeatureId = feature.Id,
        Ability = spellcasting.Ability,
        Progression = spellcasting.Progression,
        SlotTableKey = spellcasting.SlotTable,
        SpellListKey = spellcasting.SpellList,
        PreparedCountKey = spellcasting.Prepared,
        CantripCountKey = spellcasting.Cantrips,
        Pool = spellcasting.Pool,
        PrepareChange = spellcasting.PrepareChange,
        RitualMode = spellcasting.Ritual,
        Focus = spellcasting.Focus,
      };
      feature.Spellcasting = profile;
      this._pendingProfiles.Add((profile, spellcasting.Class ?? @class?.Key, feature));
    }

    return feature;
  }

  private Effect BuildEffect(SeedEffect seed, string identity, Guid? featureId, Guid? choiceId) => new()
  {
    Id = SeedKeys.Id("effect", identity),
    FeatureId = featureId,
    ChoiceId = choiceId,
    Target = seed.Target,
    Operation = seed.Op,
    ValueExpr = seed.Value,
    ConditionExpr = seed.Condition,
    StackGroup = seed.StackGroup,
    Label = seed.Label,
  };

  private Item BuildItem(SeedItem seed)
  {
    Item item = (seed.Kind ?? ItemKindEnum.Gear) switch
    {
      ItemKindEnum.Weapon => new Weapon
      {
        Key = seed.Key,
        Name = seed.Name ?? seed.Key,
        Category = seed.WeaponCategory ?? WeaponCategoryEnum.Simple,
        Ranged = seed.Ranged ?? false,
        Damage = seed.Damage ?? "1",
        DamageType = seed.DamageType ?? DamageTypeEnum.Bludgeoning,
        Properties = seed.Properties ?? WeaponPropertyEnum.None,
        VersatileDamage = seed.Versatile,
        RangeNormal = seed.RangeNormal,
        RangeLong = seed.RangeLong,
        MasteryId = seed.Mastery == null ? null : this._result.Masteries.FirstOrDefault(entry => entry.Key == seed.Mastery)?.Id,
      },
      ItemKindEnum.Armor => new Armor
      {
        Key = seed.Key,
        Name = seed.Name ?? seed.Key,
        Category = seed.ArmorCategory ?? ArmorEnum.Light,
        BaseAc = seed.BaseAc ?? 10,
        DexCap = seed.DexCap,
        StrengthRequirement = seed.StrengthRequirement,
        StealthDisadvantage = seed.StealthDisadvantage ?? false,
      },
      ItemKindEnum.Tool => new Tool { Key = seed.Key, Name = seed.Name ?? seed.Key, Group = seed.ToolGroup ?? "other" },
      _ => new Item { Key = seed.Key, Name = seed.Name ?? seed.Key },
    };

    item.Kind = seed.Kind ?? ItemKindEnum.Gear;
    item.Group ??= seed.Category;
    item.CostCp = seed.Cost ?? 0;
    item.Weight = seed.Weight ?? 0;
    item.Bundle = seed.Bundle ?? 1;
    item.Magic = seed.Magic ?? false;
    item.RequiresAttunement = seed.Attunement ?? false;
    this.Rule(item, "item", seed);

    foreach (SeedFeature feature in seed.Features)
    {
      item.Features.Add(this.BuildFeature(feature, OwnerTypeEnum.Item, SheetSectionEnum.Equipment, entity => entity.ItemId = item.Id));
    }

    return item;
  }

  /// <summary>
  ///   Starting equipment with item keys checked and picks resolved to the items of their groups
  /// </summary>
  private List<StartingEquipmentOption> BuildStartingEquipment(string owner, List<SeedStartingEquipment>? seeds)
  {
    List<StartingEquipmentOption> options = new();
    foreach (SeedStartingEquipment seed in seeds ?? [])
    {
      StartingEquipmentOption option = new() { Key = seed.Key, CoinsCp = seed.Gold * 100 };
      foreach (SeedStartingItem entry in seed.Items)
      {
        List<string> choose = entry.Choose ?? [];
        string key = entry.Item ?? string.Join("-or-", choose);
        if (option.Items.Any(existing => existing.Key == key))
        {
          key = $"{key}-{option.Items.Count + 1}";
        }

        if (entry.Item != null && this._result.Items.All(item => item.Key != entry.Item))
        {
          this._errors.Add($"{owner}: starting equipment item '{entry.Item}' doesn't exist");
        }

        List<string> picks = this._result.Items
          .Where(item => item.Group != null && choose.Contains(item.Group))
          .OrderBy(item => item.Name, StringComparer.Ordinal)
          .Select(item => item.Key)
          .ToList();
        if ((entry.Item == null) == (choose.Count == 0) || (choose.Count > 0 && picks.Count == 0))
        {
          this._errors.Add($"{owner}: starting equipment entry '{key}' needs an item or groups with items to choose from");
        }

        if (entry.SameAs != null)
        {
          this._pendingSameAs.Add((owner, entry.SameAs));
        }

        option.Items.Add(new StartingEquipmentEntry
        {
          Key = key,
          Name = entry.Name,
          ItemKey = entry.Item,
          Options = picks,
          SameAs = entry.SameAs,
          Quantity = entry.Quantity,
          Note = entry.Note,
        });
      }

      options.Add(option);
    }

    return options;
  }

  private void ResolveReferences()
  {
    foreach ((string owner, string choiceKey) in this._pendingSameAs)
    {
      if (this._features.Values
        .SelectMany(feature => feature.Choices)
        .All(choice => choice.Key != choiceKey))
      {
        this._errors.Add($"{owner}: starting equipment refers to the unknown choice '{choiceKey}'");
      }
    }

    foreach ((ChoiceOption option, string key) in this._pendingGrants)
    {
      if (this._features.TryGetValue(key, out Feature? feature))
      {
        option.GrantsFeatureId = feature.Id;
        option.GrantsFeature = feature;
      }
      else
      {
        this._errors.Add($"option '{option.Key}': granted feature '{key}' doesn't exist");
      }
    }

    foreach ((SpellcastingProfile profile, string? classKey, Feature feature) in this._pendingProfiles)
    {
      CharacterClass? @class = this._result.Classes.FirstOrDefault(entry => entry.Key == classKey);
      if (@class == null)
      {
        this._errors.Add($"feature '{feature.Key}': spellcasting class '{classKey}' doesn't exist");
        continue;
      }

      profile.ClassId = @class.Id;
    }
  }

  private T Rule<T>(T entity, string type, SeedNamed seed) where T : RuleEntity
  {
    entity.Id = SeedKeys.Id(type, seed.Key);
    entity.DescriptionHtml = seed.Description ?? entity.DescriptionHtml;
    entity.SourceId = this._result.Source.Id;
    return entity;
  }

  #endregion
}
