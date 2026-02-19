# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Starbender.RecipeApp/Starbender.RecipeApp.csproj", "Starbender.RecipeApp/"]
COPY ["Starbender.RecipeApp.Blazor/Starbender.RecipeApp.Blazor.csproj", "Starbender.RecipeApp.Blazor/"]
COPY ["Starbender.RecipeApp.Core/Starbender.RecipeApp.Core.csproj", "Starbender.RecipeApp.Core/"]
COPY ["Starbender.BlobStorage/Starbender.BlobStorage.csproj", "Starbender.BlobStorage/"]
COPY ["Starbender.Core/Starbender.Core.csproj", "Starbender.Core/"]
COPY ["Starbender.RecipeApp.Services.Contracts/Starbender.RecipeApp.Services.Contracts.csproj", "Starbender.RecipeApp.Services.Contracts/"]
COPY ["Starbender.RecipeApp.Doamin.Shared/Starbender.RecipeApp.Domain.Shared.csproj", "Starbender.RecipeApp.Doamin.Shared/"]
COPY ["Starbender.RecipeApp.EntityFrameworkCore/Starbender.RecipeApp.EntityFrameworkCore.csproj", "Starbender.RecipeApp.EntityFrameworkCore/"]
COPY ["Starbender.RecipeApp.Domain/Starbender.RecipeApp.Domain.csproj", "Starbender.RecipeApp.Domain/"]
COPY ["Starbender.RecipeApp.Services/Starbender.RecipeApp.Services.csproj", "Starbender.RecipeApp.Services/"]
RUN dotnet restore "./Starbender.RecipeApp/Starbender.RecipeApp.csproj"
COPY . .
WORKDIR "/src/Starbender.RecipeApp"
RUN dotnet build "./Starbender.RecipeApp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Starbender.RecipeApp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Starbender.RecipeApp.dll"]