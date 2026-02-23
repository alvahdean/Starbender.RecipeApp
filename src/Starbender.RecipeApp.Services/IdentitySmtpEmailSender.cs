using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.RecipeApp.EntityFrameworkCore;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Services;

public sealed class IdentitySmtpEmailSender(
    IOptions<SmtpEmailSenderOptions> options,
    ILogger<IdentitySmtpEmailSender> logger) : IEmailSender<ApplicationUser>
{
    private readonly SmtpEmailSenderOptions _options = options.Value;

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendHtmlEmailAsync(email, "Confirm your email", $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>.");

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendHtmlEmailAsync(email, "Reset your password", $"Please reset your password by <a href='{resetLink}'>clicking here</a>.");

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendTextEmailAsync(email, "Reset your password", $"Please reset your password using the following code: {resetCode}");

    private async Task SendHtmlEmailAsync(string toEmail, string subject, string htmlBody)
    {
        ValidateConfiguration();

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress!, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(toEmail);

        using var client = CreateClient();
        await client.SendMailAsync(message);

        logger.LogInformation("SMTP email sent to {Email} with subject '{Subject}'", toEmail, subject);
    }

    private async Task SendTextEmailAsync(string toEmail, string subject, string body)
    {
        ValidateConfiguration();

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress!, _options.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(toEmail);

        using var client = CreateClient();
        await client.SendMailAsync(message);

        logger.LogInformation("SMTP email sent to {Email} with subject '{Subject}'", toEmail, subject);
    }

    private SmtpClient CreateClient()
    {
        var client = new SmtpClient(_options.Host!, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            UseDefaultCredentials = _options.UseDefaultCredentials
        };

        if (!_options.UseDefaultCredentials && !string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        return client;
    }

    private void ValidateConfiguration()
    {
        if (!_options.Enabled)
        {
            throw new InvalidOperationException("SMTP email sender is not enabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.Host))
        {
            throw new InvalidOperationException("Email:Smtp:Host is required when SMTP email is enabled.");
        }

        if (string.IsNullOrWhiteSpace(_options.FromAddress))
        {
            throw new InvalidOperationException("Email:Smtp:FromAddress is required when SMTP email is enabled.");
        }
    }
}
