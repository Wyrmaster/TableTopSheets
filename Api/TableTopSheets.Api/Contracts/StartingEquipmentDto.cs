using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Starting equipment a character can take from its background or first class</summary>
/// <param name="Source">background or class</param>
/// <param name="OwnerKey">key of the background or class</param>
/// <param name="OwnerName">name of the background or class</param>
/// <param name="Taken">key of the alternative already taken; null while it can be taken</param>
/// <param name="Options">the alternatives</param>
public sealed record StartingEquipmentDto
(
  StartingEquipmentSourceEnum Source,
  string OwnerKey,
  string OwnerName,
  string? Taken,
  IReadOnlyList<StartingEquipmentOptionDto> Options
);
