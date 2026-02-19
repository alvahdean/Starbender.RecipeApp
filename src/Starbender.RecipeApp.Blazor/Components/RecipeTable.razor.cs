using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using MudBlazor;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Blazor.Components;

public partial class RecipeTable : RecipeComponentBase
{
    [Inject] IRecipeAppService RecipeService { get; set; } = default!;
    [Inject] IOptions<RecipeAppOptions> Options { get; set; } = default!;
    [Inject] IDialogService DialogService { get; set; } = default!;

    [Parameter] public bool CanCollapse { get; set; } = false;
    [Parameter] public bool IsEditable { get; set; } = true;

    private List<RecipeDto> _recipes = new();
    private readonly Dictionary<int, int> _imageRefresh = new();
    private string _searchTerm = string.Empty;
    private RecipeAppOptions _options = new();
    private bool _isExpanded = true;

    private void ToggleExpanded() =>
        _isExpanded = !_isExpanded;

    private IEnumerable<RecipeDto> FilteredRecipes =>
        string.IsNullOrWhiteSpace(_searchTerm)
            ? _recipes
            : _recipes.Where(recipe =>
                (recipe.Title?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (recipe.Description?.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        _recipes = (await RecipeService.GetAllAsync()).ToList();
        _options = Options.Value;

        await base.OnInitializedAsync();
    }

    private async Task OpenEditorAsync(RecipeDto? selectedRecipe)
    {
        if (selectedRecipe is null)
        {
            return;
        }

        var parameters = new DialogParameters<RecipeEditorDialog>
        {
            { x => x.Model, selectedRecipe },
            { x => x.OnSaved, EventCallback.Factory.Create<RecipeDto>(this, HandleRecipeUpdatedAsync) }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<RecipeEditorDialog>("Edit Recipe", parameters, options);
        var result = await dialog.Result;

        //if (!(result?.Canceled ?? true))
        //{
        //    StateHasChanged();
        //}
    }

    private async Task HandleRecipeDeleteAsync(RecipeDto recipe)
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
        } catch (Exception ex)
        {
            Snackbar.Add($"Delete failed: {ex.Message}", Severity.Error);
        }
    }

    private Task HandleRecipeUpdatedAsync(RecipeDto updated)
    {
        var index = 0;
        var existing = _recipes.FirstOrDefault(x => x.Id == updated.Id);

        if (existing is not null)
        {
            index = _recipes.IndexOf(existing);
            _recipes.Remove(existing);
        }

        _recipes.Insert(index, updated);
        _imageRefresh[updated.Id] = _imageRefresh.TryGetValue(updated.Id, out var value) ? value + 1 : 1;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private int GetImageRefreshToken(int recipeId) =>
        _imageRefresh.TryGetValue(recipeId, out var value) ? value : 0;
}
