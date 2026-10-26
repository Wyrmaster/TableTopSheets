using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Uniform description of a selectable option
/// </summary>
public sealed record OptionDescriptor
(
  string Key,
  string Name,
  int Level = 0,
  string? School = null,
  bool Ritual = false,
  string? Category = null,
  string? Value = null,
  bool Repeatable = false,
  string? PrerequisiteExpr = null,
  bool Damage = false
)
{
  public IReadOnlyList<string> Lists { get; init; } = [];
}
