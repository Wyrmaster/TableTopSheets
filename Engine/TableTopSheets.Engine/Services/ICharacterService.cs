using Microsoft.EntityFrameworkCore;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Common.Key;
using TableTopSheets.Common.Sheet;
using TableTopSheets.Engine.Choices;
using TableTopSheets.Engine.Evaluation;
using TableTopSheets.Engine.Rules;
using TableTopSheets.Engine.Sheet;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Engine.Services;

/// <summary>
///   Contract of <see cref="CharacterService"/>
/// </summary>
public interface ICharacterService
{
  Task<Character> LoadAsync(Guid characterId, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Evaluate a character against the rules it needs, including the descriptions of the options its pending choices embed
  /// </summary>
  Task<CharacterEvaluation> EvaluateAsync(Character character, CancellationToken cancellationToken = default);

  Task<CharacterSheet> BuildSheetAsync(Guid characterId, CancellationToken cancellationToken = default);

  Task<Character> CreateAsync(CreateCharacterRequest request, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Change the descriptive fields of a character (null keeps the current value); nothing rule relevant changes
  /// </summary>
  Task<CharacterEvaluation> UpdateDetailsAsync(Guid characterId, CharacterDetailsUpdate update, CancellationToken cancellationToken = default);

  Task DeleteAsync(Guid characterId, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Answer open choices. The selections belong to the latest level (origin when the character has no level yet),
  ///   so undoing that level removes them as well
  /// </summary>
  Task<CharacterEvaluation> SelectAsync(Guid characterId, IReadOnlyList<SelectionRequest> selections, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Answer open choices of a loaded character (in memory, saved by the caller)
  /// </summary>
  Task SelectAsync(Character character, IReadOnlyList<SelectionRequest> selections, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Throw when required choices are still open
  /// </summary>
  Task EnsureCompleteAsync(Character character, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Validate selections without saving them; throws <see cref="RuleViolationException"/> when the rules are violated
  /// </summary>
  Task<T> ValidateSelectionsAsync<T>(Guid characterId, IReadOnlyList<SelectionRequest> selections, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Take back a selection (and the selections scoped to it). Only selections of the latest level (or of the origin when the
  ///   character has no level) can be removed; older ones are removed by undoing levels
  /// </summary>
  Task<CharacterEvaluation> DeselectAsync(Guid characterId, Guid selectionId, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Choices the character will have to answer when gaining a level in the class (nothing is saved)
  /// </summary>
  Task<IReadOnlyList<PendingChoice>> PreviewLevelUpAsync(Guid characterId, string classKey, string? subclassKey = null, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Evaluate the character as if it gained a level in the class and project the result (nothing is saved)
  /// </summary>
  Task<T> PreviewLevelUpAsync<T>(Guid characterId, string classKey, string? subclassKey, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Validate a level up without saving it; throws <see cref="RuleViolationException"/> when the rules are violated
  /// </summary>
  Task<T> ValidateLevelUpAsync<T>(Guid characterId, LevelUpRequest request, Func<CharacterEvaluation, T> project, CancellationToken cancellationToken = default);

  Task<CharacterEvaluation> LevelUpAsync(Guid characterId, LevelUpRequest request, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Apply a level up to a loaded character (in memory, saved by the caller)
  /// </summary>
  Task LevelUpAsync(Character character, LevelUpRequest request, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Remove the highest level: its selections are deleted and selections it replaced become active again.
  ///   Loadouts that became invalid are reported as violations of the returned evaluation
  /// </summary>
  Task<CharacterEvaluation> UndoLastLevelAsync(Guid characterId, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Change a selection in place when its choice allows changes on a rest (Wizard cantrips ...)
  /// </summary>
  Task<CharacterEvaluation> ReplaceOnRestAsync(Guid characterId, Guid selectionId, string optionKey, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Replace the long rest prepared spell list of a class
  /// </summary>
  Task<CharacterEvaluation> SetPreparedSpellsAsync(Guid characterId, string classKey, IReadOnlyList<string> spellKeys, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Copy a spell into the spellbook (scroll or another book)
  /// </summary>
  Task<CharacterEvaluation> CopyToSpellbookAsync(Guid characterId, string spellKey, string? note = null, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Add an item to the inventory; a purchase pays its cost from the character's coins (with change)
  /// </summary>
  Task<CharacterItem> AddItemAsync(Guid characterId, string itemKey, int quantity = 1, CancellationToken cancellationToken = default, bool purchase = false);

  /// <summary>
  ///   Remove an item (or part of a stack) from the inventory; selling credits half of its cost
  /// </summary>
  /// <param name="quantity">how many to remove; null removes the whole stack</param>
  /// <param name="sell">credit half of the item's cost per piece to the coins (the usual selling price)</param>
  Task<CharacterEvaluation> RemoveItemAsync(Guid characterId, Guid characterItemId, int? quantity = null, bool sell = false, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Starting equipment the character can take: from the background, and from the first class once it has a level
  /// </summary>
  Task<IReadOnlyList<StartingEquipmentOffer>> GetStartingEquipmentAsync(Guid characterId, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Take one alternative of the starting equipment of the background or the first class: its items are added to the
  ///   inventory and its coins to the purse. Each source gives its equipment once
  /// </summary>
  Task<CharacterEvaluation> TakeStartingEquipmentAsync(
    Guid characterId,
    StartingEquipmentSourceEnum source,
    StartingEquipmentRequest request,
    CancellationToken cancellationToken = default);

  /// <summary>
  ///   Replace all coins of the character
  /// </summary>
  Task<CharacterEvaluation> SetCoinsAsync(Guid characterId, int copper, int silver, int electrum, int gold, int platinum, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Add (positive) or remove (negative) coins of each kind
  /// </summary>
  Task<CharacterEvaluation> ChangeCoinsAsync(Guid characterId, int copper, int silver, int electrum, int gold, int platinum, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Pay an amount in copper pieces; coins are spent and change is returned
  /// </summary>
  Task<CharacterEvaluation> PayAsync(Guid characterId, long amountCp, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Don, doff, wield, ready or stow an item. Any number of weapons can be ready; armor and each hand hold one item.
  ///   Equipping into an occupied hand puts a weapon held there back to ready (other items are stowed)
  /// </summary>
  Task<CharacterEvaluation> EquipAsync(Guid characterId, Guid characterItemId, EquipSlotEnum slot, CancellationToken cancellationToken = default);

  Task<CharacterEvaluation> AttuneAsync(Guid characterId, Guid characterItemId, bool attuned, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Switch a toggleable feature (Rage, Mage Armor ...) on or off
  /// </summary>
  Task<CharacterEvaluation> ToggleAsync(Guid characterId, string featureKey, bool active, CancellationToken cancellationToken = default);

  /// <summary>
  ///   Pick the Armor Class calculation by its label (null = best)
  /// </summary>
  Task<CharacterEvaluation> ChooseArmorClassAsync(Guid characterId, string? label, CancellationToken cancellationToken = default);
}
