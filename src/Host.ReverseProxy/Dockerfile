# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file and all project files
COPY ["ClubExample.sln", "./"]
COPY ["src/Host.ReverseProxy/Host.ReverseProxy.csproj", "src/Host.ReverseProxy/"]

# Restore dependencies
RUN dotnet restore "src/Host.ReverseProxy/Host.ReverseProxy.csproj"

# Copy all source code
COPY . .

# Build the project
WORKDIR "/src/src/Host.ReverseProxy"
RUN dotnet build "Host.ReverseProxy.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "Host.ReverseProxy.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Host.ReverseProxy.dll"]
