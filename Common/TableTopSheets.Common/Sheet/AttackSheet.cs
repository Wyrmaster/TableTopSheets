using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

/// <param name="SaveDc">set instead of an attack roll for effects that force a saving throw</param>
/// <param name="SaveAbility">ability key of the saving throw</param>
/// <param name="DamageType">null when the damage type is chosen on use</param>
public sealed record AttackSheet
(
  string Name,
  int AttackBonus,
  string Damage,
  IReadOnlyList<DiceEnum> DamageDice,
  int DamageBonus,
  DamageTypeEnum? DamageType,
  string Notes,
  int? SaveDc = null,
  string? SaveAbility = null
);
