using TableTopSheets.Common.Enum.Rules;

namespace TableTopSheets.Api.Contracts;

/// <summary>An item that can be picked</summary>
public sealed record ItemOptionDto(string Key, string Name);
