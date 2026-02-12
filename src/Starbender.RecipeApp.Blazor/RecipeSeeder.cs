using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor;

public class RecipeSeeder
{
    private readonly IRecipeAppService _recipeService;
    private IUnitAppService _unitService;
    private IIngredientAppService _ingredientService;
    private readonly Random _random = new Random();

    public RecipeSeeder(
        IRecipeAppService recipeService,
        IUnitAppService unitService,
        IIngredientAppService ingredientService)
    {
        _recipeService = recipeService;
        _unitService = unitService;
        _ingredientService = ingredientService;
    }

    public async Task SeedAll()
    {
        await SeedUnitsAsync();
        await SeedIngredientsAsync();
        await SeedRecipesAsync();
    }

    private async Task SeedUnitsAsync()
    {
        var unitNames = new string[] { "tsp", "tbsp", "ounce", "pound", "cup" };
        var existing = await _unitService.GetAllAsync();

        foreach (var unitName in unitNames)
        {
            if (!existing.Any(t => t.Name.Equals(unitName, StringComparison.OrdinalIgnoreCase)))
            {
                await _unitService.CreateAsync(new UnitDto() { Name = unitName });
            }
        }
    }

    private async Task SeedRecipesAsync()
    {
        var existing = await _recipeService.GetAllAsync();
        var units = await _unitService.GetAllAsync();
        var ingredients = await _ingredientService.GetAllAsync();
        var unitCount = units.Count();
        var ingredientCount = ingredients.Count();

        for (int i = 1; i <= 5; i++)
        {
            var title = $"Recipe {i}";

            if (!existing.Any(t => t.Title.Equals(title, StringComparison.OrdinalIgnoreCase)))
            {
                var newRecipe = await _recipeService.CreateAsync(new RecipeDto()
                {
                    Title = title,
                    Description = $"{title} description."
                });

                for (int j = 1; j <= 5; j++)
                {
                    var ingredient = ingredients[_random.Next(0, ingredientCount)];
                    var unit = units[_random.Next(0, unitCount)];
                    var qty = _random.Next(1, 5);
                    newRecipe.RecipeIngredients.Add(new RecipeIngredientDto()
                    {
                        RecipeId = newRecipe.Id,
                        IngredientId = ingredient.Id,
                        UnitId = unit.Id,
                        Quantity = qty,
                    });
                }

                await _recipeService.UpdateAsync(newRecipe);
            }
        }
    }

    private async Task SeedIngredientsAsync()
    {
        var existing = await _ingredientService.GetAllAsync();
        for (int i = 1; i <= 100; i++)
        {
            var name = $"Ingredient {i}";
            if (!existing.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {

                var newRecipe = await _ingredientService.CreateAsync(new IngredientDto()
                {
                    Name = name
                });
            }
        }
    }
}
