using System.Text.Json;
using TableTopSheets.Tools.Extractor;
using TableTopSheets.Seed;
using TableTopSheets.Seed.Model;

// Extracts rule content from the Player's Handbook (2024) markdown into draft seed files.
// Usage: TableTopSheets.Tools.Extractor <handbook.md> [output directory]
if (args.Length < 1 || !File.Exists(args[0]))
{
  Console.Error.WriteLine("Usage: TableTopSheets.Tools.Extractor <Player's Handbook (2024).md> [output directory]");
  return 1;
}

string output = args.Length > 1 ? args[1] : DefaultOutput();
Directory.CreateDirectory(output);

HandbookParser parser = new(File.ReadAllLines(args[0]));

void Write(string file, SeedDocument document)
{
  File.WriteAllText(Path.Combine(output, file), JsonSerializer.Serialize(document, SeedJson.Options));
  Console.WriteLine($"Wrote {file}");
}

Write("spells.json", new SeedDocument { Spells = parser.Spells() });
Write("feats.json", new SeedDocument { Feats = parser.Feats() });
Write("species.json", new SeedDocument { Species = parser.Species() });
Write("backgrounds.json", new SeedDocument { Backgrounds = parser.Backgrounds() });
(List<SeedClassFeatureText> texts, List<SeedFeature> options) = parser.ClassFeatures();
Write("class-features.json", new SeedDocument { ClassFeatureTexts = texts, Features = options });
Write("items.json", new SeedDocument { Items = parser.Items(), Masteries = parser.Masteries() });
Write("languages.json", new SeedDocument { Languages = parser.Languages() });
return 0;

static string DefaultOutput()
{
  string? directory = AppContext.BaseDirectory;
  while (directory != null && !File.Exists(Path.Combine(directory, "TableTopSheets.slnx")))
  {
    directory = Path.GetDirectoryName(directory);
  }

  return directory == null
    ? Path.Combine(Environment.CurrentDirectory, "Extracted")
    : Path.Combine(directory, "Seed", "Data", "Extracted");
}
