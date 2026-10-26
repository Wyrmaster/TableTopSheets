using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Choices;

/// <summary>
///   An option of a choice instance and why it can't be selected (null when it can)
/// </summary>
public sealed record OptionAvailability(OptionDescriptor Option, string? Reason)
{
  public bool Available => this.Reason == null;
}
