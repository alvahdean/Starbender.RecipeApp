using Microsoft.AspNetCore.Components;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class RecipeTable : ComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;
    private List<RecipeDto> _recipes = new();
    protected override async Task OnInitializedAsync()
    {
        _recipes = (await RecipeService.GetAllAsync()).ToList();

        await base.OnInitializedAsync();
    }
}
