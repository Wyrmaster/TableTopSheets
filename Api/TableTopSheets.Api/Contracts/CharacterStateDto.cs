using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

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

  public required CoinsDto Coins { get; init; }

  /// <summary>Starting equipment alternative taken from the background (null = still open)</summary>
  public required string? BackgroundEquipment { get; init; }

  /// <summary>Starting equipment alternative taken from the first class (null = still open or no class yet)</summary>
  public required string? ClassEquipment { get; init; }

  /// <summary>True when no required choice is open</summary>
  public required bool Complete { get; init; }

  /// <summary>Choices that still need selections (follow-up choices appear here after their parent was selected)</summary>
  public required IReadOnlyList<PendingChoiceDto> PendingChoices { get; init; }

  public required IReadOnlyList<ViolationDto> Violations { get; init; }
}
