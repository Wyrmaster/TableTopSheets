namespace TableTopSheets.Api.Contracts;

/// <summary>New user of the database authentication</summary>
public sealed record RegisterDto(string UserName, string Password, string? Email = null);

/// <summary>Credentials of the database authentication</summary>
public sealed record LoginDto(string UserName, string Password);

/// <summary>User without credentials</summary>
public sealed record UserDto(Guid Id, string UserName, string? Email);

/// <summary>Signed in user with the number of characters they own</summary>
public sealed record CurrentUserDto(Guid Id, string UserName, string? Email, int CharacterCount);

/// <summary>
///   Access token for the <c>Authorization: Bearer</c> header
/// </summary>
public sealed record TokenDto(string AccessToken, string TokenType, DateTime ExpiresAt, UserDto User);
