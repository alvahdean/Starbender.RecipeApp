using AutoMapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Text;

namespace Starbender.RecipeApp.Blazor.Components;

public class RecipeComponentBase : MudComponentBase
{
    [Inject] protected ISnackbar Snackbar { get; set; } = null!;
    [Inject] protected IMapper Mapper { get; set; } = null!;
}
