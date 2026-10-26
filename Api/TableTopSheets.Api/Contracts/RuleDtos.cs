using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Species in a list</summary>
public sealed record SpeciesSummaryDto(string Key, string Name, string CreatureType);

/// <summary>Background in a list</summary>
public sealed record BackgroundSummaryDto(string Key, string Name, IReadOnlyList<string> AbilityOptions, string? OriginFeat);

/// <summary>Class in a list</summary>
public sealed record ClassSummaryDto(string Key, string Name, DiceEnum HitDie, IReadOnlyList<string> PrimaryAbilities, PrimaryAbilityModeEnum PrimaryAbilityMode, int SubclassLevel, IReadOnlyList<SubclassSummaryDto> Subclasses);

/// <summary>Subclass of a class</summary>
public sealed record SubclassSummaryDto(string Key, string Name);

/// <summary>Feat in a list</summary>
public sealed record FeatSummaryDto(string Key, string Name, string Category, bool Repeatable, bool HasPrerequisite);

/// <summary>Spell in a list</summary>
public sealed record SpellSummaryDto
(
  string Key,
  string Name,
  int Level,
  string School,
  string CastingTime,
  string Range,
  bool Ritual,
  bool Concentration,
  string? Damage,
  DamageTypeEnum? DamageType,
  IReadOnlyList<string> Lists
);

/// <summary>Item in a list; cost and weight are per <c>bundle</c> (20 arrows for 1 GP), quantities count pieces</summary>
public sealed record ItemSummaryDto
(
  string Key,
  string Name,
  ItemKindEnum Kind,
  string? Category,
  int CostCp,
  double Weight,
  bool Magic,
  bool RequiresAttunement,
  int Bundle
);

/// <summary>A feature (class feature, species trait, feat benefit ...) with the choices it offers</summary>
public sealed record FeatureDto(string Key, string Name, int? Level, string Description, IReadOnlyList<ChoiceDefinitionDto> Choices);

/// <summary>Definition of a choice as authored in the rules (independent of a character)</summary>
public sealed record ChoiceDefinitionDto(string Key, string Name, string Count, OptionSourceEnum OptionSource, bool Optional, bool AllowDuplicates, int MaxPerOption, ReplacePolicyEnum ReplacePolicy);

/// <summary>Full description of a rule entity with its features; a background also lists its starting equipment (otherwise empty)</summary>
public sealed record RuleDetailDto
(
  string Key,
  string Name,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
);

/// <summary>Class with its features per level, subclasses and the starting equipment of a character that starts with it</summary>
public sealed record ClassDetailDto
(
  ClassSummaryDto Class,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<RuleDetailDto> Subclasses,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
);

/// <summary>Spell with its full text</summary>
public sealed record SpellDetailDto(SpellSummaryDto Spell, string Components, string? Material, string Duration, bool AttackRoll, string? SaveAbility, string Description);

/// <summary>Item with its full text and properties</summary>
public sealed record ItemDetailDto(ItemSummaryDto Item, string Description, IReadOnlyDictionary<string, string> Properties, IReadOnlyList<FeatureDto> Features);
