using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TableTopSheets.Seed.Model;

namespace TableTopSheets.Seed;

/// <summary>
///   Reads the seed files and merges them into one document.
///   <c>Extracted</c> (generated from the rule book) is read first, <c>Rules</c> (authored mechanics) overrides it.
///   The seed folder is not part of the build: it is the repository's <c>dnddata</c> folder or a folder mounted into the
///   container (<c>Migrator__DataDirectory</c>, <c>--data</c>)
/// </summary>
public static class SeedLoader
{
  #region Constants

  /// <summary>
  ///   Name of the seed folder in the repository root
  /// </summary>
  public const string RepositoryFolder = "dnddata";

  #endregion

  #region Properties

  /// <summary>
  ///   Seed folder when none is configured: the first <c>dnddata</c> folder above the assembly or the current directory
  ///   (a checkout of the repository)
  /// </summary>
  public static string DefaultDirectory => FindDefaultDirectory();

  #endregion

  #region Methods

  /// <summary>
  ///   Read every seed file (<c>Extracted</c> first, then <c>Rules</c>, each in ordinal order) and merge them
  /// </summary>
  public static SeedDocument Load(string? directory = null)
  {
    SeedDocument merged = new();
    foreach (string file in RequiredFiles(directory ?? DefaultDirectory))
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
    foreach (string file in RequiredFiles(directory))
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

  private static string FindDefaultDirectory()
  {
    foreach (string start in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
    {
      for (DirectoryInfo? folder = new(start); folder != null; folder = folder.Parent)
      {
        string candidate = Path.Combine(folder.FullName, RepositoryFolder);
        if (Directory.Exists(candidate))
        {
          return candidate;
        }
      }
    }

    return RepositoryFolder;
  }

  /// <summary>
  ///   The seed files; a missing or empty seed folder is an error (it would be an empty ruleset)
  /// </summary>
  private static List<string> RequiredFiles(string directory)
  {
    List<string> files = Files(directory).ToList();
    if (files.Count == 0)
    {
      throw new InvalidDataException(
        $"Seed folder '{Path.GetFullPath(directory)}' not found or without Extracted/ and Rules/ files: mount the seed "
        + "data (dnddata.zip from the GitHub releases) and point Migrator__DataDirectory or --data to it");
    }

    return files;
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
