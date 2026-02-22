using Starbender.Core;
using Starbender.RecipeApp.Services.Contracts.Authorization;

namespace Starbender.RecipeApp.Security;

internal sealed class CurrentUserPermissionService(ICurrentUserAccessor currentUserAccessor) : IPermissionService
{
    public async Task<bool> HasPermissionAsync(string permission, CancellationToken ct = default)
    {
        var user = await currentUserAccessor.GetCurrentUserAsync(ct);
        return RecipeAuthorizationEvaluator.HasPermission(user.Principal, permission);
    }
}
