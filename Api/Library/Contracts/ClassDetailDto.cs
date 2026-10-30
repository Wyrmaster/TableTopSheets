using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Class with its features per level, subclasses and the starting equipment of a character that starts with it</summary>
public sealed record ClassDetailDto
(
  ClassSummaryDto Class,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<RuleDetailDto> Subclasses,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
);
