using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Take one alternative of a starting equipment ("Choose A or B")
/// </summary>
/// <param name="OptionKey">the alternative (<c>a</c>, <c>b</c> ...)</param>
/// <param name="Picks">
///   item picked per entry key for entries that offer a choice (a Holy Symbol, a Gaming Set); a pick of a "same as above"
///   entry also answers that choice when it is still open
/// </param>
public sealed record StartingEquipmentRequest(string OptionKey, IReadOnlyDictionary<string, string>? Picks = null);
