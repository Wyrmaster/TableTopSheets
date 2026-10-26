using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Starting equipment a character can take from its background or first class
/// </summary>
/// <param name="Source">background or class</param>
/// <param name="OwnerKey">key of the background or class</param>
/// <param name="OwnerName">name of the background or class</param>
/// <param name="Options">the alternatives ("Choose A or B")</param>
/// <param name="Taken">key of the alternative already taken (null = not taken yet)</param>
/// <param name="SameAs">item already selected per "same as above" choice (choice key → item key)</param>
public sealed record StartingEquipmentOffer
(
  StartingEquipmentSourceEnum Source,
  string OwnerKey,
  string OwnerName,
  IReadOnlyList<StartingEquipmentOption> Options,
  string? Taken,
  IReadOnlyDictionary<string, string> SameAs
);
