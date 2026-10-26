using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Reads the seed files and merges them into one document.
///   <c>Data/Extracted</c> (generated from the rule book) is read first, <c>Data/Rules</c> (authored mechanics) overrides it
/// </summary>
public static class SeedLoader
{
  #region Properties

  /// <summary>
  ///   Seed directory copied next to the assembly
  /// </summary>
  public static string DefaultDirectory => Path.Combine(AppContext.BaseDirectory, "Data");

  #endregion

  #region Methods

  /// <summary>
  ///   Read every seed file (<c>Extracted</c> first, then <c>Rules</c>, each in ordinal order) and merge them
  /// </summary>
  public static SeedDocument Load(string? directory = null)
  {
    SeedDocument merged = new();
    foreach (string file in Files(directory ?? DefaultDirectory))
    {
      SeedDocument document;
      try
      {
        document = JsonSerializer.Deserialize<SeedDocument>(File.ReadAllText(file), SeedJson.Options)!;
      }
      catch (JsonException exception)
      {
        throw new InvalidDataException($"Seed file '{file}' is invalid: {exception.Message}", exception);
      }

      SeedMerger.Merge(merged, document);
    }

    return merged;
  }

  /// <summary>
  ///   SHA-256 (lower case hex) over the relative paths and contents of the seed files; it changes whenever a file is
  ///   added, removed, renamed or edited
  /// </summary>
  public static string Hash(string? directory = null)
  {
    directory ??= DefaultDirectory;
    using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
    foreach (string file in Files(directory))
    {
      string relative = Path
        .GetRelativePath(directory, file)
        .Replace('\\', '/');
      hash.AppendData(Encoding.UTF8.GetBytes(relative + "\n"));
      hash.AppendData(File.ReadAllBytes(file));
      hash.AppendData("\n"u8);
    }

    return Convert.ToHexStringLower(hash.GetHashAndReset());
  }

  private static IEnumerable<string> Files(string directory)
  {
    foreach (string folder in new[] { "Extracted", "Rules" })
    {
      string path = Path.Combine(directory, folder);
      if (!Directory.Exists(path))
      {
        continue;
      }

      foreach (string file in Directory
        .GetFiles(path, "*.json", SearchOption.AllDirectories)
        .Order(StringComparer.Ordinal))
      {
        yield return file;
      }
    }
  }

  #endregion
}
