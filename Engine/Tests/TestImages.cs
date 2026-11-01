using System.Buffers.Binary;
using System.IO.Compression;

namespace TableTopSheets.Engine.Tests;

/// <summary>
///   Pictures for card tests, made without an image library
/// </summary>
public static class TestImages
{
  #region Constants

  /// <summary>
  ///   24 × 16 px JPEG with a red to blue gradient
  /// </summary>
  private const string JpegBase64 = "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAAQABgDASIAAhEBAxEB/8QAFgABAQEAAAAAAAAAAAAAAAAAAAUG/8QAFhABAQEAAAAAAAAAAAAAAAAAABNh/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAWEQADAAAAAAAAAAAAAAAAAAAAFmH/2gAMAwEAAhEDEQA/AMffS+pV9L6pLUK7BSrfRKvoLUDBT//Z";

  #endregion

  #region Fields

  private static readonly uint[] CrcTable = Enumerable
    .Range(0, 256)
    .Select(index =>
    {
      uint crc = (uint)index;
      for (int bit = 0; bit < 8; bit++)
      {
        crc = (crc & 1) != 0 ? 0xEDB88320 ^ (crc >> 1) : crc >> 1;
      }

      return crc;
    })
    .ToArray();

  #endregion

  #region Methods

  /// <summary>
  ///   Small landscape JPEG
  /// </summary>
  public static byte[] Jpeg() => Convert.FromBase64String(JpegBase64);

  /// <summary>
  ///   RGB PNG of <paramref name="width"/> × <paramref name="height"/> px with a diagonal gradient and a frame, tinted by
  ///   <paramref name="seed"/> so the cards look different
  /// </summary>
  public static byte[] Png(int width, int height, int seed = 0)
  {
    using MemoryStream raw = new();
    for (int y = 0; y < height; y++)
    {
      raw.WriteByte(0);
      for (int x = 0; x < width; x++)
      {
        bool frame = x < 2 || y < 2 || x >= width - 2 || y >= height - 2;
        raw.WriteByte(frame ? (byte)40 : (byte)((x * 255 / width + seed * 70) % 256));
        raw.WriteByte(frame ? (byte)40 : (byte)(y * 255 / height));
        raw.WriteByte(frame ? (byte)40 : (byte)((255 - x * 255 / width + seed * 40) % 256));
      }
    }

    using MemoryStream compressed = new();
    using (ZLibStream zlib = new(compressed, CompressionLevel.Optimal, leaveOpen: true))
    {
      raw.Position = 0;
      raw.CopyTo(zlib);
    }

    byte[] header = new byte[13];
    BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(0), width);
    BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
    header[8] = 8; // bit depth
    header[9] = 2; // RGB

    using MemoryStream png = new();
    png.Write([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);
    WriteChunk(png, "IHDR", header);
    WriteChunk(png, "IDAT", compressed.ToArray());
    WriteChunk(png, "IEND", []);
    return png.ToArray();
  }

  #endregion

  #region Helpers

  private static void WriteChunk(Stream stream, string type, byte[] data)
  {
    byte[] length = new byte[4];
    BinaryPrimitives.WriteInt32BigEndian(length, data.Length);
    stream.Write(length);

    byte[] typeAndData = [.. type.Select(character => (byte)character), .. data];
    stream.Write(typeAndData);

    uint crc = 0xFFFFFFFF;
    foreach (byte value in typeAndData)
    {
      crc = CrcTable[(crc ^ value) & 0xFF] ^ (crc >> 8);
    }

    byte[] checksum = new byte[4];
    BinaryPrimitives.WriteUInt32BigEndian(checksum, crc ^ 0xFFFFFFFF);
    stream.Write(checksum);
  }

  #endregion
}
