using System.Text.Json;
using TableTopSheets.Tools.Extractor;
using TableTopSheets.Seed;
using TableTopSheets.Seed.Model;

// Extracts rule content from the Player's Handbook (2024) and the magic items of the Dungeon Master's Guide (2024) markdown
// into draft seed files.
// Usage: TableTopSheets.Tools.Extractor [<handbook.md>] [output directory] [--dmg <guide.md> [--supplement <supplement.md>]]
//   --dmg          writes magic-items.json; the base items are read from the items.json of the output directory
//   --supplement   entries the guide markdown lacks or has wrong (same format, see MagicItemParser)
List<string> positional = new();
string? guide = null;
string? supplement = null;
for (int index = 0; index < args.Length; index++)
{
  switch (args[index])
  {
    case "--dmg" when index + 1 < args.Length:
      guide = args[++index];
      break;
    case "--supplement" when index + 1 < args.Length:
      supplement = args[++index];
      break;
    default:
      positional.Add(args[index]);
      break;
  }
}

string? handbook = positional.FirstOrDefault(entry => entry.EndsWith(".md", StringComparison.OrdinalIgnoreCase));
if ((handbook == null && guide == null)
    || (handbook != null && !File.Exists(handbook))
    || (guide != null && !File.Exists(guide))
    || (supplement != null && !File.Exists(supplement)))
{
  Console.Error.WriteLine("Usage: TableTopSheets.Tools.Extractor [<Player's Handbook (2024).md>] [output directory] [--dmg <Dungeon Master's Guide (2024).md> [--supplement <supplement.md>]]");
  return 1;
}

string output = positional.FirstOrDefault(entry => entry != handbook) ?? DefaultOutput();
Directory.CreateDirectory(output);

void Write(string file, SeedDocument document)
{
  File.WriteAllText(Path.Combine(output, file), JsonSerializer.Serialize(document, SeedJson.Options));
  Console.WriteLine($"Wrote {file}");
}

if (handbook != null)
{
  HandbookParser parser = new(File.ReadAllLines(handbook));
  Write("spells.json", new SeedDocument { Spells = parser.Spells() });
  Write("feats.json", new SeedDocument { Feats = parser.Feats() });
  Write("species.json", new SeedDocument { Species = parser.Species() });
  Write("backgrounds.json", new SeedDocument { Backgrounds = parser.Backgrounds() });
  (List<SeedClassFeatureText> texts, List<SeedFeature> options) = parser.ClassFeatures();
  Write("class-features.json", new SeedDocument { ClassFeatureTexts = texts, Features = options });
  Write("items.json", new SeedDocument { Items = parser.Items(), Masteries = parser.Masteries() });
  Write("languages.json", new SeedDocument { Languages = parser.Languages() });
}

if (guide != null)
{
  string items = Path.Combine(output, "items.json");
  SeedDocument mundane = File.Exists(items)
    ? JsonSerializer.Deserialize<SeedDocument>(File.ReadAllText(items), SeedJson.Options)!
    : new SeedDocument();
  MagicItemParser parser = new(
    File.ReadAllLines(guide),
    supplement == null ? [] : File.ReadAllLines(supplement),
    mundane.Items.Select(entry => entry.Key));
  Write("magic-items.json", new SeedDocument { Items = parser.Items() });
  foreach (string warning in parser.Warnings)
  {
    Console.Error.WriteLine($"warning: {warning}");
  }
}

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
    : Path.Combine(directory, "dnddata", "Extracted");
}
