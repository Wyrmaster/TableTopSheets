namespace TableTopSheets.Api.Contracts;

/// <summary>User without credentials</summary>
public sealed record UserDto(Guid Id, string UserName, string? Email);
