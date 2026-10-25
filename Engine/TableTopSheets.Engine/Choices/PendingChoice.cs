using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Choices;

/// <summary>
///   Choice instance that still needs selections
/// </summary>
public sealed record PendingChoice(Choice Choice, FeatureInstance Owner, int Total, int Selected)
{
  public int Remaining => this.Total - this.Selected;

  /// <summary>
  ///   Id of the selection that scopes this choice instance (pass as <c>ParentRef</c> when selecting)
  /// </summary>
  public Guid? ParentSelectionId => this.Owner.SelectionScope;
}

/// <summary>
///   An option of a choice instance and why it can't be selected (null when it can)
/// </summary>
public sealed record OptionAvailability(OptionDescriptor Option, string? Reason)
{
  public bool Available => this.Reason == null;
}
