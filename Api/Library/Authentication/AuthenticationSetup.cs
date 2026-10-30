using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using TableTopSheets.Api.Configuration;
using TableTopSheets.Engine.Services;

namespace TableTopSheets.Api.Authentication;

public static class AuthenticationSetup
{
  #region Constants

  private const string SchemeName = "Bearer";

  #endregion

  #region Methods

  /// <summary>
  ///   Registers the <see cref="IAuthenticationProvider"/> chosen by <c>Authentication:Provider</c>, bearer token
  ///   authentication with its validation and the current user of a request
  /// </summary>
  public static WebApplicationBuilder AddTableTopSheetsAuthentication(this WebApplicationBuilder builder)
  {
    AuthenticationOptions options = builder.Configuration
      .GetSection(AuthenticationOptions.Section)
      .Get<AuthenticationOptions>() ?? new AuthenticationOptions();
    IAuthenticationProvider provider = options.Provider switch
    {
      AuthenticationProvider.Oidc => new OidcAuthenticationProvider(options),
      _ => new DatabaseAuthenticationProvider(options, DatabaseAuthenticationProvider.SigningKey(options.Jwt, builder.Environment)),
    };

    builder.Services.AddSingleton(options);
    builder.Services.AddSingleton(provider);
    builder.Services.AddScoped<CurrentUser>();
    builder.Services.AddScoped<ICurrentUser>(services => services.GetRequiredService<CurrentUser>());
    builder.Services
      .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
      .AddJwtBearer(provider.ConfigureJwtBearer);
    builder.Services.AddAuthorization();
    return builder;
  }

  /// <summary>
  ///   Bearer scheme in the OpenAPI document, so the Swagger UI can send a token
  /// </summary>
  public static void AddBearerScheme(this Microsoft.AspNetCore.OpenApi.OpenApiOptions options)
    => options.AddDocumentTransformer((document, _, _) =>
    {
      document.Components ??= new OpenApiComponents();
      document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
      document.Components.SecuritySchemes[SchemeName] = new OpenApiSecurityScheme
      {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Access token from POST /api/auth/login (database authentication) or from the identity provider (OIDC)",
      };
      document.Security = [new OpenApiSecurityRequirement { [new OpenApiSecuritySchemeReference(SchemeName, document)] = [] }];
      return Task.CompletedTask;
    });

  #endregion
}
