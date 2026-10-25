using Microsoft.AspNetCore.WebUtilities;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Describes choice instances and their options so a client can render them without knowing the rules
/// </summary>
public static class ChoiceMapper
{
  #region Constants

  /// <summary>
  ///   Choices with at most this many available options embed them in the pending choice
  /// </summary>
  public const int EmbeddedOptionLimit = 30;

  private const int SummaryLength = 160;

  #endregion

  #region Pending Choices

  /// <param name="evaluation">evaluation the choice belongs to</param>
  /// <param name="pending">choice instance</param>
  /// <param name="preview">level up preview (<c>class</c> or <c>class:subclass</c>) the options are evaluated against</param>
  public static PendingChoiceDto Pending(CharacterEvaluation evaluation, PendingChoice pending, string? preview = null)
  {
    Choice choice = pending.Choice;
    List<OptionDescriptor> available = ChoiceService.AvailableOptions(evaluation, choice, pending.Owner).ToList();
    string? parentRef = pending.ParentSelectionId?.ToString();

    return new PendingChoiceDto
    {
      Id = parentRef == null ? choice.Key : $"{choice.Key}@{parentRef}",
      ChoiceKey = choice.Key,
      Name = choice.Name,
      Source = pending.Owner.Source,
      Description = HtmlText.ToPlain(pending.Owner.Feature.DescriptionHtml),
      ParentRef = parentRef,
      Input = InputKind(choice, pending.Total),
      OptionSource = choice.OptionSource,
      Optional = choice.Optional,
      Total = pending.Total,
      Selected = pending.Selected,
      Remaining = pending.Remaining,
      Constraints = Constraints(choice, pending, available.Count),
      OptionFilters = OptionFilters(choice.OptionSource),
      OptionCount = available.Count,
      Options = available.Count <= EmbeddedOptionLimit
        ? available.Select(entry => Option(evaluation.Catalog, choice, entry, null, false)).ToList()
        : null,
      OptionsUrl = OptionsUrl(evaluation.Character.Id, choice.Key, parentRef, preview),
      SelectedOptions = evaluation.SelectionsOf(choice, pending.Owner)
        .Select(entry => new SelectedOptionDto(entry.Selection.Id, entry.Selection.OptionKey, evaluation.OptionName(choice, entry.Selection.OptionKey)))
        .ToList(),
    };
  }

  public static IReadOnlyList<PendingChoiceDto> Pending(CharacterEvaluation evaluation, string? preview = null)
    => ChoiceService.Pending(evaluation).Select(entry => Pending(evaluation, entry, preview)).ToList();

  public static ChoiceInputKind InputKind(Choice choice, int total)
    => choice.AllowDuplicates && choice.MaxPerOption > 1 && total > 1
      ? ChoiceInputKind.Distribution
      : total == 1
        ? ChoiceInputKind.SingleSelect
        : ChoiceInputKind.MultiSelect;

  private static ChoiceConstraintsDto Constraints(Choice choice, PendingChoice pending, int availableCount)
  {
    int maxPerOption = choice.AllowDuplicates ? Math.Max(1, (int)choice.MaxPerOption) : 1;
    return new ChoiceConstraintsDto
    (
      choice.Optional ? 0 : pending.Remaining,
      pending.Remaining,
      maxPerOption,
      choice.AllowDuplicates,
      InputKind(choice, pending.Total) == ChoiceInputKind.Distribution ? Partitions(pending.Remaining, maxPerOption, availableCount) : null
    );
  }

  /// <summary>
  ///   Ways to split <paramref name="total"/> picks into at most <paramref name="options"/> parts of at most
  ///   <paramref name="maxPart"/>, largest part first (3 picks, 2 per option: [2,1] and [1,1,1])
  /// </summary>
  public static IReadOnlyList<IReadOnlyList<int>> Partitions(int total, int maxPart, int options)
  {
    List<IReadOnlyList<int>> result = [];

    void Split(int rest, int max, List<int> parts)
    {
      if (rest == 0)
      {
        result.Add(parts.ToList());
        return;
      }

      if (parts.Count == options)
      {
        return;
      }

      for (int part = Math.Min(rest, max); part >= 1; part--)
      {
        parts.Add(part);
        Split(rest - part, part, parts);
        parts.RemoveAt(parts.Count - 1);
      }
    }

    Split(total, maxPart, []);
    return result;
  }

  /// <summary>
  ///   Filters of the options endpoint that make sense for an option source
  /// </summary>
  public static IReadOnlyList<string> OptionFilters(OptionSourceEnum source) => source switch
  {
    OptionSourceEnum.Spell => ["search", "level", "minLevel", "maxLevel", "school", "ritual", "damage"],
    OptionSourceEnum.Feat => ["search", "category"],
    OptionSourceEnum.FeatureCategory => ["search", "level", "minLevel", "maxLevel"],
    OptionSourceEnum.Tool or OptionSourceEnum.Language or OptionSourceEnum.Item or OptionSourceEnum.Weapon => ["search", "category"],
    _ => ["search"],
  };

  public static string OptionsUrl(Guid characterId, string choiceKey, string? parentRef, string? preview)
  {
    string url = $"/api/characters/{characterId}/choices/{Uri.EscapeDataString(choiceKey)}/options";
    if (parentRef != null)
    {
      url = QueryHelpers.AddQueryString(url, "parentRef", parentRef);
    }

    if (preview != null)
    {
      url = QueryHelpers.AddQueryString(url, "preview", preview);
    }

    return url;
  }

  #endregion

  #region Options

  /// <summary>
  ///   Describe an option; <paramref name="reason"/> marks it as disabled
  /// </summary>
  public static OptionDto Option(RulesCatalog catalog, Choice choice, OptionDescriptor option, string? reason, bool includeDescription)
  {
    (string? html, IReadOnlyList<string> grants) = Describe(catalog, choice, option);
    string? plain = html == null ? null : HtmlText.ToPlain(html);
    return new OptionDto
    {
      Key = option.Key,
      Name = option.Name,
      Summary = plain == null ? null : Summarize(plain),
      Description = includeDescription ? plain : null,
      Disabled = reason != null,
      DisabledReason = reason,
      Grants = grants,
      Meta = new OptionMetaDto
      (
        choice.OptionSource is OptionSourceEnum.Spell or OptionSourceEnum.FeatureCategory ? option.Level : null,
        option.School,
        choice.OptionSource == OptionSourceEnum.Spell ? option.Ritual : null,
        choice.OptionSource == OptionSourceEnum.Spell ? option.Damage : null,
        option.Category,
        option.Value,
        choice.OptionSource == OptionSourceEnum.Feat ? option.Repeatable : null,
        option.PrerequisiteExpr,
        option.Lists.Count > 0 ? option.Lists : null
      ),
    };
  }

  /// <summary>
  ///   Description html and names of granted features of an option
  /// </summary>
  private static (string? Html, IReadOnlyList<string> Grants) Describe(RulesCatalog catalog, Choice choice, OptionDescriptor option)
  {
    switch (choice.OptionSource)
    {
      case OptionSourceEnum.Static:
        ChoiceOption? authored = choice.Options.FirstOrDefault(entry => entry.Key == option.Key);
        if (authored?.GrantsFeatureId is { } featureId && catalog.Features.TryGetValue(featureId, out Feature? granted))
        {
          return (granted.DescriptionHtml, [granted.Name]);
        }

        return (null, []);
      case OptionSourceEnum.Spell:
        return (catalog.SpellsByKey.GetValueOrDefault(option.Key)?.DescriptionHtml, []);
      case OptionSourceEnum.Feat:
        return (catalog.FeatsByKey.GetValueOrDefault(option.Key)?.DescriptionHtml, []);
      case OptionSourceEnum.FeatureCategory:
        return (catalog.FeaturesByKey.GetValueOrDefault(option.Key)?.DescriptionHtml, []);
      case OptionSourceEnum.Tool or OptionSourceEnum.Item or OptionSourceEnum.Weapon:
        return (catalog.ItemsByKey.GetValueOrDefault(option.Key)?.DescriptionHtml, []);
      case OptionSourceEnum.Language:
        return (catalog.Languages.GetValueOrDefault(option.Key)?.DescriptionHtml, []);
      default:
        return (null, []);
    }
  }

  /// <summary>
  ///   First sentence (or the start) of a text
  /// </summary>
  public static string? Summarize(string text)
  {
    string flat = string.Join(' ', text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    if (flat.Length == 0)
    {
      return null;
    }

    int end = flat.IndexOf(". ", StringComparison.Ordinal);
    string sentence = end >= 0 && end < SummaryLength ? flat[..(end + 1)] : flat;
    return sentence.Length <= SummaryLength ? sentence : sentence[..(SummaryLength - 1)].TrimEnd() + "…";
  }

  #endregion
}
