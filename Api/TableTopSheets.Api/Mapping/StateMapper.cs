using TableTopSheets.Api.Contracts;
using TableTopSheets.Common.Key;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Api.Mapping;

/// <summary>
///   Maps an evaluated character to API contracts
/// </summary>
public static class StateMapper
{
  #region Methods

  public static CharacterStateDto State(CharacterEvaluation evaluation, string? preview = null)
  {
    Character character = evaluation.Character;
    CharacterSheet sheet = CharacterSheetBuilder.Build(evaluation);
    IReadOnlyList<PendingChoiceDto> pending = ChoiceMapper.Pending(evaluation, preview);

    return new CharacterStateDto
    {
      Id = character.Id,
      Name = character.Name,
      Level = evaluation.TotalLevel,
      Species = sheet.Species,
      Background = sheet.Background,
      Alignment = character.Alignment,
      Classes = Classes(evaluation),
      Abilities = RuleKeys.Abilities
        .Select(entry => new AbilityDto(entry, Naming.Title(entry), evaluation.Score(entry), evaluation.Modifier(entry)))
        .ToList(),
      ProficiencyBonus = evaluation.ProficiencyBonus,
      HitPointMaximum = sheet.HitPointMaximum,
      ArmorClass = sheet.ArmorClass,
      Items = character.Items.Select(entry => Item(evaluation, entry)).ToList(),
      Coins = new CoinsDto(sheet.Coins.Copper, sheet.Coins.Silver, sheet.Coins.Electrum, sheet.Coins.Gold, sheet.Coins.Platinum),
      BackgroundEquipment = character.BackgroundEquipment,
      ClassEquipment = character.ClassEquipment,
      Complete = pending.All(entry => entry.Optional),
      PendingChoices = pending,
      Violations = sheet.Violations.Select(entry => new ViolationDto(entry.Code, entry.Message, entry.Severity)).ToList(),
    };
  }

  /// <summary>
  ///   Summary of a character with its levels; the names come from the given rule rows instead of an evaluation
  /// </summary>
  public static CharacterSummaryDto Summary
  (
    Character character,
    IReadOnlyDictionary<Guid, CharacterClass> classes,
    IReadOnlyDictionary<Guid, string> species,
    IReadOnlyDictionary<Guid, string> backgrounds
  )
  {
    List<CharacterLevel> levels = character.Levels.OrderBy(entry => entry.Number).ToList();
    return new CharacterSummaryDto
    (
      character.Id,
      character.Name,
      levels.Count,
      character.SpeciesId is { } speciesId ? species[speciesId] : string.Empty,
      character.BackgroundId is { } backgroundId ? backgrounds[backgroundId] : string.Empty,
      levels
        .GroupBy(entry => entry.ClassId)
        .Select(group =>
        {
          CharacterClass @class = classes[group.Key];
          Guid? subclassId = group.Select(entry => entry.SubclassId).LastOrDefault(entry => entry != null);
          string? subclass = subclassId is { } id ? @class.Subclasses.Single(entry => entry.Id == id).Key : null;
          return new ClassLevelDto(@class.Key, @class.Name, group.Count(), subclass);
        })
        .ToList()
    );
  }

  public static IReadOnlyList<ClassLevelDto> Classes(CharacterEvaluation evaluation)
    => evaluation.ClassLevels
      .Select(entry =>
      {
        CharacterClass @class = evaluation.Catalog.Classes[entry.Key];
        return new ClassLevelDto(@class.Key, @class.Name, entry.Value, evaluation.Subclasses.GetValueOrDefault(entry.Key)?.Key);
      })
      .ToList();

  /// <summary>
  ///   Active selections; only those of the latest level (or of the origin without levels) can be removed
  /// </summary>
  public static IReadOnlyList<SelectionInfoDto> Selections(CharacterEvaluation evaluation)
  {
    Guid? latest = evaluation.Levels.MaxBy(entry => entry.Number)?.Id;
    return evaluation.Selections.Select(entry => Selection(evaluation, entry, entry.Selection.LevelId == latest)).ToList();
  }

  public static SelectionInfoDto Selection(CharacterEvaluation evaluation, SelectionInstance selection, bool removable) => new
  (
    selection.Selection.Id,
    selection.Choice.Key,
    selection.Choice.Name,
    selection.Selection.OptionKey,
    evaluation.OptionName(selection.Choice, selection.Selection.OptionKey),
    selection.Owner.Source,
    evaluation.Levels.FirstOrDefault(level => level.Id == selection.Selection.LevelId)?.Number,
    selection.Selection.ParentId,
    selection.Choice.ReplacePolicy,
    removable
  );

  private static CharacterItemDto Item(CharacterEvaluation evaluation, CharacterItem entry)
  {
    Item item = evaluation.Catalog.Items[entry.ItemId];
    return new CharacterItemDto(entry.Id, item.Key, item.Name, item.Kind, entry.Quantity, entry.EquipSlot, entry.Attuned, item.RequiresAttunement, entry.Notes);
  }

  #endregion
}
