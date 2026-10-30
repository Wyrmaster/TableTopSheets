using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Swap an existing selection on a level up</summary>
public sealed record ReplacementDto(Guid SelectionId, string OptionKey);
