namespace TableTopSheets.Persistence.Entity;

/// <summary>
///   Base Entity for all Entities
/// </summary>
public abstract class BaseEntity
{
  #region Properties

  /// <summary>
  ///   Id of an Entity
  /// </summary>
  public Guid Id { get; set; }

  #endregion
}
