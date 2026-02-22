namespace Starbender.RecipeApp.Security;

internal sealed class BootstrapUserOptions
{
    public const string ConfigurationSection = "Authorization:BootstrapUser";

    public bool Enabled { get; set; }

    public string? Email { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool AutoCreate { get; set; }

    public bool ConfirmEmail { get; set; } = true;

    public bool ReplaceManagedPermissionClaims { get; set; }

    public List<string> Permissions { get; set; } = [];
}
