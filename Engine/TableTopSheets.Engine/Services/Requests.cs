using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   One selection for a choice
/// </summary>
/// <param name="ChoiceKey">key of the choice</param>
/// <param name="OptionKey">selected option</param>
/// <param name="Ref">optional name other selections of the same request can use as <paramref name="ParentRef"/></param>
/// <param name="ParentRef">
///   scope of the choice instance: the <paramref name="Ref"/> of another selection of the same request
///   or the id of an existing selection (e.g. the ability choice of a feat selected in the same request)
/// </param>
public sealed record SelectionRequest(string ChoiceKey, string OptionKey, string? Ref = null, string? ParentRef = null);

/// <summary>
///   Replace an existing selection whose choice allows one swap per level up
/// </summary>
public sealed record ReplacementRequest(Guid SelectionId, string OptionKey);

/// <summary>
///   Take one alternative of a starting equipment ("Choose A or B")
/// </summary>
/// <param name="OptionKey">the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Picks">
///   item picked per entry key for entries that offer a choice (a Holy Symbol, a Gaming Set); a pick of a "same as above"
///   entry also answers that choice when it is still open
/// </param>
public sealed record StartingEquipmentRequest(string OptionKey, IReadOnlyDictionary<string, string>? Picks = null);

public sealed record CreateCharacterRequest
{
  public required string Name { get; init; }

  public required string SpeciesKey { get; init; }

  public required string BackgroundKey { get; init; }

  public AbilityMethodEnum AbilityMethod { get; init; }

  /// <summary>
  ///   Base scores (before background and other increases) keyed by ability key
  /// </summary>
  public required IReadOnlyDictionary<string, byte> Abilities { get; init; }

  public AlignmentEnum Alignment { get; init; }

  /// <summary>
  ///   Species, background and origin feat selections
  /// </summary>
  public IReadOnlyList<SelectionRequest> Selections { get; init; } = [];

  /// <summary>
  ///   Starting equipment of the background (null = take it later)
  /// </summary>
  public StartingEquipmentRequest? StartingEquipment { get; init; }

  /// <summary>
  ///   Allow open choices (otherwise every required choice has to be answered)
  /// </summary>
  public bool AllowIncomplete { get; init; }
}

public sealed record LevelUpRequest
{
  public required string ClassKey { get; init; }

  /// <summary>
  ///   Required at the class level the subclass is gained
  /// </summary>
  public string? SubclassKey { get; init; }

  /// <summary>
  ///   Rolled hit points (null = fixed value); ignored at character level 1
  /// </summary>
  public byte? HitPointRoll { get; init; }

  public IReadOnlyList<SelectionRequest> Selections { get; init; } = [];

  public IReadOnlyList<ReplacementRequest> Replacements { get; init; } = [];

  /// <summary>
  ///   Starting equipment of the class; only with the first level of the character (null = take it later)
  /// </summary>
  public StartingEquipmentRequest? StartingEquipment { get; init; }

  public bool AllowIncomplete { get; init; }
}

/// <summary>
///   Descriptive fields of a character; null keeps the current value
/// </summary>
public sealed record CharacterDetailsUpdate
{
  public string? Name { get; init; }

  public AlignmentEnum? Alignment { get; init; }

  public string? Appearance { get; init; }

  public string? Backstory { get; init; }

  public int? Experience { get; init; }
}
