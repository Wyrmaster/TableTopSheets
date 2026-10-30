using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Active selection of a character</summary>
public sealed record SelectionInfoDto
(
  Guid Id,
  string ChoiceKey,
  string ChoiceName,
  string OptionKey,
  string OptionName,
  string Source,
  int? Level,
  Guid? ParentId,
  ReplacePolicyEnum ReplacePolicy,
  bool Removable
);
