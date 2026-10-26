using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Feat in a list</summary>
public sealed record FeatSummaryDto(string Key, string Name, string Category, bool Repeatable, bool HasPrerequisite);
