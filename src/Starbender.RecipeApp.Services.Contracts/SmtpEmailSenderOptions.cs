namespace Starbender.RecipeApp.Services.Contracts;

public sealed class SmtpEmailSenderOptions
{
    public const string ConfigurationSection = "Email:Smtp";

    public bool Enabled { get; set; }

    public string? Host { get; set; }

    public int Port { get; set; } = 25;

    public bool UseSsl { get; set; }

    public bool UseDefaultCredentials { get; set; }

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public string? FromAddress { get; set; }

    public string? FromName { get; set; }
}
