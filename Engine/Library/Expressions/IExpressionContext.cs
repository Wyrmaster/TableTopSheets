namespace TableTopSheets.Engine.Expressions;

/// <summary>
///   Supplies identifiers and functions to an expression
/// </summary>
public interface IExpressionContext
{
  #region Methods

  /// <summary>
  ///   Resolve an identifier (<c>level</c>, <c>pb</c>, <c>option.level</c> ...)
  /// </summary>
  ExprValue Resolve(string identifier);

  /// <summary>
  ///   Call a function (<c>mod('dex')</c>, <c>scale('wizard.cantrips')</c> ...)
  /// </summary>
  ExprValue Call(string function, IReadOnlyList<ExprValue> arguments);

  #endregion
}
