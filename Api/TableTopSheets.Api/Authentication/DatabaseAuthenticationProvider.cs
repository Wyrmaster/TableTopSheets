using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Api.Contracts;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Api.Authentication;

/// <summary>
///   Users stored in the database with a salted password hash (PBKDF2, <see cref="PasswordHasher{TUser}"/>); the API issues
///   signed JWTs after register and login
/// </summary>
public sealed class DatabaseAuthenticationProvider(AuthenticationOptions options, byte[] signingKey) : IAuthenticationProvider
{
  #region Constants

  public const int MinimumUserNameLength = 3;
  public const int MaximumUserNameLength = 64;

  #endregion

  #region Fields

  private static readonly PasswordHasher<User> Hasher = new();

  private readonly SymmetricSecurityKey _key = new(signingKey);

  #endregion

  #region IAuthenticationProvider

  public void ConfigureJwtBearer(JwtBearerOptions jwt)
  {
    jwt.MapInboundClaims = false;
    jwt.TokenValidationParameters = new TokenValidationParameters
    {
      ValidIssuer = options.Jwt.Issuer,
      ValidAudience = options.Jwt.Audience,
      IssuerSigningKey = this._key,
      NameClaimType = JwtClaims.Name,
    };
  }

  public void MapEndpoints(RouteGroupBuilder group)
  {
    if (options.Database.AllowRegistration)
    {
      group
        .MapPost("/register", this.Register)
        .WithName("Register")
        .WithSummary("Create a user and sign in")
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);
    }

    group
      .MapPost("/login", this.Login)
      .WithName("Login")
      .WithSummary("Sign in with user name and password")
      .ProducesProblem(StatusCodes.Status401Unauthorized);
  }

  public async Task<User?> ResolveUserAsync(ClaimsPrincipal principal, TableTopSheetsContext context, CancellationToken cancellationToken)
    => Guid.TryParse(principal.Value(JwtClaims.Subject), out Guid id)
      ? await context.Users.SingleOrDefaultAsync(entry => entry.Id == id, cancellationToken)
      : null;

  #endregion

  #region Endpoints

  private async Task<Results<Ok<TokenDto>, ValidationProblem, ProblemHttpResult>> Register(TableTopSheetsContext context, RegisterDto dto, CancellationToken cancellationToken)
  {
    string userName = dto.UserName?.Trim() ?? string.Empty;
    Dictionary<string, string[]> errors = new();
    if (userName.Length is < MinimumUserNameLength or > MaximumUserNameLength)
    {
      errors[nameof(RegisterDto.UserName)] = [$"The user name needs {MinimumUserNameLength} to {MaximumUserNameLength} characters"];
    }

    if ((dto.Password?.Length ?? 0) < options.Database.MinimumPasswordLength)
    {
      errors[nameof(RegisterDto.Password)] = [$"The password needs at least {options.Database.MinimumPasswordLength} characters"];
    }

    if (errors.Count > 0)
    {
      return TypedResults.ValidationProblem(errors);
    }

    if (await FindAsync(context, userName, cancellationToken) != null)
    {
      return TypedResults.Problem(statusCode: StatusCodes.Status409Conflict, title: "User name taken", detail: $"The user name '{userName}' is already taken");
    }

    User user = new()
    {
      Id = Guid.NewGuid(),
      UserName = userName,
      Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),
      CreatedAt = DateTime.UtcNow,
    };
    user.PasswordHash = Hasher.HashPassword(user, dto.Password!);
    context.Users.Add(user);
    await context.SaveChangesAsync(cancellationToken);
    return TypedResults.Ok(this.Issue(user));
  }

  private async Task<Results<Ok<TokenDto>, ProblemHttpResult>> Login(TableTopSheetsContext context, LoginDto dto, CancellationToken cancellationToken)
  {
    User? user = await FindAsync(context, dto.UserName?.Trim() ?? string.Empty, cancellationToken);
    PasswordVerificationResult result = user?.PasswordHash == null || dto.Password == null
      ? PasswordVerificationResult.Failed
      : Hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
    if (user == null || result == PasswordVerificationResult.Failed)
    {
      return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Sign in failed", detail: "Unknown user name or wrong password");
    }

    if (result == PasswordVerificationResult.SuccessRehashNeeded)
    {
      user.PasswordHash = Hasher.HashPassword(user, dto.Password!);
      await context.SaveChangesAsync(cancellationToken);
    }

    return TypedResults.Ok(this.Issue(user));
  }

  #endregion

  #region Helpers

  /// <summary>
  ///   User names are unique regardless of case
  /// </summary>
  private static Task<User?> FindAsync(TableTopSheetsContext context, string userName, CancellationToken cancellationToken)
  {
    string lower = userName.ToLowerInvariant();
    return context.Users.SingleOrDefaultAsync(entry => entry.UserName.ToLower() == lower, cancellationToken);
  }

  private TokenDto Issue(User user)
  {
    DateTime expires = DateTime.UtcNow.Add(options.Jwt.Lifetime);
    string token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
      Issuer = options.Jwt.Issuer,
      Audience = options.Jwt.Audience,
      Expires = expires,
      SigningCredentials = new SigningCredentials(this._key, SecurityAlgorithms.HmacSha256),
      Claims = new Dictionary<string, object>
      {
        [JwtClaims.Subject] = user.Id.ToString(),
        [JwtClaims.Name] = user.UserName,
        [JwtClaims.TokenId] = Guid
          .NewGuid()
          .ToString(),
      },
    });
    return new TokenDto(token, "Bearer", expires, new UserDto(user.Id, user.UserName, user.Email));
  }

  /// <summary>
  ///   Bytes of the configured signing key; a random key in Development when none is configured
  /// </summary>
  public static byte[] SigningKey(JwtOptions jwt, IHostEnvironment environment)
  {
    if (!string.IsNullOrEmpty(jwt.SigningKey))
    {
      byte[] key = Encoding.UTF8.GetBytes(jwt.SigningKey);
      return key.Length >= 32
        ? key
        : throw new InvalidOperationException($"{AuthenticationOptions.Section}:Jwt:SigningKey needs at least 32 characters");
    }

    return environment.IsDevelopment()
      ? System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)
      : throw new InvalidOperationException($"{AuthenticationOptions.Section}:Jwt:SigningKey is required for the database authentication");
  }

  #endregion
}
