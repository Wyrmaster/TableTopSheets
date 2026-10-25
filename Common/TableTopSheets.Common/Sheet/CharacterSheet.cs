using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

/// <summary>
///   Every computed value needed to print a character sheet
/// </summary>
public sealed record CharacterSheet
{
  #region Title

  public string Name { get; init; } = string.Empty;

  public string Background { get; init; } = string.Empty;

  /// <summary>
  ///   Classes with levels (e.g. <c>Wizard 3 / Fighter 2</c>)
  /// </summary>
  public string Classes { get; init; } = string.Empty;

  public string Subclasses { get; init; } = string.Empty;

  public string Species { get; init; } = string.Empty;

  public int Level { get; init; }

  public int Experience { get; init; }

  public IReadOnlyList<ClassLevelSheet> ClassLevels { get; init; } = [];

  #endregion

  #region Abilities

  public int ProficiencyBonus { get; init; }

  public IReadOnlyList<AbilitySheet> Abilities { get; init; } = [];

  #endregion

  #region Combat

  public int ArmorClass { get; init; }

  public bool ShieldEquipped { get; init; }

  /// <summary>
  ///   Available Armor Class calculations (only one applies at a time)
  /// </summary>
  public IReadOnlyList<SourceValue> ArmorClassOptions { get; init; } = [];

  public IReadOnlyList<SourceValue> ArmorClassSources { get; init; } = [];

  public int HitPointMaximum { get; init; }

  public int? CurrentHitPoints { get; init; }

  public int TemporaryHitPoints { get; init; }

  /// <summary>
  ///   Hit point dice per die type
  /// </summary>
  public IReadOnlyList<DicePool> HitDice { get; init; } = [];

  public int Initiative { get; init; }

  public int Speed { get; init; }

  public IReadOnlyList<SourceValue> OtherSpeeds { get; init; } = [];

  public SizeEnum Size { get; init; }

  public int PassivePerception { get; init; }

  public IReadOnlyList<SourceValue> Senses { get; init; } = [];

  public int AttacksPerAction { get; init; } = 1;

  public IReadOnlyList<AttackSheet> Attacks { get; init; } = [];

  public IReadOnlyList<string> Resistances { get; init; } = [];

  public IReadOnlyList<string> Immunities { get; init; } = [];

  public IReadOnlyList<string> Vulnerabilities { get; init; } = [];

  #endregion

  #region Training

  public ArmorEnum ArmorTraining { get; init; }

  public IReadOnlyList<string> WeaponProficiencies { get; init; } = [];

  public IReadOnlyList<string> WeaponMasteries { get; init; } = [];

  public IReadOnlyList<string> ToolProficiencies { get; init; } = [];

  public IReadOnlyList<string> Languages { get; init; } = [];

  #endregion

  #region Magic

  public IReadOnlyList<SpellcastingSheet> Spellcasting { get; init; } = [];

  /// <summary>
  ///   One entry per slot; Pact Magic slots refresh on a short rest
  /// </summary>
  public IReadOnlyList<SpellSlot> SpellSlots { get; init; } = [];

  public IReadOnlyList<SpellSheet> Spells { get; init; } = [];

  #endregion

  #region Features

  public IReadOnlyList<FeatureSheet> ClassFeatures { get; init; } = [];

  public IReadOnlyList<FeatureSheet> SpeciesTraits { get; init; } = [];

  public IReadOnlyList<FeatureSheet> Feats { get; init; } = [];

  public IReadOnlyList<ResourceSheet> Resources { get; init; } = [];

  #endregion

  #region Equipment

  public IReadOnlyList<string> Equipment { get; init; } = [];

  public IReadOnlyList<string> AttunedItems { get; init; } = [];

  public Coins Coins { get; init; } = new();

  #endregion

  #region Validation

  /// <summary>
  ///   Rule problems (open choices, illegal loadouts, missing training ...)
  /// </summary>
  public IReadOnlyList<SheetViolation> Violations { get; init; } = [];

  #endregion
}

public sealed record ClassLevelSheet(string Class, string? Subclass, int Level, DiceEnum HitDie);

public sealed record SourceValue(string Label, int Value);

public sealed record DicePool(DiceEnum Die, int Count);

public sealed record SkillSheet(string Key, string Name, int Proficiency, int Bonus, bool Advantage, bool Disadvantage);

public sealed record AbilitySheet(string Key, string Name, int Score, int Modifier, int SaveProficiency, int SaveBonus, IReadOnlyList<SkillSheet> Skills);

/// <param name="SaveDc">set instead of an attack roll for effects that force a saving throw</param>
/// <param name="SaveAbility">ability key of the saving throw</param>
/// <param name="DamageType">null when the damage type is chosen on use</param>
public sealed record AttackSheet
(
  string Name,
  int AttackBonus,
  string Damage,
  IReadOnlyList<DiceEnum> DamageDice,
  int DamageBonus,
  DamageTypeEnum? DamageType,
  string Notes,
  int? SaveDc = null,
  string? SaveAbility = null
);

public sealed record SpellcastingSheet(string Class, string Ability, int Modifier, int SaveDc, int AttackBonus, int PreparedLimit, int CantripLimit);

public sealed record SpellSheet
(
  string Key,
  string Name,
  int Level,
  string CastingTime,
  string Range,
  bool Concentration,
  bool Ritual,
  bool Material,
  string Source,
  string Notes
);

public sealed record FeatureSheet(string Key, string Name, string Source, string Description);

public sealed record ResourceSheet(string Key, string Name, int Maximum, int Used, string? Die, string Recharge);

public sealed record Coins
{
  public int Copper { get; init; }

  public int Silver { get; init; }

  public int Electrum { get; init; }

  public int Gold { get; init; }

  public int Platinum { get; init; }
}

public enum ViolationSeverityEnum
{
  Info = 0,
  Warning = 1,
  Error = 2,
}

/// <summary>
///   Rule problem found while computing a sheet
/// </summary>
public sealed record SheetViolation(string Code, string Message, ViolationSeverityEnum Severity = ViolationSeverityEnum.Warning);
