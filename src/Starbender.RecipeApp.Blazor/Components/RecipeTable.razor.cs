using Microsoft.AspNetCore.Components;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class RecipeTable : ComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;
    [Inject] IUnitAppService UnitService { get; set; } = default!;
    private List<RecipeDto> _recipes = new();
    private List<UnitDto> _units = new();
    protected override async Task OnInitializedAsync()
    {
        _recipes = (await RecipeService.GetAllAsync()).ToList();
        _units = (await UnitService.GetAllAsync()).ToList();

        await SeedRecipesAsync();
        await base.OnInitializedAsync();
    }


}
