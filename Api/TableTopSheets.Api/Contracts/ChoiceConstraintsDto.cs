using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Limits of a choice instance, enough for a client to validate before sending
/// </summary>
/// <param name="Min">selections that have to be sent before the character is complete</param>
/// <param name="Max">selections that may still be sent</param>
/// <param name="MaxPerOption">how often one option may be picked</param>
/// <param name="AllowDuplicates">whether an option may be picked more than once</param>
/// <param name="AllowedPatterns">for distributions: the allowed splits of the remaining picks, e.g. <c>[[2,1],[1,1,1]]</c></param>
public sealed record ChoiceConstraintsDto(int Min, int Max, int MaxPerOption, bool AllowDuplicates, IReadOnlyList<IReadOnlyList<int>>? AllowedPatterns);
