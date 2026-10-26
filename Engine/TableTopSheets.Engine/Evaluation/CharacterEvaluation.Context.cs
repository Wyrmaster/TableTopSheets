using TableTopSheets.Engine.Expressions;
using TableTopSheets.Persistence.Entity.Characters;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Engine.Evaluation;

/// <summary>
///   Identifiers and functions available to rule expressions
/// </summary>
public sealed partial class CharacterEvaluation: IExpressionContext
{
  #region Fields

  /// <summary>
  ///   Identifiers resolved by <see cref="Resolve"/>
  /// </summary>
  public static readonly IReadOnlySet<string> KnownIdentifiers = new HashSet<string>
  {
    "level", "pb", "half", "proficient", "expertise", "spellcaster", "true", "false", "armor.ac", "shield.ac",
  };

  /// <summary>
  ///   Functions resolved by <see cref="Call"/> and the built in helpers
  /// </summary>
  public static readonly IReadOnlySet<string> KnownFunctions = new HashSet<string>
  {
    "floor", "ceil", "min", "max", "oneOf",
    "classLevel", "subclass", "score", "mod", "scale", "stat", "wearing", "wielding", "equipped", "attuned",
    "has", "chosen", "proficient", "skillProficiency", "active", "maxSpellLevel",
  };

  #endregion

  #region IExpressionContext

  public ExprValue Resolve(string identifier) => identifier switch
  {
    "level" => ExprValue.Of(this.TotalLevel),
    "pb" => ExprValue.Of(this.ProficiencyBonus),
    "half" => ExprValue.Of(5),
    "proficient" => ExprValue.Of(10),
    "expertise" => ExprValue.Of(20),
    "spellcaster" => ExprValue.Of(this.SpellcastingProfiles.Count > 0),
    "armor.ac" => ExprValue.Of(this.WornArmor?.BaseAc ?? 0),
    "shield.ac" => ExprValue.Of(this.WieldedShield?.BaseAc ?? 0),
    _ => throw new ExpressionException($"Unknown identifier '{identifier}'"),
  };

  public ExprValue Call(string function, IReadOnlyList<ExprValue> arguments)
  {
    string Argument() => arguments.Count == 1
      ? arguments[0].AsString()
      : throw new ExpressionException($"{function} expects one argument");

    return function switch
    {
      "classLevel" => ExprValue.Of(this.ClassLevel(Argument())),
      "subclass" => ExprValue.Of(this.Subclasses.Values.Any(entry => entry.Key == Argument())),
      "score" => ExprValue.Of(this.Score(Argument())),
      "mod" => ExprValue.Of(this.Modifier(Argument())),
      "scale" => this.Scale(Argument()),
      "stat" => ExprValue.Of(this.Stat(Argument())),
      "wearing" => ExprValue.Of(this.Wearing(Argument())),
      "wielding" => ExprValue.Of(this.Wielding(Argument())),
      "equipped" => ExprValue.Of(this.EquippedItems.Any(entry => this.Catalog.Items[entry.ItemId].Key == Argument())),
      "attuned" => ExprValue.Of(this.Character.Items.Any(entry => entry.Attuned && this.Catalog.Items[entry.ItemId].Key == Argument())),
      "has" => ExprValue.Of(this.HasFeature(Argument())),
      "chosen" => ExprValue.Of(this._selections.Any(entry => entry.Owner.Active && entry.Selection.OptionKey == Argument())),
      "proficient" => ExprValue.Of(this.Stat($"proficiency.{Argument()}") > 0),
      "skillProficiency" => ExprValue.Of(this.SkillProficiency(Argument())),
      "active" => ExprValue.Of(this.Character.ActiveToggles.Contains(Argument())),
      "maxSpellLevel" => ExprValue.Of(this.MaxSpellLevel(Argument())),
      _ => throw new ExpressionException($"Unknown function '{function}'"),
    };
  }

  #endregion

  #region Methods

  /// <summary>
  ///   Value of a class table column at the current level of its class (total level for scales without class)
  /// </summary>
  public ExprValue Scale(string key)
  {
    if (!this.Catalog.ScaleValues.TryGetValue(key, out IReadOnlyList<ScaleValue>? values))
    {
      throw new ExpressionException($"Unknown scale '{key}'");
    }

    int level = values[0].ClassId is { } classId ? this.ClassLevel(classId) : this.TotalLevel;
    ScaleValue? value = values.LastOrDefault(entry => entry.Level <= level);
    return value == null ? ExprValue.Zero : ExprValue.Parse(value.Value);
  }

  #endregion
}
