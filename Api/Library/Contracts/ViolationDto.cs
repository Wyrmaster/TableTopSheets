using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Sheet;

namespace TableTopSheets.Api.Contracts;

/// <summary>Rule violation of the current state (e.g. armor without training)</summary>
public sealed record ViolationDto(string Code, string Message, ViolationSeverityEnum Severity);
