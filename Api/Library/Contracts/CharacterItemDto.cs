using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Owned item; <c>notes</c> tell what it is about (a book of <c>prayers</c>); <c>rarity</c> is null for mundane items</summary>
public sealed record CharacterItemDto
(
  Guid Id,
  string ItemKey,
  string Name,
  ItemKindEnum Kind,
  int Quantity,
  EquipSlotEnum Slot,
  bool Attuned,
  bool RequiresAttunement,
  string? Notes,
  bool Magic,
  RarityEnum? Rarity
);
