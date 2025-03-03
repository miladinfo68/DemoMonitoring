FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
HEALTHCHECK CMD curl --fail http://localhost/health || exit 1

# Install curl for debugging
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Demo.WebApp/Demo.WebApp.csproj", "Demo.WebApp/"]
COPY ["Demo.Shared/Demo.Shared.csproj", "Demo.Shared/"]
RUN dotnet restore "Demo.WebApp/Demo.WebApp.csproj"
COPY . .
WORKDIR "/src/Demo.WebApp"
RUN dotnet build "Demo.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Demo.WebApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.WebApp.dll"]