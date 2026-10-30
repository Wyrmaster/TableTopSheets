using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>A feature (class feature, species trait, feat benefit ...) with the choices it offers</summary>
public sealed record FeatureDto(string Key, string Name, int? Level, string Description, IReadOnlyList<ChoiceDefinitionDto> Choices);
