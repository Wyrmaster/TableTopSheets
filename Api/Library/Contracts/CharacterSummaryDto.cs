using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Character in a list; <c>PortraitUrl</c> is null when the character has no picture</summary>
public sealed record CharacterSummaryDto
(
  Guid Id,
  string Name,
  int Level,
  string Species,
  string Background,
  IReadOnlyList<ClassLevelDto> Classes,
  string? PortraitUrl
);
