using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

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
