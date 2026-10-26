namespace TableTopSheets.Api.Contracts;

/// <summary>New user of the database authentication</summary>
public sealed record RegisterDto(string UserName, string Password, string? Email = null);
