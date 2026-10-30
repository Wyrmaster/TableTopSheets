namespace TableTopSheets.Api.Contracts;

/// <summary>
///   Access token for the <c>Authorization: Bearer</c> header
/// </summary>
public sealed record TokenDto(string AccessToken, string TokenType, DateTime ExpiresAt, UserDto User);
