using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

public sealed record LevelUpRequest
{
  public required string ClassKey { get; init; }

  /// <summary>
  ///   Required at the class level the subclass is gained
  /// </summary>
  public string? SubclassKey { get; init; }

  /// <summary>
  ///   Rolled hit points (null = fixed value); ignored at character level 1
  /// </summary>
  public byte? HitPointRoll { get; init; }

  public IReadOnlyList<SelectionRequest> Selections { get; init; } = [];

  public IReadOnlyList<ReplacementRequest> Replacements { get; init; } = [];

  /// <summary>
  ///   Starting equipment of the class; only with the first level of the character (null = take it later)
  /// </summary>
  public StartingEquipmentRequest? StartingEquipment { get; init; }

  public bool AllowIncomplete { get; init; }
}
