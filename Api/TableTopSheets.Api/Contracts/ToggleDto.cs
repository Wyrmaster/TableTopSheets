using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Switch a toggleable feature (Rage, Mage Armor ...)</summary>
public sealed record ToggleDto(bool Active);
