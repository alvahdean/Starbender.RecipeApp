# --- build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Build from repo-root context so project references under src/ are available
COPY src/ ./src/

# Restore and publish the web app project
RUN dotnet restore src/Starbender.RecipeApp/Starbender.RecipeApp.csproj
RUN dotnet publish src/Starbender.RecipeApp/Starbender.RecipeApp.csproj -c Release -o /app/publish --no-restore

# --- run ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Container Apps ingress will hit this port
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=DockerDev
ENV DOTNET_ENVIRONMENT=DockerDev

EXPOSE 8080

COPY --from=build /app/publish ./
ENTRYPOINT ["dotnet", "Starbender.RecipeApp.dll"]
