using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Background in a list</summary>
public sealed record BackgroundSummaryDto(string Key, string Name, IReadOnlyList<string> AbilityOptions, string? OriginFeat);
