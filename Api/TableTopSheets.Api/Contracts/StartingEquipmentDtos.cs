using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>One alternative of a starting equipment ("(A) Spear, 2 Daggers ..., 28 GP")</summary>
/// <param name="Key">letter of the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Items">items of the alternative; entries with <c>options</c> need a pick</param>
/// <param name="CoinsCp">coins of the alternative in copper pieces (paid out in gold pieces)</param>
public sealed record StartingEquipmentOptionDto(string Key, IReadOnlyList<StartingEquipmentItemDto> Items, int CoinsCp);

/// <summary>An item of a starting equipment alternative: a fixed item (<c>itemKey</c>) or a pick from <c>options</c></summary>
/// <param name="Key">entry key; picks are sent keyed by it</param>
/// <param name="Name">item name, or the name of the pick (<c>Holy Symbol</c>)</param>
/// <param name="ItemKey">the fixed item; for a pick the item already chosen through <c>sameAs</c>, otherwise null</param>
/// <param name="Quantity">pieces (2 daggers, 20 arrows)</param>
/// <param name="Note">note stored with the item (<c>prayers</c>)</param>
/// <param name="Options">items to pick from (empty for fixed items)</param>
/// <param name="SameAs">choice the pick has to match ("same as above"); a pick answers it while it's open</param>
public sealed record StartingEquipmentItemDto
(
  string Key,
  string Name,
  string? ItemKey,
  int Quantity,
  string? Note,
  IReadOnlyList<ItemOptionDto> Options,
  string? SameAs
);

/// <summary>An item that can be picked</summary>
public sealed record ItemOptionDto(string Key, string Name);

/// <summary>Starting equipment a character can take from its background or first class</summary>
/// <param name="Source">background or class</param>
/// <param name="OwnerKey">key of the background or class</param>
/// <param name="OwnerName">name of the background or class</param>
/// <param name="Taken">key of the alternative already taken; null while it can be taken</param>
/// <param name="Options">the alternatives</param>
public sealed record StartingEquipmentDto
(
  StartingEquipmentSourceEnum Source,
  string OwnerKey,
  string OwnerName,
  string? Taken,
  IReadOnlyList<StartingEquipmentOptionDto> Options
);

/// <summary>Take an alternative of a starting equipment</summary>
/// <param name="OptionKey">the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Picks">picked item key per entry key, for the entries with options</param>
public sealed record TakeStartingEquipmentDto(string OptionKey, IReadOnlyDictionary<string, string>? Picks = null);
