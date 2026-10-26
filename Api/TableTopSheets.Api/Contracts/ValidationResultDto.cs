using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Result of a validation without saving</summary>
public sealed record ValidationResultDto(bool Valid, IReadOnlyList<string> Errors, CharacterStateDto? State);
