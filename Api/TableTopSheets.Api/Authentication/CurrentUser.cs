using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   User of the request, set by <see cref="CurrentUserFilter"/>
/// </summary>
public sealed class CurrentUser: ICurrentUser
{
  #region Fields

  private Guid? _userId;

  #endregion

  #region Properties

  public Guid UserId
  {
    get => this._userId ?? throw new InvalidOperationException("The endpoint doesn't require a signed in user");
    set => this._userId = value;
  }

  #endregion
}
