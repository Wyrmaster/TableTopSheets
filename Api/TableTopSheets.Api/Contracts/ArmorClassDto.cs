using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Select the armor class calculation by its label; null takes the highest</summary>
public sealed record ArmorClassDto(string? Label);
