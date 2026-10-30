using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Replace an existing selection whose choice allows one swap per level up
/// </summary>
public sealed record ReplacementRequest(Guid SelectionId, string OptionKey);
