using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Ability score with its modifier</summary>
public sealed record AbilityDto(string Key, string Name, int Score, int Modifier);
