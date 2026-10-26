using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Selections for open choices</summary>
public sealed record SelectionsDto(IReadOnlyList<SelectionDto> Selections);
