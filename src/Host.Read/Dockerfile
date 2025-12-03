FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/Host.Read/Host.Read.csproj", "src/Host.Read/"]
COPY ["src/Core/Core.csproj", "src/Core/"]
COPY ["src/Adapters/Adapter.Api/Adapter.Api.csproj", "src/Adapters/Adapter.Api/"]
COPY ["src/Adapters/Adapter.PostgreSQL/Adapter.PostgreSQL.csproj", "src/Adapters/Adapter.PostgreSQL/"]
COPY ["src/Adapters/Adapter.Redis/Adapter.Redis.csproj", "src/Adapters/Adapter.Redis/"]

# Restore dependencies
RUN dotnet restore "src/Host.Read/Host.Read.csproj"

# Copy everything else
COPY . .

# Build
WORKDIR "/src/src/Host.Read"
RUN dotnet build "Host.Read.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Host.Read.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.Read.dll"]
