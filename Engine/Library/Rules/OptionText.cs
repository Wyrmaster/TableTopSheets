using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Rules;

/// <summary>
///   Description html of an option and the names of the features it grants
/// </summary>
public sealed record OptionText(string? Html, IReadOnlyList<string> Grants);
