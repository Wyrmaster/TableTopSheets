using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Images;

/// <summary>
///   Reads the artwork of the seed: <c>Images/&lt;category&gt;/&lt;key&gt;.jpg</c> with its thumbnail
///   <c>&lt;key&gt;.thumb.jpg</c> (written by <c>Tools/images/convert-images.sh</c>)
/// </summary>
public static class ImageLoader
{
  #region Constants

  private const string ThumbnailSuffix = ".thumb.jpg";

  /// <summary>
  ///   Folder below <c>Images</c> per category
  /// </summary>
  public static readonly IReadOnlyDictionary<string, ImageCategoryEnum> Folders = new Dictionary<string, ImageCategoryEnum>
  {
    ["spells"] = ImageCategoryEnum.Spell,
    ["classes"] = ImageCategoryEnum.Class,
    ["species"] = ImageCategoryEnum.Species,
    ["backgrounds"] = ImageCategoryEnum.Background,
    ["scenes"] = ImageCategoryEnum.Scene,
    ["items"] = ImageCategoryEnum.Item,
    ["magic-items"] = ImageCategoryEnum.Item,
    ["coins"] = ImageCategoryEnum.Coin,
  };

  #endregion

  #region Methods

  /// <summary>
  ///   Every image below <paramref name="directory"/>; empty when the folder doesn't exist
  /// </summary>
  public static List<RuleImage> Load(string directory)
  {
    List<RuleImage> images = [];
    foreach ((string folder, ImageCategoryEnum category) in Folders)
    {
      string path = Path.Combine(directory, folder);
      if (!Directory.Exists(path))
      {
        continue;
      }

      foreach (string file in Directory
        .EnumerateFiles(path, "*.jpg")
        .Where(entry => !entry.EndsWith(ThumbnailSuffix, StringComparison.Ordinal))
        .Order(StringComparer.Ordinal))
      {
        images.Add(Read(category, folder, file));
      }
    }

    // a later folder replaces the picture of the same owner (the magic items artwork the equipment picture of a Spell Scroll)
    return images
      .GroupBy(image => (image.Category, image.OwnerKey))
      .Select(group => group.Last())
      .ToList();
  }

  /// <summary>
  ///   Readable title of a key: <c>dragon-battle</c> → <c>Dragon battle</c>
  /// </summary>
  public static string Title(string key)
  {
    string text = key.Replace('-', ' ');
    return text.Length == 0 ? text : char.ToUpper(text[0], CultureInfo.InvariantCulture) + text[1..];
  }

  private static RuleImage Read(ImageCategoryEnum category, string folder, string file)
  {
    string key = Path.GetFileNameWithoutExtension(file);
    string thumbnailFile = Path.Combine(Path.GetDirectoryName(file)!, key + ThumbnailSuffix);
    byte[] data = File.ReadAllBytes(file);
    byte[] thumbnail = File.Exists(thumbnailFile) ? File.ReadAllBytes(thumbnailFile) : data;
    (int width, int height) = JpegSize(data)
      ?? throw new InvalidDataException($"'{file}' is not a JPEG image");

    using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    hash.AppendData(data);
    hash.AppendData(thumbnail);

    return new RuleImage
    {
      Id = SeedKeys.Id("image", $"{folder}/{key}"),
      Category = category,
      OwnerKey = key,
      Title = Title(key),
      ContentType = "image/jpeg",
      Width = width,
      Height = height,
      Data = data,
      ThumbnailData = thumbnail,
      ContentHash = Convert.ToHexStringLower(hash.GetHashAndReset()),
    };
  }

  /// <summary>
  ///   Width and height from the frame header (<c>SOF</c> marker) of a JPEG file
  /// </summary>
  private static (int Width, int Height)? JpegSize(byte[] data)
  {
    if (data.Length < 4 || data[0] != 0xFF || data[1] != 0xD8)
    {
      return null;
    }

    ReadOnlySpan<byte> bytes = data;
    int position = 2;
    while (position + 9 < data.Length)
    {
      if (data[position] != 0xFF)
      {
        return null;
      }

      byte marker = data[position + 1];
      // fill bytes between segments
      if (marker == 0xFF)
      {
        position++;
        continue;
      }

      int length = BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 2)..]);
      // start of frame: baseline, progressive, lossless ...; not DHT (C4), JPG (C8) or DAC (CC)
      if (marker is >= 0xC0 and <= 0xCF and not (0xC4 or 0xC8 or 0xCC))
      {
        return (BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 7)..]), BinaryPrimitives.ReadUInt16BigEndian(bytes[(position + 5)..]));
      }

      position += 2 + length;
    }

    return null;
  }

  #endregion
}
