using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Take an alternative of a starting equipment</summary>
/// <param name="OptionKey">the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Picks">picked item key per entry key, for the entries with options</param>
public sealed record TakeStartingEquipmentDto(string OptionKey, IReadOnlyDictionary<string, string>? Picks = null);
