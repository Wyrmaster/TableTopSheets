using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Character in a list</summary>
public sealed record CharacterSummaryDto(Guid Id, string Name, int Level, string Species, string Background, IReadOnlyList<ClassLevelDto> Classes);
