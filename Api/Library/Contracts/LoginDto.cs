namespace TableTopSheets.Api.Contracts;

/// <summary>Credentials of the database authentication</summary>
public sealed record LoginDto(string UserName, string Password);
