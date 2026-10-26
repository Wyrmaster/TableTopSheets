using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>Definition of a choice as authored in the rules (independent of a character)</summary>
public sealed record ChoiceDefinitionDto(string Key, string Name, string Count, OptionSourceEnum OptionSource, bool Optional, bool AllowDuplicates, int MaxPerOption, ReplacePolicyEnum ReplacePolicy);
