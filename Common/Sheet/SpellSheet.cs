using TableTopSheets.Common.Container.Magic;
using TableTopSheets.Common.Enum;
using TableTopSheets.Common.Enum.Item;

namespace TableTopSheets.Common.Sheet;

public sealed record SpellSheet
(
  string Key,
  string Name,
  int Level,
  string CastingTime,
  string Range,
  bool Concentration,
  bool Ritual,
  bool Material,
  string Source,
  string Notes
);
