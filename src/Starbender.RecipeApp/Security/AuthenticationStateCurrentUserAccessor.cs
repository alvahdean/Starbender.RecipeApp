using Microsoft.AspNetCore.Components.Authorization;
using Starbender.Core;
using Starbender.RecipeApp.Services.Contracts.Authorization;

namespace Starbender.RecipeApp.Security;

internal sealed class AuthenticationStateCurrentUserAccessor(AuthenticationStateProvider authenticationStateProvider) : ICurrentUserAccessor
{
    public async Task<CurrentUserInfo> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var authState = await authenticationStateProvider.GetAuthenticationStateAsync();
        var principal = authState.User;

        return new CurrentUserInfo(
            principal,
            principal.Identity?.IsAuthenticated == true,
            RecipeAuthorizationEvaluator.GetUserId(principal));
    }
}
