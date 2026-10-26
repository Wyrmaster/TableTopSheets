using TableTopSheets.Api.Contracts;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Maps API requests to engine requests
/// </summary>
public static class RequestMapper
{
  #region Methods

  public static IReadOnlyList<SelectionRequest> Selections(IReadOnlyList<SelectionDto>? selections)
    => (selections ?? [])
      .Select(entry => new SelectionRequest(entry.ChoiceKey, entry.OptionKey, entry.Ref, entry.ParentRef))
      .ToList();

  public static CreateCharacterRequest Create(CreateCharacterDto dto) => new()
  {
    Name = dto.Name,
    SpeciesKey = dto.SpeciesKey,
    BackgroundKey = dto.BackgroundKey,
    AbilityMethod = dto.AbilityMethod,
    Abilities = dto.Abilities,
    Alignment = dto.Alignment,
    Selections = Selections(dto.Selections),
    StartingEquipment = StartingEquipment(dto.StartingEquipment),
    AllowIncomplete = dto.AllowIncomplete,
  };

  public static LevelUpRequest LevelUp(LevelUpDto dto) => new()
  {
    ClassKey = dto.ClassKey,
    SubclassKey = dto.SubclassKey,
    HitPointRoll = dto.HitPointRoll,
    Selections = Selections(dto.Selections),
    Replacements = (dto.Replacements ?? [])
      .Select(entry => new ReplacementRequest(entry.SelectionId, entry.OptionKey))
      .ToList(),
    StartingEquipment = StartingEquipment(dto.StartingEquipment),
    AllowIncomplete = dto.AllowIncomplete,
  };

  public static StartingEquipmentRequest? StartingEquipment(TakeStartingEquipmentDto? dto)
    => dto == null ? null : new StartingEquipmentRequest(dto.OptionKey, dto.Picks);

  public static CharacterDetailsUpdate Details(UpdateCharacterDto dto) => new()
  {
    Name = dto.Name,
    Alignment = dto.Alignment,
    Appearance = dto.Appearance,
    Backstory = dto.Backstory,
    Experience = dto.Experience,
  };

  /// <summary>
  ///   Split a preview parameter (<c>class</c> or <c>class:subclass</c>)
  /// </summary>
  public static (string ClassKey, string? SubclassKey) Preview(string preview)
  {
    string[] parts = preview.Split(':', 2, StringSplitOptions.TrimEntries);
    return (parts[0], parts.Length > 1 && parts[1].Length > 0 ? parts[1] : null);
  }

  public static string Preview(string classKey, string? subclassKey) => subclassKey == null ? classKey : $"{classKey}:{subclassKey}";

  #endregion
}
