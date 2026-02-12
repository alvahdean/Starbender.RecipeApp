using Microsoft.AspNetCore.Components;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class RecipeTable : RecipeComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;

    private List<RecipeDto> _recipes = new();
    private string _searchTerm = string.Empty;

    private IEnumerable<RecipeDto> FilteredRecipes =>
        string.IsNullOrWhiteSpace(_searchTerm)
            ? _recipes
            : _recipes.Where(recipe =>
                (recipe.Title?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (recipe.Description?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        _recipes = (await RecipeService.GetAllAsync()).ToList();

        await base.OnInitializedAsync();
    }
}
