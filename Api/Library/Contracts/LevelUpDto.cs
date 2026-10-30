using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Gain a level</summary>
public sealed record LevelUpDto
{
  public required string ClassKey { get; init; }

  /// <summary>Required at the class level the subclass is gained</summary>
  public string? SubclassKey { get; init; }

  /// <summary>Rolled hit points; null takes the fixed value</summary>
  public byte? HitPointRoll { get; init; }

  public IReadOnlyList<SelectionDto> Selections { get; init; } = [];

  public IReadOnlyList<ReplacementDto> Replacements { get; init; } = [];

  /// <summary>Starting equipment of the class, only with the first level; null takes it later (<c>POST /starting-equipment/class</c>)</summary>
  public TakeStartingEquipmentDto? StartingEquipment { get; init; }

  /// <summary>Save even when required choices are open (default); false rejects incomplete level ups</summary>
  public bool AllowIncomplete { get; init; } = true;
}
