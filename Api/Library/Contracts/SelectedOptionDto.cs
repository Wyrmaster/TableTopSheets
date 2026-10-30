using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>A selection made for a choice instance</summary>
public sealed record SelectedOptionDto(Guid SelectionId, string OptionKey, string Name);
