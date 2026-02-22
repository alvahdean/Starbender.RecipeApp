using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Starbender.RecipeApp.EntityFrameworkCore;
using Starbender.RecipeApp.Services.Contracts.Authorization;

namespace Starbender.RecipeApp.Security;

internal sealed class BootstrapUserHostedService(
    IServiceProvider serviceProvider,
    IOptions<BootstrapUserOptions> options,
    ILogger<BootstrapUserHostedService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (!settings.Enabled)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        var user = await FindUserAsync(userManager, settings);
        if (user is null)
        {
            if (!settings.AutoCreate)
            {
                logger.LogWarning("Bootstrap user not found and AutoCreate is disabled. Email='{Email}', UserName='{UserName}'", settings.Email, settings.UserName);
                return;
            }

            if (string.IsNullOrWhiteSpace(settings.Password))
            {
                logger.LogWarning("Bootstrap user AutoCreate is enabled but no password was configured.");
                return;
            }

            user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(settings.UserName) ? settings.Email : settings.UserName,
                Email = settings.Email,
                EmailConfirmed = settings.ConfirmEmail
            };

            var createResult = await userManager.CreateAsync(user, settings.Password);
            if (!createResult.Succeeded)
            {
                logger.LogError("Failed to create bootstrap user: {Errors}", string.Join("; ", createResult.Errors.Select(e => e.Description)));
                return;
            }

            logger.LogInformation("Created bootstrap user '{UserName}' ({Email})", user.UserName, user.Email);
        }
        else if (settings.ConfirmEmail && !user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            var updateUserResult = await userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
            {
                logger.LogWarning("Failed to confirm bootstrap user's email: {Errors}", string.Join("; ", updateUserResult.Errors.Select(e => e.Description)));
            }
        }

        await ApplyPermissionClaimsAsync(userManager, user, settings);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<ApplicationUser?> FindUserAsync(UserManager<ApplicationUser> userManager, BootstrapUserOptions settings)
    {
        ApplicationUser? user = null;

        if (!string.IsNullOrWhiteSpace(settings.Email))
        {
            user = await userManager.FindByEmailAsync(settings.Email);
        }

        if (user is null && !string.IsNullOrWhiteSpace(settings.UserName))
        {
            user = await userManager.FindByNameAsync(settings.UserName);
        }

        return user;
    }

    private async Task ApplyPermissionClaimsAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, BootstrapUserOptions settings)
    {
        var configuredPermissions = settings.Permissions
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p.Trim())
            .Where(RecipeAppPermissions.All.Contains)
            .Distinct(StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        var claims = await userManager.GetClaimsAsync(user);
        var existingPermissionClaims = claims
            .Where(c => string.Equals(c.Type, RecipeAppClaimTypes.Permission, StringComparison.OrdinalIgnoreCase))
            .Where(c => RecipeAppPermissions.All.Contains(c.Value, StringComparer.Ordinal))
            .ToList();

        var existingPermissions = existingPermissionClaims
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

        var permissionsToAdd = configuredPermissions.Except(existingPermissions, StringComparer.Ordinal)
            .Select(p => new Claim(RecipeAppClaimTypes.Permission, p))
            .ToList();

        if (permissionsToAdd.Count > 0)
        {
            var addResult = await userManager.AddClaimsAsync(user, permissionsToAdd);
            if (!addResult.Succeeded)
            {
                logger.LogError("Failed adding bootstrap permission claims: {Errors}", string.Join("; ", addResult.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (settings.ReplaceManagedPermissionClaims)
        {
            var claimsToRemove = existingPermissionClaims
                .Where(c => !configuredPermissions.Contains(c.Value))
                .Cast<Claim>()
                .ToList();

            if (claimsToRemove.Count > 0)
            {
                var removeResult = await userManager.RemoveClaimsAsync(user, claimsToRemove);
                if (!removeResult.Succeeded)
                {
                    logger.LogError("Failed removing bootstrap permission claims: {Errors}", string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                    return;
                }
            }
        }

        logger.LogInformation(
            "Bootstrap permissions applied to '{UserName}'. Explicit permissions: {Permissions}",
            user.UserName,
            configuredPermissions.Count == 0 ? "(none)" : string.Join(", ", configuredPermissions.OrderBy(p => p)));
    }
}
