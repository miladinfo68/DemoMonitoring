FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
HEALTHCHECK CMD curl --fail http://localhost/health || exit 1

# Install curl for debugging
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Demo.WebApi/Demo.WebApi.csproj", "Demo.WebApi/"]
COPY ["Demo.Shared/Demo.Shared.csproj", "Demo.Shared/"]
RUN dotnet restore "Demo.WebApi/Demo.WebApi.csproj"
COPY . .
WORKDIR "/src/Demo.WebApi"
RUN dotnet build "Demo.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Demo.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Demo.WebApi.dll"]