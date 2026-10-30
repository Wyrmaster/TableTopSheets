using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>One alternative of a starting equipment ("(A) Spear, 2 Daggers ..., 28 GP")</summary>
/// <param name="Key">letter of the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Items">items of the alternative; entries with <c>options</c> need a pick</param>
/// <param name="CoinsCp">coins of the alternative in copper pieces (paid out in gold pieces)</param>
public sealed record StartingEquipmentOptionDto(string Key, IReadOnlyList<StartingEquipmentItemDto> Items, int CoinsCp);
