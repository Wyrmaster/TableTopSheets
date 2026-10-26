using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Full description of a rule entity with its features; a background also lists its starting equipment (otherwise empty)</summary>
public sealed record RuleDetailDto
(
  string Key,
  string Name,
  string Description,
  IReadOnlyList<FeatureDto> Features,
  IReadOnlyList<StartingEquipmentOptionDto> StartingEquipment
);
