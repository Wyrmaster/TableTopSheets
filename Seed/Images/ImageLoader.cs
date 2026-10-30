using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Images;

/// <summary>
///   Reads the artwork of the seed: <c>Images/&lt;category&gt;/&lt;key&gt;.webp</c> with its thumbnail
///   <c>&lt;key&gt;.thumb.webp</c> (written by <c>Tools/images/convert-images.sh</c>)
/// </summary>
public static class ImageLoader
{
  #region Constants

  private const string ThumbnailSuffix = ".thumb.webp";

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
        .EnumerateFiles(path, "*.webp")
        .Where(entry => !entry.EndsWith(ThumbnailSuffix, StringComparison.Ordinal))
        .Order(StringComparer.Ordinal))
      {
        images.Add(Read(category, folder, file));
      }
    }

    return images;
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
    (int width, int height) = WebpSize(data)
      ?? throw new InvalidDataException($"'{file}' is not a WebP image");

    using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    hash.AppendData(data);
    hash.AppendData(thumbnail);

    return new RuleImage
    {
      Id = SeedKeys.Id("image", $"{folder}/{key}"),
      Category = category,
      OwnerKey = key,
      Title = Title(key),
      ContentType = "image/webp",
      Width = width,
      Height = height,
      Data = data,
      ThumbnailData = thumbnail,
      ContentHash = Convert.ToHexStringLower(hash.GetHashAndReset()),
    };
  }

  /// <summary>
  ///   Width and height from the header of a WebP file (lossy <c>VP8</c>, lossless <c>VP8L</c> or extended <c>VP8X</c>)
  /// </summary>
  private static (int Width, int Height)? WebpSize(byte[] data)
  {
    if (data.Length < 30 || Encoding.ASCII.GetString(data, 0, 4) != "RIFF" || Encoding.ASCII.GetString(data, 8, 4) != "WEBP")
    {
      return null;
    }

    ReadOnlySpan<byte> bytes = data;
    return Encoding.ASCII.GetString(data, 12, 4) switch
    {
      "VP8 " => (BinaryPrimitives.ReadUInt16LittleEndian(bytes[26..]) & 0x3FFF, BinaryPrimitives.ReadUInt16LittleEndian(bytes[28..]) & 0x3FFF),
      "VP8L" => LosslessSize(BinaryPrimitives.ReadUInt32LittleEndian(bytes[21..])),
      "VP8X" => (1 + (bytes[24] | (bytes[25] << 8) | (bytes[26] << 16)), 1 + (bytes[27] | (bytes[28] << 8) | (bytes[29] << 16))),
      _ => null,
    };
  }

  private static (int Width, int Height) LosslessSize(uint bits)
    => (1 + (int)(bits & 0x3FFF), 1 + (int)((bits >> 14) & 0x3FFF));

  #endregion
}
