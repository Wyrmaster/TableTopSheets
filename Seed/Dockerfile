# TableTopSheets ruleset migrator: applies the schema migrations and writes the rules into the database, then exits.
# docker compose runs it before the API (see compose.api.yaml)
# build from the repository root: docker build -f Seed/Dockerfile -t tabletopsheets-migrator .
# The image contains no rule data: mount the seed folder (dnddata/ of the repository or dnddata.zip from the GitHub
# releases) at /seed, e.g. docker run -v ./dnddata:/seed:ro tabletopsheets-migrator

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

# seed folder with Extracted/, Rules/ and Images/, mounted read-only
VOLUME /seed

ENV Database__Provider=Sqlite \
    ConnectionStrings__TableTopSheets="Data Source=/app/data/tabletopsheets.db" \
    Migrator__DataDirectory=/seed

USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# project files first, so the restore layer is reused until a dependency changes
COPY Common/TableTopSheets.Common.csproj Common/
COPY Engine/Library/TableTopSheets.Engine.csproj Engine/Library/
COPY Persistence/Library/TableTopSheets.Persistence.csproj Persistence/Library/
COPY Persistence/Postgresql/TableTopSheets.Persistence.Postgresql.csproj Persistence/Postgresql/
COPY Persistence/Sqlite/TableTopSheets.Persistence.Sqlite.csproj Persistence/Sqlite/
COPY Seed/TableTopSheets.Seed.csproj Seed/
RUN dotnet restore Seed/TableTopSheets.Seed.csproj

COPY Common/ Common/
COPY Engine/Library/ Engine/Library/
COPY Persistence/ Persistence/
COPY Seed/ Seed/
RUN dotnet publish Seed/TableTopSheets.Seed.csproj \
      --configuration $BUILD_CONFIGURATION \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

FROM base AS final

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TableTopSheets.Seed.dll"]
CMD ["migrate"]
