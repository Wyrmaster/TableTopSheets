using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Species in a list</summary>
public sealed record SpeciesSummaryDto(string Key, string Name, string CreatureType);
