using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Expression context while checking a single option of a choice; <c>option.*</c> identifiers and <c>onList</c> describe the option
/// </summary>
public sealed class OptionContext(CharacterEvaluation evaluation, OptionDescriptor option): IExpressionContext
{
  #region Fields

  public static readonly IReadOnlySet<string> KnownIdentifiers = new HashSet<string>
  {
    "option.key", "option.name", "option.level", "option.school", "option.ritual", "option.category", "option.value", "option.repeatable", "option.damage",
  };

  #endregion

  #region IExpressionContext

  public ExprValue Resolve(string identifier) => identifier switch
  {
    "option.key" => ExprValue.Of(option.Key),
    "option.name" => ExprValue.Of(option.Name),
    "option.level" => ExprValue.Of(option.Level),
    "option.school" => ExprValue.Of(option.School ?? string.Empty),
    "option.ritual" => ExprValue.Of(option.Ritual),
    "option.category" => ExprValue.Of(option.Category ?? string.Empty),
    "option.value" => ExprValue.Of(option.Value ?? string.Empty),
    "option.repeatable" => ExprValue.Of(option.Repeatable),
    "option.damage" => ExprValue.Of(option.Damage),
    _ => evaluation.Resolve(identifier),
  };

  public ExprValue Call(string function, IReadOnlyList<ExprValue> arguments) => function switch
  {
    "onList" => ExprValue.Of(arguments.Any(argument => option.Lists.Contains(argument.AsString()))),
    _ => evaluation.Call(function, arguments),
  };

  #endregion
}
