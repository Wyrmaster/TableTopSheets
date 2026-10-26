using System.Globalization;
using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Sheet;

/// <summary>
///   Turns a <see cref="CharacterEvaluation"/> into the printable <see cref="CharacterSheet"/>
/// </summary>
public static class CharacterSheetBuilder
{
  #region Public Methods

  public static CharacterSheet Build(CharacterEvaluation evaluation)
  {
    Character character = evaluation.Character;
    List<ClassLevelSheet> classLevels = ClassLevels(evaluation);
    (int armorClass, List<SourceValue> armorClassOptions, List<SourceValue> armorClassSources) = ArmorClass(evaluation);

    return new CharacterSheet
    {
      Name = character.Name,
      Background = character.BackgroundId is { } backgroundId ? evaluation.Catalog.Backgrounds[backgroundId].Name : string.Empty,
      Species = character.SpeciesId is { } speciesId ? evaluation.Catalog.Species[speciesId].Name : string.Empty,
      Classes = string.Join(" / ", classLevels.Select(entry => $"{entry.Class} {entry.Level}")),
      Subclasses = string.Join(" / ", classLevels.Where(entry => entry.Subclass != null).Select(entry => entry.Subclass)),
      ClassLevels = classLevels,
      Level = evaluation.TotalLevel,
      Experience = character.Experience,
      ProficiencyBonus = evaluation.ProficiencyBonus,
      Abilities = Abilities(evaluation),

      ArmorClass = armorClass,
      ArmorClassOptions = armorClassOptions,
      ArmorClassSources = armorClassSources,
      ShieldEquipped = evaluation.WieldedShield != null,
      HitPointMaximum = HitPointMaximum(evaluation),
      CurrentHitPoints = character.PlayState.CurrentHitPoints,
      TemporaryHitPoints = character.PlayState.TemporaryHitPoints,
      HitDice = classLevels
        .GroupBy(entry => entry.HitDie)
        .Select(group => new DicePool(group.Key, group.Sum(entry => entry.Level)))
        .OrderByDescending(entry => entry.Die)
        .ToList(),
      Initiative = evaluation.Modifier("dex") + (int)evaluation.Stat("initiative.bonus"),
      Speed = Speed(evaluation),
      OtherSpeeds = new[] { "fly", "swim", "climb", "burrow" }
        .Select(entry => new SourceValue(Naming.Title(entry), (int)evaluation.Stat($"speed.{entry}")))
        .Where(entry => entry.Value > 0)
        .ToList(),
      Size = (SizeEnum)(int)evaluation.Stat("size"),
      PassivePerception = 10 + evaluation.SkillBonus("perception") + (int)evaluation.Stat("passive.perception.bonus"),
      Senses = new[] { "darkvision", "blindsight", "tremorsense", "truesight" }
        .Select(entry => new SourceValue(Naming.Title(entry), (int)evaluation.Stat($"sense.{entry}")))
        .Where(entry => entry.Value > 0)
        .ToList(),
      AttacksPerAction = 1 + (int)evaluation.Stat("attack.extraAttacks"),
      Attacks = Attacks(evaluation),
      Resistances = Names(evaluation, "damage.resist."),
      Immunities = Names(evaluation, "damage.immune.").Concat(Names(evaluation, "condition.immune.")).ToList(),
      Vulnerabilities = Names(evaluation, "damage.vulnerable."),

      ArmorTraining = ArmorTraining(evaluation),
      WeaponProficiencies = evaluation.StatKeysWithPrefix("proficiency.weapon.")
        .Select(key => key["proficiency.weapon.".Length..])
        .Select(key => key switch
        {
          "simple" => "Simple Weapons",
          "martial" => "Martial Weapons",
          _ => ItemName(evaluation, key),
        })
        .ToList(),
      WeaponMasteries = evaluation.StatKeysWithPrefix("mastery.").Select(key => ItemName(evaluation, key["mastery.".Length..])).ToList(),
      ToolProficiencies = evaluation.StatKeysWithPrefix("proficiency.tool.")
        .Select(key => ItemName(evaluation, key["proficiency.tool.".Length..]) + (evaluation.Stat(key) >= 20 ? " (Expertise)" : string.Empty))
        .ToList(),
      Languages = evaluation.StatKeysWithPrefix("language.")
        .Select(key => key["language.".Length..])
        .Select(key => evaluation.Catalog.Languages.TryGetValue(key, out Language? language) ? language.Name : Naming.Title(key))
        .ToList(),

      Spellcasting = Spellcasting(evaluation),
      SpellSlots = SpellSlots(evaluation),
      Spells = Spells(evaluation),

      ClassFeatures = Features(evaluation, SheetSectionEnum.ClassFeatures),
      SpeciesTraits = Features(evaluation, SheetSectionEnum.SpeciesTraits),
      Feats = Features(evaluation, SheetSectionEnum.Feats),
      Resources = Resources(evaluation),

      Equipment = character.Items
        .Select(entry => (Item: evaluation.Catalog.Items[entry.ItemId], entry))
        .Select(entry => entry.Item.Name
                         + (entry.entry.Quantity > 1 ? $" ×{entry.entry.Quantity}" : string.Empty)
                         + (entry.entry.Equipped ? " (equipped)" : string.Empty))
        .ToList(),
      AttunedItems = character.Items.Where(entry => entry.Attuned).Select(entry => evaluation.Catalog.Items[entry.ItemId].Name).ToList(),
      Coins = new Coins
      {
        Copper = character.PlayState.CopperPieces,
        Silver = character.PlayState.SilverPieces,
        Electrum = character.PlayState.ElectrumPieces,
        Gold = character.PlayState.GoldPieces,
        Platinum = character.PlayState.PlatinumPieces,
      },

      Violations = ViolationChecker.Check(evaluation),
    };
  }

  #endregion

  #region Sections

  private static List<ClassLevelSheet> ClassLevels(CharacterEvaluation evaluation) => evaluation.Levels
    .GroupBy(entry => entry.ClassId)
    .Select(group =>
    {
      CharacterClass @class = evaluation.Catalog.Classes[group.Key];
      return new ClassLevelSheet(@class.Name, evaluation.Subclasses.GetValueOrDefault(group.Key)?.Name, group.Count(), @class.HitDie);
    })
    .ToList();

  private static List<AbilitySheet> Abilities(CharacterEvaluation evaluation)
  {
    Armor? armor = evaluation.WornArmor;
    bool untrainedArmor = armor != null && !evaluation.HasArmorTraining(armor.Category);

    return RuleKeys.Abilities
      .Select(ability => new AbilitySheet
      (
        ability,
        Naming.Title(ability),
        evaluation.Score(ability),
        evaluation.Modifier(ability),
        evaluation.SaveProficiency(ability),
        evaluation.SaveBonus(ability),
        RuleKeys.Skills
          .Where(skill => RuleKeys.SkillAbilityKey(skill) == ability)
          .Select(skill => new SkillSheet
          (
            skill,
            Naming.Title(skill),
            evaluation.SkillProficiency(skill),
            evaluation.SkillBonus(skill),
            evaluation.Stat($"skill.{skill}.advantage") > 0 || evaluation.Stat("skill.all.advantage") > 0,
            evaluation.Stat($"skill.{skill}.disadvantage") > 0
            || (skill == "stealth" && armor?.StealthDisadvantage == true)
            || (untrainedArmor && ability is "str" or "dex")
          ))
          .ToList()
      ))
      .ToList();
  }

  /// <summary>
  ///   Only one base Armor Class calculation (<c>ac.base</c> candidate) applies at a time: armor, unarmored or a feature formula
  ///   (Unarmored Defense, Mage Armor ...). Shields and other bonuses are <c>ac.bonus</c> effects; all of them are rule data
  /// </summary>
  private static (int ArmorClass, List<SourceValue> Options, List<SourceValue> Sources) ArmorClass(CharacterEvaluation evaluation)
  {
    List<SourceValue> options = evaluation.Candidates("ac.base").Select(entry => new SourceValue(entry.Label, (int)entry.Value)).ToList();

    SourceValue? @base = options.FirstOrDefault(entry => entry.Label == evaluation.Character.ArmorClassChoice)
                         ?? options.MaxBy(entry => entry.Value);
    List<SourceValue> sources = @base == null ? [] : [@base];
    sources.AddRange(evaluation.Sources("ac.bonus").Select(entry => new SourceValue(entry.Label, (int)entry.Value)));
    return (sources.Sum(entry => entry.Value), options, sources);
  }

  /// <summary>
  ///   Level 1 of the first class grants the maximum of the hit die, every other level the fixed average or the stored roll.
  ///   The Constitution modifier applies to every level retroactively
  /// </summary>
  private static int HitPointMaximum(CharacterEvaluation evaluation)
  {
    if (evaluation.TotalLevel == 0)
    {
      return 0;
    }

    int total = 0;
    foreach (CharacterLevel level in evaluation.Levels)
    {
      int die = (int)evaluation.Catalog.Classes[level.ClassId].HitDie;
      total += level.Number == 1 ? die : level.HitPointRoll ?? die / 2 + 1;
    }

    int perLevel = evaluation.Modifier("con") + (int)evaluation.Stat("hp.perLevel");
    return Math.Max(evaluation.TotalLevel, total + perLevel * evaluation.TotalLevel + (int)evaluation.Stat("hp.bonus"));
  }

  private static int Speed(CharacterEvaluation evaluation)
  {
    int speed = (int)evaluation.Stat("speed.walk");
    if (evaluation.WornArmor is { StrengthRequirement: { } strength } && evaluation.Score("str") < strength)
    {
      speed -= 10;
    }

    return Math.Max(0, speed);
  }

  private static List<AttackSheet> Attacks(CharacterEvaluation evaluation)
  {
    List<AttackSheet> attacks = new();
    int strength = evaluation.Modifier("str");
    int dexterity = evaluation.Modifier("dex");
    bool meleeDexterity = evaluation.Stat("attack.melee.dex") > 0;

    // equipped weapons only: ready ones and the ones in hand
    foreach (Weapon weapon in evaluation.EquippedItems.Select(entry => evaluation.Catalog.Items[entry.ItemId]).OfType<Weapon>().DistinctBy(entry => entry.Id))
    {
      bool finesse = weapon.Properties.HasFlag(WeaponPropertyEnum.Finesse);
      int modifier = weapon.Ranged
        ? dexterity
        : finesse || meleeDexterity ? Math.Max(strength, dexterity) : strength;
      bool proficient = evaluation.IsProficient(weapon);
      int attackBonus = modifier
                        + (proficient ? evaluation.ProficiencyBonus : 0)
                        + (int)evaluation.Stat("attack.bonus")
                        + (int)evaluation.Stat(weapon.Ranged ? "attack.ranged.bonus" : "attack.melee.bonus");
      int damageBonus = modifier
                        + (int)evaluation.Stat("damage.bonus")
                        + (int)evaluation.Stat(weapon.Ranged ? "damage.ranged.bonus" : "damage.melee.bonus");

      List<string> notes = new();
      if (weapon.VersatileDamage != null)
      {
        notes.Add($"Versatile ({weapon.VersatileDamage})");
      }

      if (weapon.RangeNormal != null)
      {
        notes.Add($"Range {weapon.RangeNormal}/{weapon.RangeLong}");
      }

      notes.AddRange(PropertyNames(weapon.Properties & ~(WeaponPropertyEnum.Versatile | WeaponPropertyEnum.Range)));
      if (weapon.MasteryId is { } masteryId && evaluation.Stat($"mastery.{weapon.Key}") > 0)
      {
        notes.Add($"Mastery: {evaluation.Catalog.Masteries[masteryId].Name}");
      }

      if (!proficient)
      {
        notes.Add("not proficient");
      }

      attacks.Add(new(weapon.Name, attackBonus, Damage(weapon.Damage, damageBonus), Dice(weapon.Damage), damageBonus, weapon.DamageType, string.Join(", ", notes)));
    }

    // unarmed strike: 1 + Strength, or the Martial Arts die
    int unarmedDie = (int)evaluation.Stat("attack.unarmed.die");
    int unarmedModifier = meleeDexterity ? Math.Max(strength, dexterity) : strength;
    int unarmedDamage = unarmedModifier + (int)evaluation.Stat("damage.bonus") + (int)evaluation.Stat("damage.melee.bonus");
    string unarmedDice = unarmedDie > 0 ? $"1d{unarmedDie}" : "1";
    attacks.Add(new
    (
      "Unarmed Strike",
      unarmedModifier + evaluation.ProficiencyBonus + (int)evaluation.Stat("attack.bonus") + (int)evaluation.Stat("attack.melee.bonus") + (int)evaluation.Stat("attack.unarmed.bonus"),
      Damage(unarmedDice, unarmedDamage),
      Dice(unarmedDice),
      unarmedDamage,
      DamageTypeEnum.Bludgeoning,
      string.Empty
    ));

    attacks.AddRange(CantripAttacks(evaluation));
    return attacks;
  }

  /// <summary>
  ///   Damaging cantrips the character can cast; their damage grows with the character level (not the class level)
  /// </summary>
  private static IEnumerable<AttackSheet> CantripAttacks(CharacterEvaluation evaluation)
  {
    List<KnownSpell> cantrips = evaluation.KnownSpells().Where(entry => entry.Spell.Level == 0 && (entry.Prepared || entry.AlwaysPrepared)).ToList();
    foreach (KnownSpell known in cantrips)
    {
      string? damage = evaluation.CantripDamage(known.Spell);
      if (damage == null)
      {
        continue;
      }

      int modifier = evaluation.Modifier(known.Ability);
      int bonus = (int)evaluation.Stat($"damage.spell.{known.Spell.Key}");
      int? saveDc = known.Spell.SaveAbility == null || known.Spell.AttackRoll
        ? null
        : 8 + evaluation.ProficiencyBonus + modifier + (int)evaluation.Stat("spellcasting.dc.bonus");

      List<string> notes = [known.Spell.Range];
      if (known.Spell.DamageType == null)
      {
        notes.Add("damage type of your choice");
      }

      if (saveDc != null)
      {
        notes.Add($"{Naming.Title(known.Spell.SaveAbility!)} save");
      }

      // the same cantrip learned through two classes can use different abilities, so both rows stay
      bool duplicate = cantrips.Count(entry => entry.Spell.Id == known.Spell.Id) > 1;
      yield return new AttackSheet
      (
        duplicate ? $"{known.Spell.Name} ({known.Source})" : known.Spell.Name,
        evaluation.ProficiencyBonus + modifier + (int)evaluation.Stat("spellcasting.attack.bonus"),
        damage.Contains('×') && bonus != 0 ? $"{damage.Split('×')[0].Trim()} × ({Damage(damage.Split('×')[1].Trim(), bonus)})" : Damage(damage, bonus),
        SpellDice(damage),
        bonus,
        known.Spell.DamageType,
        string.Join(", ", notes),
        saveDc,
        known.Spell.SaveAbility
      );
    }
  }

  /// <summary>
  ///   <c>2 × 1d10</c> → [D10, D10]; <c>2d8 or 2d12</c> → [D8, D8]
  /// </summary>
  private static List<DiceEnum> SpellDice(string damage)
  {
    string[] beams = damage.Split(" or ")[0].Split('×', StringSplitOptions.TrimEntries);
    int count = beams.Length == 2 ? int.Parse(beams[0], CultureInfo.InvariantCulture) : 1;
    List<DiceEnum> dice = Dice(beams[^1]);
    return Enumerable.Range(0, count).SelectMany(_ => dice).ToList();
  }

  private static List<SpellcastingSheet> Spellcasting(CharacterEvaluation evaluation) => evaluation.SpellcastingProfiles
    .Select(profile =>
    {
      int modifier = evaluation.Modifier(profile.Ability);
      return new SpellcastingSheet
      (
        evaluation.Catalog.Classes[profile.ClassId].Name,
        Naming.Title(profile.Ability),
        modifier,
        8 + evaluation.ProficiencyBonus + modifier + (int)evaluation.Stat("spellcasting.dc.bonus"),
        evaluation.ProficiencyBonus + modifier + (int)evaluation.Stat("spellcasting.attack.bonus"),
        profile.PreparedCountKey == null ? 0 : (int)evaluation.Scale(profile.PreparedCountKey).AsNumber(),
        profile.CantripCountKey == null ? 0 : (int)evaluation.Scale(profile.CantripCountKey).AsNumber()
      );
    })
    .ToList();

  private static List<SpellSlot> SpellSlots(CharacterEvaluation evaluation)
  {
    List<SpellSlot> slots = new();
    int[] spellSlots = evaluation.SpellSlots();
    for (int level = 1; level < spellSlots.Length; level++)
    {
      for (int count = 0; count < spellSlots[level]; count++)
      {
        slots.Add(new SpellSlot { Level = (byte)level, RefreshPeriod = PeriodEnum.LongRest });
      }
    }

    (int pactCount, int pactLevel) = evaluation.PactSlots();
    for (int count = 0; count < pactCount; count++)
    {
      slots.Add(new SpellSlot { Level = (byte)pactLevel, RefreshPeriod = PeriodEnum.ShortRest });
    }

    return slots;
  }

  private static List<SpellSheet> Spells(CharacterEvaluation evaluation) => evaluation.KnownSpells()
    .Where(entry => entry.Prepared || entry.AlwaysPrepared || (entry.InSpellbook && entry.Spell.Ritual && entry.Profile?.RitualMode == RitualModeEnum.FromSpellbook))
    .Select(entry =>
    {
      List<string> notes = new();
      if (entry.AlwaysPrepared && entry.Spell.Level > 0)
      {
        notes.Add("always prepared");
      }

      if (!entry.Prepared && entry.InSpellbook)
      {
        notes.Add("ritual from spellbook");
      }

      return new SpellSheet
      (
        entry.Spell.Key,
        entry.Spell.Name,
        entry.Spell.Level,
        entry.Spell.CastingTime,
        entry.Spell.Range,
        entry.Spell.Concentration,
        entry.Spell.Ritual,
        entry.Spell.Components.Contains('M'),
        entry.Source,
        string.Join(", ", notes)
      );
    })
    .ToList();

  private static List<FeatureSheet> Features(CharacterEvaluation evaluation, SheetSectionEnum section) => evaluation.Features
    .Where(entry => entry.Feature.SheetSection == section)
    .Select(entry =>
    {
      string description = HtmlText.ToPlain(entry.Feature.DescriptionHtml);
      List<string> choices = evaluation.Selections
        .Where(selection => selection.Owner == entry)
        .Select(selection => evaluation.OptionName(selection.Choice, selection.Selection.OptionKey))
        .ToList();
      string name = entry.Feature.Name
                    + (choices.Count > 0 ? $" ({string.Join(", ", choices)})" : string.Empty)
                    + (entry.Feature.Toggleable ? entry.Active ? " [active]" : " [inactive]" : string.Empty);
      return new FeatureSheet(entry.Feature.Key, name, entry.Source, description);
    })
    .ToList();

  private static List<ResourceSheet> Resources(CharacterEvaluation evaluation) => evaluation.Features
    .SelectMany(entry => entry.Feature.Resources)
    .DistinctBy(entry => entry.Id)
    .Select(resource => new ResourceSheet
    (
      resource.Key,
      resource.Name,
      (int)Expression.Parse(resource.MaxExpr).EvaluateNumber(evaluation),
      evaluation.Character.ResourceUsages.FirstOrDefault(entry => entry.Key == $"resource.{resource.Key}")?.Used ?? 0,
      resource.DieExpr == null ? null : Expression.Parse(resource.DieExpr).Evaluate(evaluation).AsString(),
      resource.Recharge.ToString()
    ))
    .ToList();

  #endregion

  #region Helpers

  private static ArmorEnum ArmorTraining(CharacterEvaluation evaluation)
  {
    ArmorEnum training = 0;
    foreach (ArmorEnum category in new[] { ArmorEnum.Light, ArmorEnum.Medium, ArmorEnum.Heavy, ArmorEnum.Shield })
    {
      if (evaluation.HasArmorTraining(category))
      {
        training |= category;
      }
    }

    return training;
  }

  private static List<string> Names(CharacterEvaluation evaluation, string prefix)
    => evaluation.StatKeysWithPrefix(prefix).Select(key => Naming.Title(key[prefix.Length..])).ToList();

  private static string ItemName(CharacterEvaluation evaluation, string key)
    => evaluation.Catalog.ItemsByKey.TryGetValue(key, out Item? item) ? item.Name : Naming.Title(key);

  private static IEnumerable<string> PropertyNames(WeaponPropertyEnum properties)
    => System.Enum.GetValues<WeaponPropertyEnum>()
      .Where(entry => entry != WeaponPropertyEnum.None && properties.HasFlag(entry))
      .Select(entry => entry == WeaponPropertyEnum.TwoHanded ? "Two-Handed" : entry.ToString());

  private static string Damage(string dice, int bonus)
    => bonus switch
    {
      > 0 => $"{dice}+{bonus}",
      < 0 => $"{dice}{bonus}",
      _ => dice,
    };

  /// <summary>
  ///   <c>2d6</c> → [D6, D6]; a flat value (<c>1</c>) → [D1]
  /// </summary>
  private static List<DiceEnum> Dice(string dice)
  {
    ExprValue value = ExprValue.Parse(dice);
    return value.Kind == ExprValueKind.Dice
      ? Enumerable.Repeat((DiceEnum)value.DiceSides, value.DiceCount).ToList()
      : Enumerable.Repeat(DiceEnum.D1, Math.Max(1, (int)value.AsNumber())).ToList();
  }

  #endregion
}
