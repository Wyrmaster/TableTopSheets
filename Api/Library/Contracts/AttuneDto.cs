using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Attune to an item or end the attunement</summary>
public sealed record AttuneDto(bool Attuned);
