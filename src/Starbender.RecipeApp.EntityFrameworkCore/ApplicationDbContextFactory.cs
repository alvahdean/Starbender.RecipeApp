using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Starbender.RecipeApp.EntityFrameworkCore;

public sealed class ApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Point to your host project's appsettings.json (adjust paths as needed)
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "Starbender.RecipeApp"));

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' not found.");
        var sqlServerSection = configuration.GetSection("Database:SqlServer");
        var commandTimeoutSeconds = sqlServerSection.GetValue<int?>("CommandTimeoutSeconds") ?? 180;
        var maxRetryCount = sqlServerSection.GetValue<int?>("MaxRetryCount") ?? 10;
        var maxRetryDelaySeconds = sqlServerSection.GetValue<int?>("MaxRetryDelaySeconds") ?? 30;

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.CommandTimeout(commandTimeoutSeconds);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: maxRetryCount,
                    maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelaySeconds),
                    errorNumbersToAdd: null);
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
