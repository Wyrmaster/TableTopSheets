namespace TableTopSheets.Api.Contracts;

/// <summary>Mood picture (tavern, dungeon ...) the user interface shows at random</summary>
public sealed record SceneDto(string Key, string Title, int Width, int Height, string ImageUrl, string ThumbnailUrl);
