namespace TableTopSheets.Api.Contracts;

/// <summary>Picture of a character; the type (JPEG, PNG or WebP) is recognised from the data</summary>
public sealed record SetPortraitDto
{
  /// <summary>The image file, base64 encoded</summary>
  public required string Data { get; init; }
}
