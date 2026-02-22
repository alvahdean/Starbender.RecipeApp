using System.Security.Claims;

namespace Starbender.RecipeApp.Services.Contracts.Authorization;

public static class RecipeAppClaimTypes
{
    public const string Permission = "permission";
}

public static class RecipeAppPermissions
{
    public const string PublishOwnedRecipe = nameof(PublishOwnedRecipe);
    public const string UnpublishOwnedRecipe = nameof(UnpublishOwnedRecipe);
    public const string ManageOwnedRecipe = nameof(ManageOwnedRecipe);
    public const string ViewAnyRecipe = nameof(ViewAnyRecipe);
    public const string ModifyAnyRecipe = nameof(ModifyAnyRecipe);
    public const string ManageSecurity = nameof(ManageSecurity);

    public static readonly string[] DefaultAuthenticatedPermissions =
    [
        UnpublishOwnedRecipe,
        ManageOwnedRecipe
    ];

    public static readonly string[] All =
    [
        PublishOwnedRecipe,
        UnpublishOwnedRecipe,
        ManageOwnedRecipe,
        ViewAnyRecipe,
        ModifyAnyRecipe,
        ManageSecurity
    ];
}

public static class RecipeAppPolicies
{
    public const string PublishOwnedRecipe = "Permission:PublishOwnedRecipe";
    public const string UnpublishOwnedRecipe = "Permission:UnpublishOwnedRecipe";
    public const string ManageOwnedRecipe = "Permission:ManageOwnedRecipe";
    public const string ViewAnyRecipe = "Permission:ViewAnyRecipe";
    public const string ModifyAnyRecipe = "Permission:ModifyAnyRecipe";
    public const string ManageSecurity = "Permission:ManageSecurity";

    public static string Permission(string permission) => $"Permission:{permission}";
}

public static class RecipeAuthorizationEvaluator
{
    public static bool HasPermission(ClaimsPrincipal? user, string permission)
    {
        if (user?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (RecipeAppPermissions.DefaultAuthenticatedPermissions.Contains(permission, StringComparer.Ordinal))
        {
            return true;
        }

        return user.Claims.Any(claim =>
            string.Equals(claim.Type, RecipeAppClaimTypes.Permission, StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, permission, StringComparison.Ordinal));
    }

    public static string? GetUserId(ClaimsPrincipal? user)
        => user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? user?.FindFirst("sub")?.Value;

    public static bool IsImplicitDefaultPermission(string permission)
        => RecipeAppPermissions.DefaultAuthenticatedPermissions.Contains(permission, StringComparer.Ordinal);
}
