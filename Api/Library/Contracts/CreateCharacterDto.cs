using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

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

  /// <summary>Starting equipment of the background; null takes it later (<c>POST /starting-equipment/background</c>)</summary>
  public TakeStartingEquipmentDto? StartingEquipment { get; init; }

  /// <summary>Save even when required choices are open (default); false rejects incomplete characters</summary>
  public bool AllowIncomplete { get; init; } = true;
}
