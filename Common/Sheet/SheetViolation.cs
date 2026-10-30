using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

/// <summary>
///   Rule problem found while computing a sheet
/// </summary>
public sealed record SheetViolation(string Code, string Message, ViolationSeverityEnum Severity = ViolationSeverityEnum.Warning);
