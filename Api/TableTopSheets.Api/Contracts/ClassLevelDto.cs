using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Levels of a character in one class</summary>
public sealed record ClassLevelDto(string Key, string Name, int Level, string? Subclass);
