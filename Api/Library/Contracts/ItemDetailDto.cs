using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Item with its full text and properties</summary>
public sealed record ItemDetailDto(ItemSummaryDto Item, string Description, IReadOnlyDictionary<string, string> Properties, IReadOnlyList<FeatureDto> Features);
