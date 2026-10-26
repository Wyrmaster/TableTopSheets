using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Coins of a character (or, for a change, the coins added (positive) or removed (negative) per kind)</summary>
public sealed record CoinsDto(int Copper = 0, int Silver = 0, int Electrum = 0, int Gold = 0, int Platinum = 0);
