using Microsoft.AspNetCore.Components;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class UnitTable : ComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;
    [Inject] IUnitAppService UnitService { get; set; } = default!;

    private List<UnitDto> _units = new();

    protected override async Task OnInitializedAsync()
    {
        await SeedUnitsAsync();
        _units = (await UnitService.GetAllAsync()).ToList();

        await base.OnInitializedAsync();
    }
}
