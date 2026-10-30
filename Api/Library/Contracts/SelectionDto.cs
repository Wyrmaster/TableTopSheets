using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Answer for a choice. <c>parentRef</c> addresses the choice instance: the <c>parentRef</c> of the pending choice, or the
///   <c>ref</c> of another selection of the same request (choose a feat and its abilities at once)
/// </summary>
public sealed record SelectionDto(string ChoiceKey, string OptionKey, string? Ref = null, string? ParentRef = null);
