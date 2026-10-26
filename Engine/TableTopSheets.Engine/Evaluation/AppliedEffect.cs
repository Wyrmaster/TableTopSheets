using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Effect that applies to the character with all placeholders (<c>$option</c>, <c>$value</c>) replaced
/// </summary>
public sealed record AppliedEffect(Effect Effect, string Target, string ValueExpr, string Label, FeatureInstance Owner);
