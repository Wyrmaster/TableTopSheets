using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Contribution of a single effect to a stat (used for the source breakdown)
/// </summary>
public sealed record StatSource(string Label, string Operation, double Value);
