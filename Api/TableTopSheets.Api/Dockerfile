# TableTopSheets REST API
# build from the repository root: docker build -f Api/TableTopSheets.Api/Dockerfile -t tabletopsheets-api .

ARG DOTNET_VERSION=10.0

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
COPY Seed/TableTopSheets.Seed/TableTopSheets.Seed.csproj Seed/TableTopSheets.Seed/
RUN dotnet restore Api/TableTopSheets.Api/TableTopSheets.Api.csproj

COPY Api/ Api/
COPY Common/ Common/
COPY Engine/ Engine/
COPY Pdf/Library/ Pdf/Library/
COPY Persistence/ Persistence/
COPY Seed/ Seed/
RUN dotnet publish Api/TableTopSheets.Api/TableTopSheets.Api.csproj \
      --configuration $BUILD_CONFIGURATION \
      --no-restore \
      --output /app/publish \
      /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
WORKDIR /app

# Npgsql loads the GSSAPI library when it connects (logs an error without it)
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

# SQLite database lives here; mount a volume to keep it
RUN mkdir -p /app/data && chown $APP_UID /app/data
VOLUME /app/data

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080 \
    Database__Provider=Sqlite \
    ConnectionStrings__TableTopSheets="Data Source=/app/data/tabletopsheets.db"

USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "TableTopSheets.Api.dll"]
