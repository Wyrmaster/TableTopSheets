namespace TableTopSheets.Api.Contracts;

/// <summary>Spells and items to print as cards, by key, in the order they are printed (at most 200 together)</summary>
public sealed record CardsRequestDto(IReadOnlyList<string>? Spells, IReadOnlyList<string>? Items);
