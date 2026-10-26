using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   One selection for a choice
/// </summary>
/// <param name="ChoiceKey">key of the choice</param>
/// <param name="OptionKey">selected option</param>
/// <param name="Ref">optional name other selections of the same request can use as <paramref name="ParentRef"/></param>
/// <param name="ParentRef">
///   scope of the choice instance: the <paramref name="Ref"/> of another selection of the same request
///   or the id of an existing selection (e.g. the ability choice of a feat selected in the same request)
/// </param>
public sealed record SelectionRequest(string ChoiceKey, string OptionKey, string? Ref = null, string? ParentRef = null);
