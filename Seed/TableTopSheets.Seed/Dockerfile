# TableTopSheets ruleset migrator: applies the schema migrations and writes the rules into the database, then exits.
# docker compose runs it before the API (see compose.api.yaml)
# build from the repository root: docker build -f Seed/TableTopSheets.Seed/Dockerfile -t tabletopsheets-migrator .

ARG DOTNET_VERSION=10.0

# runtime image; first stage, so IDE fast-mode debugging (which builds only the first stage) gets a dotnet runtime
FROM mcr.microsoft.com/dotnet/runtime:${DOTNET_VERSION} AS base
WORKDIR /app

# Npgsql loads the GSSAPI library when it connects (logs an error without it)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# SQLite database lives here; mount the same volume as the API
RUN mkdir -p /app/data && chown $APP_UID /app/data
VOLUME /app/data

ENV Database__Provider=Sqlite \
    ConnectionStrings__TableTopSheets="Data Source=/app/data/tabletopsheets.db"

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# project files first, so the restore layer is reused until a dependency changes
COPY Common/TableTopSheets.Common/TableTopSheets.Common.csproj Common/TableTopSheets.Common/
COPY Engine/TableTopSheets.Engine/TableTopSheets.Engine.csproj Engine/TableTopSheets.Engine/
COPY Persistence/TableTopSheets.Persistence/TableTopSheets.Persistence.csproj Persistence/TableTopSheets.Persistence/
COPY Persistence/TableTopSheets.Persistence.Postgresql/TableTopSheets.Persistence.Postgresql.csproj Persistence/TableTopSheets.Persistence.Postgresql/
COPY Persistence/TableTopSheets.Persistence.Sqlite/TableTopSheets.Persistence.Sqlite.csproj Persistence/TableTopSheets.Persistence.Sqlite/
COPY Seed/TableTopSheets.Seed/TableTopSheets.Seed.csproj Seed/TableTopSheets.Seed/
RUN dotnet restore Seed/TableTopSheets.Seed/TableTopSheets.Seed.csproj

COPY Common/ Common/
COPY Engine/ Engine/
COPY Persistence/ Persistence/
COPY Seed/ Seed/
RUN dotnet publish Seed/TableTopSheets.Seed/TableTopSheets.Seed.csproj \
      --configuration $BUILD_CONFIGURATION \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

FROM base AS final

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TableTopSheets.Seed.dll"]
CMD ["migrate"]
