namespace TableTopSheets.Api.Contracts;

/// <summary>Pieces added to an item stack (at least 1)</summary>
public sealed record AddToStackDto(int Quantity = 1);
