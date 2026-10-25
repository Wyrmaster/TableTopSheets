using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Static checks of the rule data: expressions parse, only known functions/identifiers/stats are used and references exist.
///   Run after seeding so broken data is found before a character uses it
/// </summary>
public static class CatalogValidator
{
  #region Methods

  public static IReadOnlyList<string> Validate(RulesCatalog catalog)
  {
    List<string> errors = new();

    foreach (Feature feature in catalog.Features.Values)
    {
      string where = $"feature '{feature.Key}'";
      CheckExpression(catalog, errors, where, "prerequisite", feature.PrerequisiteExpr, optionContext: true);

      foreach (Effect effect in feature.Effects)
      {
        CheckEffect(catalog, errors, where, effect, null);
      }

      foreach (Choice choice in feature.Choices)
      {
        string choiceWhere = $"choice '{choice.Key}'";
        CheckExpression(catalog, errors, choiceWhere, "count", choice.CountExpr, optionContext: false);
        CheckExpression(catalog, errors, choiceWhere, "filter", choice.FilterExpr, optionContext: true);
        if (choice.OptionSource == OptionSourceEnum.FeatureCategory && !catalog.FeaturesByCategory.ContainsKey(choice.Category ?? string.Empty))
        {
          errors.Add($"{choiceWhere}: no features with category '{choice.Category}'");
        }

        if (choice.OptionSource == OptionSourceEnum.Static && choice.Options.Count == 0)
        {
          errors.Add($"{choiceWhere}: static choice without options");
        }

        foreach (ChoiceOption option in choice.Options)
        {
          CheckExpression(catalog, errors, $"{choiceWhere} option '{option.Key}'", "prerequisite", option.PrerequisiteExpr, optionContext: true);
          if (option.GrantsFeatureId is { } grants && !catalog.Features.ContainsKey(grants))
          {
            errors.Add($"{choiceWhere} option '{option.Key}': granted feature doesn't exist");
          }
        }

        foreach (Effect effect in choice.Effects)
        {
          CheckEffect(catalog, errors, choiceWhere, effect, choice);
        }
      }

      foreach (ResourceDefinition resource in feature.Resources)
      {
        CheckExpression(catalog, errors, $"resource '{resource.Key}'", "max", resource.MaxExpr, optionContext: false);
        CheckExpression(catalog, errors, $"resource '{resource.Key}'", "die", resource.DieExpr, optionContext: false);
      }

      if (feature.Spellcasting is { } profile)
      {
        foreach (string? key in new[] { profile.PreparedCountKey, profile.CantripCountKey })
        {
          if (key != null && !catalog.ScaleValues.ContainsKey(key))
          {
            errors.Add($"{where}: spellcasting scale '{key}' doesn't exist");
          }
        }

        string table = profile.SlotTableKey ?? (profile.Progression == CasterProgressionEnum.Pact ? CharacterEvaluation.PactTable : CharacterEvaluation.MulticlassTable);
        if (!catalog.SpellSlotTables.ContainsKey(table))
        {
          errors.Add($"{where}: spell slot table '{table}' doesn't exist");
        }

        if (!RuleKeys.IsAbility(profile.Ability))
        {
          errors.Add($"{where}: unknown spellcasting ability '{profile.Ability}'");
        }

        if (catalog.Spells.Values.All(entry => !entry.Lists.Contains(profile.SpellListKey)))
        {
          errors.Add($"{where}: no spell is on the list '{profile.SpellListKey}'");
        }
      }
    }

    foreach (Feat feat in catalog.Feats.Values)
    {
      CheckExpression(catalog, errors, $"feat '{feat.Key}'", "prerequisite", feat.PrerequisiteExpr, optionContext: true);
    }

    return errors;
  }

  private static void CheckEffect(RulesCatalog catalog, List<string> errors, string where, Effect effect, Choice? choice)
  {
    string sample = choice == null ? "x" : SampleOption(catalog, choice);
    string target = effect.Target.Replace("$option", sample).Replace("$value", choice?.Options.FirstOrDefault()?.Value ?? sample);
    if (!StatKeys.IsValid(target))
    {
      errors.Add($"{where}: unknown stat '{effect.Target}'");
    }

    if (target.StartsWith("spell.", StringComparison.Ordinal) && !catalog.SpellsByKey.ContainsKey(target["spell.".Length..]) && !effect.Target.Contains('$'))
    {
      errors.Add($"{where}: spell '{target["spell.".Length..]}' doesn't exist");
    }

    string value = effect.ValueExpr.Replace("$value", choice?.Options.FirstOrDefault()?.Value ?? "1").Replace("$option", "1");
    CheckExpression(catalog, errors, where, $"value of '{effect.Target}'", value, optionContext: false);
    CheckExpression(catalog, errors, where, $"condition of '{effect.Target}'", effect.ConditionExpr, optionContext: false);
  }

  private static string SampleOption(RulesCatalog catalog, Choice choice) => choice.OptionSource switch
  {
    OptionSourceEnum.Ability => "str",
    OptionSourceEnum.Skill => "stealth",
    OptionSourceEnum.Spell => catalog.SpellsByKey.Keys.First(),
    OptionSourceEnum.Static => choice.Options.FirstOrDefault()?.Key ?? "x",
    _ => "x",
  };

  private static void CheckExpression(RulesCatalog catalog, List<string> errors, string where, string what, string? source, bool optionContext)
  {
    if (string.IsNullOrWhiteSpace(source))
    {
      return;
    }

    Expression expression;
    try
    {
      expression = Expression.Parse(source);
    }
    catch (ExpressionException exception)
    {
      errors.Add($"{where}: {what} doesn't parse: {exception.Message}");
      return;
    }

    foreach (string identifier in expression.Identifiers)
    {
      bool known = CharacterEvaluation.KnownIdentifiers.Contains(identifier)
                   || (optionContext && OptionContext.KnownIdentifiers.Contains(identifier));
      if (!known)
      {
        errors.Add($"{where}: {what} uses unknown identifier '{identifier}'");
      }
    }

    foreach ((string function, IReadOnlyList<string?> arguments) in expression.Calls)
    {
      if (!CharacterEvaluation.KnownFunctions.Contains(function) && !(optionContext && function == "onList"))
      {
        errors.Add($"{where}: {what} uses unknown function '{function}'");
        continue;
      }

      string? argument = arguments.Count > 0 ? arguments[0] : null;
      if (argument == null)
      {
        continue;
      }

      string? problem = function switch
      {
        "scale" when !catalog.ScaleValues.ContainsKey(argument) => $"scale '{argument}'",
        "classLevel" or "maxSpellLevel" when !catalog.ClassesByKey.ContainsKey(argument) => $"class '{argument}'",
        "subclass" when !catalog.SubclassesByKey.ContainsKey(argument) => $"subclass '{argument}'",
        "has" or "active" when !catalog.FeaturesByKey.ContainsKey(argument) => $"feature '{argument}'",
        "score" or "mod" when !RuleKeys.IsAbility(argument) => $"ability '{argument}'",
        "stat" when !StatKeys.IsValid(argument) => $"stat '{argument}'",
        _ => null,
      };
      if (problem != null)
      {
        errors.Add($"{where}: {what} references unknown {problem}");
      }
    }
  }

  #endregion
}
