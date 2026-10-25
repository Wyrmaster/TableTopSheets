using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Character in a list</summary>
public sealed record CharacterSummaryDto(Guid Id, string Name, int Level, string Species, string Background, IReadOnlyList<ClassLevelDto> Classes);

/// <summary>Levels of a character in one class</summary>
public sealed record ClassLevelDto(string Key, string Name, int Level, string? Subclass);

/// <summary>Ability score with its modifier</summary>
public sealed record AbilityDto(string Key, string Name, int Score, int Modifier);

/// <summary>Rule violation of the current state (e.g. armor without training)</summary>
public sealed record ViolationDto(string Code, string Message, ViolationSeverityEnum Severity);

/// <summary>Owned item</summary>
public sealed record CharacterItemDto(Guid Id, string ItemKey, string Name, ItemKindEnum Kind, int Quantity, EquipSlotEnum Slot, bool Attuned, bool RequiresAttunement);

/// <summary>
///   Everything a client needs after a change: the summary, what the user still has to decide and what is wrong
/// </summary>
public sealed record CharacterStateDto
{
  public required Guid Id { get; init; }

  public required string Name { get; init; }

  public required int Level { get; init; }

  public required string Species { get; init; }

  public required string Background { get; init; }

  public required AlignmentEnum Alignment { get; init; }

  public required IReadOnlyList<ClassLevelDto> Classes { get; init; }

  public required IReadOnlyList<AbilityDto> Abilities { get; init; }

  public required int ProficiencyBonus { get; init; }

  public required int HitPointMaximum { get; init; }

  public required int ArmorClass { get; init; }

  public required IReadOnlyList<CharacterItemDto> Items { get; init; }

  /// <summary>True when no required choice is open</summary>
  public required bool Complete { get; init; }

  /// <summary>Choices that still need selections (follow-up choices appear here after their parent was selected)</summary>
  public required IReadOnlyList<PendingChoiceDto> PendingChoices { get; init; }

  public required IReadOnlyList<ViolationDto> Violations { get; init; }
}

/// <summary>
///   New character. Only the origin is decided here; classes are added with level ups
/// </summary>
public sealed record CreateCharacterDto
{
  public required string Name { get; init; }

  public required string SpeciesKey { get; init; }

  public required string BackgroundKey { get; init; }

  public AbilityMethodEnum AbilityMethod { get; init; }

  /// <summary>Base scores before increases, keyed by <c>str</c>, <c>dex</c>, <c>con</c>, <c>int</c>, <c>wis</c>, <c>cha</c></summary>
  public required IReadOnlyDictionary<string, byte> Abilities { get; init; }

  public AlignmentEnum Alignment { get; init; }

  /// <summary>Answers to origin choices known up front (more can be posted later)</summary>
  public IReadOnlyList<SelectionDto> Selections { get; init; } = [];

  /// <summary>Save even when required choices are open (default); false rejects incomplete characters</summary>
  public bool AllowIncomplete { get; init; } = true;
}

/// <summary>Descriptive fields; omitted fields keep their value</summary>
public sealed record UpdateCharacterDto
{
  public string? Name { get; init; }

  public AlignmentEnum? Alignment { get; init; }

  public string? Appearance { get; init; }

  public string? Backstory { get; init; }

  public int? Experience { get; init; }
}

/// <summary>
///   Answer for a choice. <c>parentRef</c> addresses the choice instance: the <c>parentRef</c> of the pending choice, or the
///   <c>ref</c> of another selection of the same request (choose a feat and its abilities at once)
/// </summary>
public sealed record SelectionDto(string ChoiceKey, string OptionKey, string? Ref = null, string? ParentRef = null);

/// <summary>Selections for open choices</summary>
public sealed record SelectionsDto(IReadOnlyList<SelectionDto> Selections);

/// <summary>Swap an existing selection on a level up</summary>
public sealed record ReplacementDto(Guid SelectionId, string OptionKey);

/// <summary>Gain a level</summary>
public sealed record LevelUpDto
{
  public required string ClassKey { get; init; }

  /// <summary>Required at the class level the subclass is gained</summary>
  public string? SubclassKey { get; init; }

  /// <summary>Rolled hit points; null takes the fixed value</summary>
  public byte? HitPointRoll { get; init; }

  public IReadOnlyList<SelectionDto> Selections { get; init; } = [];

  public IReadOnlyList<ReplacementDto> Replacements { get; init; } = [];

  /// <summary>Save even when required choices are open (default); false rejects incomplete level ups</summary>
  public bool AllowIncomplete { get; init; } = true;
}

/// <summary>Active selection of a character</summary>
public sealed record SelectionInfoDto
(
  Guid Id,
  string ChoiceKey,
  string ChoiceName,
  string OptionKey,
  string OptionName,
  string Source,
  int? Level,
  Guid? ParentId,
  ReplacePolicyEnum ReplacePolicy,
  bool Removable
);

/// <summary>What a level up in a class would ask for</summary>
public sealed record LevelUpPreviewDto
(
  string ClassKey,
  int ClassLevel,
  int CharacterLevel,
  bool RequiresSubclass,
  IReadOnlyList<SubclassSummaryDto> Subclasses,
  IReadOnlyList<PendingChoiceDto> PendingChoices,
  IReadOnlyList<SelectionInfoDto> Replaceable
);

/// <summary>Result of a validation without saving</summary>
public sealed record ValidationResultDto(bool Valid, IReadOnlyList<string> Errors, CharacterStateDto? State);
