using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>What a level up in a class would ask for</summary>
public sealed record LevelUpPreviewDto
(
  string ClassKey,
  int ClassLevel,
  int CharacterLevel,
  bool RequiresSubclass,
  IReadOnlyList<SubclassSummaryDto> Subclasses,
  IReadOnlyList<PendingChoiceDto> PendingChoices,
  IReadOnlyList<SelectionInfoDto> Replaceable
);
