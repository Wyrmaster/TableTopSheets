using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   An active selection of a choice
/// </summary>
public sealed record SelectionInstance(CharacterChoice Selection, Choice Choice, FeatureInstance Owner, ChoiceOption? StaticOption);
