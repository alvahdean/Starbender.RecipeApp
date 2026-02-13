using Microsoft.AspNetCore.Components;
using MudBlazor;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class RecipeTable : RecipeComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;
    [Inject] IDialogService DialogService { get; set; } = default!;

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

    private async Task HandleRowClick(TableRowClickEventArgs<RecipeDto> args)
    {
        var selectedRecipe = args.Item;

        if (selectedRecipe is null)
        {
            return;
        }

        var parameters = new DialogParameters<RecipeEditorDialog>
        {
            { x => x.Model, selectedRecipe },
            { x => x.OnSaved, EventCallback.Factory.Create<RecipeDto>(this, HandleDialogSavedAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<RecipeEditorDialog>("Edit Recipe", parameters, options);
        await dialog.Result;
    }

    private async Task HandleDialogSavedAsync(RecipeDto _)
    {
        _recipes = (await RecipeService.GetAllAsync()).ToList();
        StateHasChanged();
    }

    private async Task DeleteRecipeAsync(RecipeDto recipe)
    {
        var confirmed = await DialogService.ShowMessageBox(
            "Delete Recipe",
            $"Delete '{recipe.Title}'?",
            yesText: "Delete",
            cancelText: "Cancel");

        if (confirmed != true)
        {
            return;
        }

        try
        {
            await RecipeService.DeleteAsync(recipe.Id);
            _recipes.RemoveAll(x => x.Id == recipe.Id);
            Snackbar.Add("Recipe deleted.", Severity.Success);
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
        }
    }
}
