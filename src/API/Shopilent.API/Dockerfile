FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /build
COPY ["Directory.Build.props", "./"]
COPY ["src/API/Shopilent.API/Shopilent.API.csproj", "API/Shopilent.API/"]
COPY ["src/Core/Shopilent.Application/Shopilent.Application.csproj", "Core/Shopilent.Application/"]
COPY ["src/Core/Shopilent.Domain/Shopilent.Domain.csproj", "Core/Shopilent.Domain/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure/Shopilent.Infrastructure.csproj", "Infrastructure/Shopilent.Infrastructure/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Cache.Redis/Shopilent.Infrastructure.Cache.Redis.csproj", "Infrastructure/Shopilent.Infrastructure.Cache.Redis/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Identity/Shopilent.Infrastructure.Identity.csproj", "Infrastructure/Shopilent.Infrastructure.Identity/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Logging/Shopilent.Infrastructure.Logging.csproj", "Infrastructure/Shopilent.Infrastructure.Logging/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Payments/Shopilent.Infrastructure.Payments.csproj", "Infrastructure/Shopilent.Infrastructure.Payments/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Persistence.PostgreSQL/Shopilent.Infrastructure.Persistence.PostgreSQL.csproj", "Infrastructure/Shopilent.Infrastructure.Persistence.PostgreSQL/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Realtime.SignalR/Shopilent.Infrastructure.Realtime.SignalR.csproj", "Infrastructure/Shopilent.Infrastructure.Realtime.SignalR/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.S3ObjectStorage/Shopilent.Infrastructure.S3ObjectStorage.csproj", "Infrastructure/Shopilent.Infrastructure.S3ObjectStorage/"]
COPY ["src/Infrastructure/Shopilent.Infrastructure.Search.Meilisearch/Shopilent.Infrastructure.Search.Meilisearch.csproj", "Infrastructure/Shopilent.Infrastructure.Search.Meilisearch/"]
RUN dotnet restore "API/Shopilent.API/Shopilent.API.csproj"
COPY src/ .
WORKDIR "/build/API/Shopilent.API"
RUN dotnet build "Shopilent.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Shopilent.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Shopilent.API.dll"]
