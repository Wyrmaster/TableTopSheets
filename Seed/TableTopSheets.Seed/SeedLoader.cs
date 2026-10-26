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

  public static SeedDocument Load(string? directory = null)
  {
    directory ??= DefaultDirectory;
    SeedDocument merged = new();
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
    }

    return merged;
  }

  #endregion
}
