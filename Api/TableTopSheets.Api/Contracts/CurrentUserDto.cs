namespace TableTopSheets.Api.Contracts;

/// <summary>Signed in user with the number of characters they own</summary>
public sealed record CurrentUserDto(Guid Id, string UserName, string? Email, int CharacterCount);
