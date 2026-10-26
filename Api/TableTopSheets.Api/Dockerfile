# TableTopSheets REST API, hosting the user interface. It neither migrates nor seeds the database: the ruleset
# migrator (Seed/TableTopSheets.Seed/Dockerfile) does that before the API starts
# build from the repository root: docker build -f Api/TableTopSheets.Api/Dockerfile -t tabletopsheets-api .

ARG DOTNET_VERSION=10.0
ARG NODE_VERSION=24

# runtime image; first stage, so IDE fast-mode debugging (which builds only the first stage) gets a dotnet runtime
FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS base
WORKDIR /app

# Npgsql loads the GSSAPI library when it connects (logs an error without it)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# SQLite database lives here; mount a volume to keep it
RUN mkdir -p /app/data && chown $APP_UID /app/data
VOLUME /app/data

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Database__Provider=Sqlite \
    ConnectionStrings__TableTopSheets="Data Source=/app/data/tabletopsheets.db"

USER $APP_UID
EXPOSE 8080

FROM node:${NODE_VERSION}-slim AS ui
WORKDIR /ui

# manifests first, so the install layer is reused until a dependency changes
COPY Ui/package.json Ui/package-lock.json ./
RUN npm ci

COPY Ui/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# project files first, so the restore layer is reused until a dependency changes
COPY Api/TableTopSheets.Api/TableTopSheets.Api.csproj Api/TableTopSheets.Api/
COPY Common/TableTopSheets.Common/TableTopSheets.Common.csproj Common/TableTopSheets.Common/
COPY Engine/TableTopSheets.Engine/TableTopSheets.Engine.csproj Engine/TableTopSheets.Engine/
COPY Pdf/Library/TableTopSheets.Pdf.csproj Pdf/Library/
COPY Persistence/TableTopSheets.Persistence/TableTopSheets.Persistence.csproj Persistence/TableTopSheets.Persistence/
COPY Persistence/TableTopSheets.Persistence.Postgresql/TableTopSheets.Persistence.Postgresql.csproj Persistence/TableTopSheets.Persistence.Postgresql/
COPY Persistence/TableTopSheets.Persistence.Sqlite/TableTopSheets.Persistence.Sqlite.csproj Persistence/TableTopSheets.Persistence.Sqlite/
RUN dotnet restore Api/TableTopSheets.Api/TableTopSheets.Api.csproj

COPY Api/ Api/
COPY Common/ Common/
COPY Engine/ Engine/
COPY Pdf/Library/ Pdf/Library/
COPY Persistence/ Persistence/
RUN dotnet publish Api/TableTopSheets.Api/TableTopSheets.Api.csproj \
      --configuration $BUILD_CONFIGURATION \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

FROM base AS final

COPY --from=build /app/publish .
# served by the API from its web root, same origin as /api
COPY --from=ui /ui/dist ./wwwroot

ENTRYPOINT ["dotnet", "TableTopSheets.Api.dll"]
