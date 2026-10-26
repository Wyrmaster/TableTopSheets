using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;
using TableTopSheets.Common.Enum.Magic;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Seed.Model;

// Seed files are authored by hand (Data/Rules) or generated from the rule book (Data/Extracted).
// Records with the same key are merged: authored values win, extracted values fill the gaps (mostly descriptions).

/// <summary>
///   Content of one seed file; every section is optional
/// </summary>
public sealed class SeedDocument
{
  public List<SeedClass> Classes { get; set; } = [];

  public List<SeedSpecies> Species { get; set; } = [];

  public List<SeedBackground> Backgrounds { get; set; } = [];

  public List<SeedFeat> Feats { get; set; } = [];

  /// <summary>
  ///   Selectable option features (invocations, metamagic ...) and toggleable spell effects
  /// </summary>
  public List<SeedFeature> Features { get; set; } = [];

  /// <summary>
  ///   Descriptions of class features from the rule book
  /// </summary>
  public List<SeedClassFeatureText> ClassFeatureTexts { get; set; } = [];

  public List<SeedSpell> Spells { get; set; } = [];

  public List<SeedItem> Items { get; set; } = [];

  public List<SeedNamed> Masteries { get; set; } = [];

  public List<SeedLanguage> Languages { get; set; } = [];

  /// <summary>
  ///   Spell slot tables: table key → rows per caster level (index 0 = level 1) → slots per spell level (index 0 = level 1)
  /// </summary>
  public Dictionary<string, List<List<int>>> SlotTables { get; set; } = [];

  /// <summary>
  ///   Tables that follow the character level instead of a class level (species traits, feats):
  ///   key → 20 comma separated values for level 1..20
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];
}

public class SeedNamed
{
  public required string Key { get; set; }

  public string? Name { get; set; }

  public string? Description { get; set; }
}

public sealed class SeedLanguage: SeedNamed
{
  public bool Rare { get; set; }
}

public sealed class SeedClass: SeedNamed
{
  public DiceEnum? HitDie { get; set; }

  public List<string>? PrimaryAbilities { get; set; }

  public PrimaryAbilityModeEnum? PrimaryAbilityMode { get; set; }

  public byte? SubclassLevel { get; set; }

  /// <summary>
  ///   Class table columns: key → 20 comma separated values for level 1..20 (<c>3,3,3,4,...</c>)
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];

  public List<SeedFeature> Features { get; set; } = [];

  public List<SeedSubclass> Subclasses { get; set; } = [];

  /// <summary>
  ///   Starting equipment alternatives of a character whose first class this is
  /// </summary>
  public List<SeedStartingEquipment>? StartingEquipment { get; set; }
}

public sealed class SeedSubclass: SeedNamed
{
  /// <summary>
  ///   Scale columns of the subclass (counted with the level of the class)
  /// </summary>
  public Dictionary<string, string> Scales { get; set; } = [];

  public List<SeedFeature> Features { get; set; } = [];
}

public sealed class SeedSpecies: SeedNamed
{
  public string? CreatureType { get; set; }

  public List<SeedFeature> Features { get; set; } = [];
}

public sealed class SeedBackground: SeedNamed
{
  public List<string>? Abilities { get; set; }

  public string? OriginFeat { get; set; }

  public List<SeedFeature> Features { get; set; } = [];

  public List<SeedStartingEquipment>? StartingEquipment { get; set; }
}

/// <summary>
///   One alternative of the starting equipment ("(A) Spear, 2 Daggers ..., 28 GP")
/// </summary>
public sealed class SeedStartingEquipment
{
  /// <summary>
  ///   Letter of the alternative (<c>a</c>, <c>b</c> ...)
  /// </summary>
  public required string Key { get; set; }

  public List<SeedStartingItem> Items { get; set; } = [];

  /// <summary>
  ///   Gold pieces of the alternative
  /// </summary>
  public int Gold { get; set; }
}

/// <summary>
///   A fixed item (<c>item</c>) or a pick from item groups (<c>choose</c>: <c>holy-symbol</c>, <c>artisan</c> ...)
/// </summary>
public sealed class SeedStartingItem
{
  public string? Item { get; set; }

  /// <summary>
  ///   Item groups (tool groups or gear categories) to pick the item from
  /// </summary>
  public List<string>? Choose { get; set; }

  /// <summary>
  ///   Name of a pick (<c>Holy Symbol</c>)
  /// </summary>
  public string? Name { get; set; }

  /// <summary>
  ///   Choice whose selection is the pick ("same as above")
  /// </summary>
  public string? SameAs { get; set; }

  public int Quantity { get; set; } = 1;

  public string? Note { get; set; }
}

public sealed class SeedFeat: SeedNamed
{
  public FeatCategoryEnum? Category { get; set; }

  public bool? Repeatable { get; set; }

  public string? Prerequisite { get; set; }

  public List<SeedFeature> Features { get; set; } = [];
}

public sealed class SeedFeature: SeedNamed
{
  public byte? Level { get; set; }

  public GrantModeEnum? GrantMode { get; set; }

  public OwnerTypeEnum? OwnerType { get; set; }

  public string? Category { get; set; }

  public SheetSectionEnum? SheetSection { get; set; }

  public string? Prerequisite { get; set; }

  public bool? Toggleable { get; set; }

  /// <summary>
  ///   Spell the feature belongs to (<c>OwnerType.Spell</c>)
  /// </summary>
  public string? Spell { get; set; }

  public List<SeedEffect> Effects { get; set; } = [];

  public List<SeedChoice> Choices { get; set; } = [];

  public List<SeedResource> Resources { get; set; } = [];

  public SeedSpellcasting? Spellcasting { get; set; }
}

public sealed class SeedEffect
{
  public required string Target { get; set; }

  public EffectOperationEnum Op { get; set; }

  public string Value { get; set; } = "1";

  public string? Condition { get; set; }

  public string? StackGroup { get; set; }

  public string? Label { get; set; }
}

public sealed class SeedChoice
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string Count { get; set; } = "1";

  public OptionSourceEnum Source { get; set; }

  public string? Category { get; set; }

  public string? Filter { get; set; }

  public ReplacePolicyEnum Replace { get; set; }

  public bool AllowDuplicates { get; set; }

  public byte MaxPerOption { get; set; } = 1;

  public bool Optional { get; set; }

  public List<SeedOption> Options { get; set; } = [];

  public List<SeedEffect> Effects { get; set; } = [];
}

public sealed class SeedOption
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string? Value { get; set; }

  /// <summary>
  ///   Key of an existing feature granted by the option
  /// </summary>
  public string? Grants { get; set; }

  /// <summary>
  ///   Inline feature granted by the option (created as <c>OwnerType.Option</c>)
  /// </summary>
  public SeedFeature? Feature { get; set; }

  public string? Prerequisite { get; set; }
}

public sealed class SeedResource
{
  public required string Key { get; set; }

  public required string Name { get; set; }

  public string Max { get; set; } = "1";

  public string? Die { get; set; }

  public PeriodEnum Recharge { get; set; } = PeriodEnum.LongRest;
}

public sealed class SeedSpellcasting
{
  public required string Ability { get; set; }

  public CasterProgressionEnum Progression { get; set; }

  public string? SlotTable { get; set; }

  public required string SpellList { get; set; }

  public string? Prepared { get; set; }

  public string? Cantrips { get; set; }

  public SpellPoolEnum Pool { get; set; }

  public PrepareChangeEnum PrepareChange { get; set; }

  public RitualModeEnum Ritual { get; set; }

  public string? Focus { get; set; }

  /// <summary>
  ///   Class the profile counts for (defaults to the owning class)
  /// </summary>
  public string? Class { get; set; }
}

public sealed class SeedClassFeatureText
{
  public required string Class { get; set; }

  public byte Level { get; set; }

  public required string Name { get; set; }

  public required string Description { get; set; }
}

public sealed class SeedSpell: SeedNamed
{
  public byte? Level { get; set; }

  public SpellSchoolEnum? School { get; set; }

  public string? CastingTime { get; set; }

  public bool? Ritual { get; set; }

  public string? Range { get; set; }

  public string? Components { get; set; }

  public string? Material { get; set; }

  public bool? Concentration { get; set; }

  public string? Duration { get; set; }

  public List<string>? Lists { get; set; }

  public string? Damage { get; set; }

  public DamageTypeEnum? DamageType { get; set; }

  public bool? AttackRoll { get; set; }

  public string? SaveAbility { get; set; }

  public List<string>? CantripDamage { get; set; }
}

public sealed class SeedItem: SeedNamed
{
  public ItemKindEnum? Kind { get; set; }

  /// <summary>
  ///   Cost in copper pieces
  /// </summary>
  public int? Cost { get; set; }

  public double? Weight { get; set; }

  public bool? Magic { get; set; }

  public bool? Attunement { get; set; }

  /// <summary>
  ///   Gear category (<c>ammunition</c>, <c>arcane-focus</c>, <c>mount</c> ...); tools use <see cref="ToolGroup"/>
  /// </summary>
  public string? Category { get; set; }

  /// <summary>
  ///   Pieces sold together for <see cref="Cost"/> (20 arrows); cost and weight are per bundle
  /// </summary>
  public int? Bundle { get; set; }

  public List<SeedFeature> Features { get; set; } = [];

  #region Weapon

  public WeaponCategoryEnum? WeaponCategory { get; set; }

  public bool? Ranged { get; set; }

  public string? Damage { get; set; }

  public DamageTypeEnum? DamageType { get; set; }

  public WeaponPropertyEnum? Properties { get; set; }

  public string? Versatile { get; set; }

  public short? RangeNormal { get; set; }

  public short? RangeLong { get; set; }

  public string? Mastery { get; set; }

  #endregion

  #region Armor

  public ArmorEnum? ArmorCategory { get; set; }

  public byte? BaseAc { get; set; }

  public byte? DexCap { get; set; }

  public byte? StrengthRequirement { get; set; }

  public bool? StealthDisadvantage { get; set; }

  #endregion

  #region Tool

  public string? ToolGroup { get; set; }

  #endregion
}
