using TableTopSheets.Engine.Rules;

namespace TableTopSheets.Api.Startup;

/// <summary>
///   Holds the catalog loaded by <see cref="DatabaseInitializer"/> after the database was migrated and seeded
/// </summary>
public sealed class RulesCatalogHolder
{
  #region Fields

  private RulesCatalog? _catalog;

  #endregion

  #region Properties

  public RulesCatalog Catalog
  {
    get => this._catalog ?? throw new InvalidOperationException("The rules catalog is not loaded yet");
    set => this._catalog = value;
  }

  #endregion
}
