# TableTopSheets REST API, hosting the user interface. It neither migrates nor seeds the database: the ruleset
# migrator (Seed/Dockerfile) does that before the API starts
# build from the repository root: docker build -f Api/Library/Dockerfile -t tabletopsheets-api .

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
COPY Api/Library/TableTopSheets.Api.csproj Api/Library/
COPY Common/TableTopSheets.Common.csproj Common/
COPY Engine/Library/TableTopSheets.Engine.csproj Engine/Library/
COPY Pdf/Library/TableTopSheets.Pdf.csproj Pdf/Library/
COPY Persistence/Library/TableTopSheets.Persistence.csproj Persistence/Library/
COPY Persistence/Postgresql/TableTopSheets.Persistence.Postgresql.csproj Persistence/Postgresql/
COPY Persistence/Sqlite/TableTopSheets.Persistence.Sqlite.csproj Persistence/Sqlite/
RUN dotnet restore Api/Library/TableTopSheets.Api.csproj

COPY Api/Library/ Api/Library/
COPY Common/ Common/
COPY Engine/Library/ Engine/Library/
COPY Pdf/Library/ Pdf/Library/
COPY Persistence/ Persistence/
RUN dotnet publish Api/Library/TableTopSheets.Api.csproj \
      --configuration $BUILD_CONFIGURATION \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

FROM base AS final

COPY --from=build /app/publish .
# served by the API from its web root, same origin as /api
COPY --from=ui /ui/dist ./wwwroot

ENTRYPOINT ["dotnet", "TableTopSheets.Api.dll"]
