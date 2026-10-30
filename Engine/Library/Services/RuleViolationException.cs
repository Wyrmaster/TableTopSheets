namespace TableTopSheets.Engine.Services;

/// <summary>
///   A request breaks the rules; nothing was changed
/// </summary>
public class RuleViolationException(IReadOnlyList<string> errors)
  : Exception(string.Join(Environment.NewLine, errors))
{
  public IReadOnlyList<string> Errors { get; } = errors;

  public RuleViolationException(string error) : this([error])
  {
  }
}
