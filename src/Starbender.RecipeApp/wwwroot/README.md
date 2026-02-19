# Starbender.RecipeApp
## Overview
This application is a simple code sample to demonstrate my personal coding style and experience with .NET/Blazor.
THe code repositiry is publicly available and can be found on [GitHub](https://github.com/alvahdean/Starbender.RecipeApp).
___

## Requirements
Create a new recipe application using Blazor (Web Assembly or Server). 
Application should use SQL Server. 
Ideally this should be under ~3 hours of work (but polish is appreciated).

1. Allow users to create a recipe
    1. Title
    2. Description
    3. Image
    4. Ingredients (multiple)
    5. Instructions
2. Recipes should be persisted to the database
3. List of recipes
   1. Viewing the list of recipes should be done with ASP.NET Core via Blazor components.
Scaffolding should not be used.
   2. The recipes list should be filterable.
4. Individual Recipe
   1. Viewing an individual recipe should be done via a Razor Page.
5. Site should use Bootstrap or some other type of front-end framework.
6. Queries should use Entity Framework
___

# Database migration
There is a script in src/migrate-database.cmd that will correctly run the database update. It will use the values in appsettings.json for the __Default__ connection string.
___

## Solution Layout
The solution is split into focused projects under `src/`:

- `Starbender.RecipeApp`: ASP.NET Core host application (startup, Identity, Razor components endpoint mapping).
- `Starbender.RecipeApp.Blazor`: UI components (MudBlazor-based recipe editor/table dialogs and reusable UI pieces).
- `Starbender.RecipeApp.Services.Contracts`: application service interfaces and DTO contracts shared across layers.
- `Starbender.RecipeApp.Services`: application service implementations (CRUD orchestration and business workflow).
- `Starbender.RecipeApp.EntityFrameworkCore`: EF Core `DbContext`, migrations, and repository implementations.
- `Starbender.RecipeApp.Domain`: domain module composition.
- `Starbender.RecipeApp.Doamin.Shared`: shared domain entities (`Recipe`, `Ingredient`, `Unit`, `RecipeIngredient`).
- `Starbender.RecipeApp.Core`: app-specific module wiring and cross-cutting registrations.
- `Starbender.Core`: generic abstractions and infrastructure primitives (`IRepository`, DTO/entity interfaces, module base).
- `Starbender.BlobStorage`: blob/file storage abstractions and implementations used for recipe images.

___

## Application Architecture
The application uses a layered architecture with dependency inversion and module-based registration:

1. UI Layer (`Starbender.RecipeApp.Blazor`)
- Razor/MudBlazor components handle interaction, validation, and dialogs.
- Components call application services via interfaces from `Services.Contracts`.

2. Application Layer (`Starbender.RecipeApp.Services`)
- Implements use cases such as creating/updating recipes and managing images.
- Maps entities <-> DTOs with AutoMapper.
- Coordinates persistence through `IRepository<T>` abstractions.

3. Data Layer (`Starbender.RecipeApp.EntityFrameworkCore`)
- `ApplicationDbContext` defines EF Core model and relationships.
- Repositories execute data access (including eager-loading for recipe ingredient graphs).
- SQL Server persistence is configured in the host (`Program.cs`).

4. Domain/Shared Model (`Starbender.RecipeApp.Doamin.Shared`, `Starbender.RecipeApp.Domain`)
- Holds core entity definitions and domain module dependencies.

5. Cross-cutting Modules (`Starbender.Core`, `Starbender.RecipeApp.Core`, `Starbender.BlobStorage`)
- Provide module bootstrapping, common abstractions, and image/blob storage infrastructure.

Runtime request flow is:
`Blazor Component -> App Service -> Repository -> EF Core DbContext -> SQL Server`
and for image operations:
`App Service -> Blob Container abstraction -> configured blob store`.
___
