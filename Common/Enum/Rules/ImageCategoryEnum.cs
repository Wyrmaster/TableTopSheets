namespace TableTopSheets.Common.Enum.Rules;

/// <summary>
///   What an image shows; together with the owner key it identifies the image (<c>Spell</c> + <c>fireball</c>)
/// </summary>
public enum ImageCategoryEnum
{
  Spell,
  Class,
  Species,
  Background,

  /// <summary>
  ///   Mood picture without an owner (tavern, dungeon ...), shown at random
  /// </summary>
  Scene,
  Item,

  /// <summary>
  ///   Coin of the purse, keyed like the coins of a character (<c>copper</c>, <c>silver</c>, <c>electrum</c>, <c>gold</c>,
  ///   <c>platinum</c>)
  /// </summary>
  Coin,
}
