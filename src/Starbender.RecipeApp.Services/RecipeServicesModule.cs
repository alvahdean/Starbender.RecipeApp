using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Starbender.Core;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Domain;
using Starbender.RecipeApp.EntityFrameworkCore;
using Starbender.RecipeApp.Services.Contracts;

namespace Starbender.RecipeApp.Services;

public class RecipeServicesModule : ModuleBase
{
    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // Add dependent modules here...
        services.AddModule<RecipeServiceContractsModule>();
        services.AddModule<RecipeDomainModule>();
        services.AddModule<RecipeEntityFrameworkModule>();

        // Register local services here...
        services.AddTransient<IRecipeAppService, RecipeAppService>();
        services.AddTransient<IIngredientAppService, IngredientAppService>();
        services.AddTransient<IUnitAppService, UnitAppService>();
        services.AddOptions<RecipeAppOptions>()
            .BindConfiguration(RecipeAppOptions.ConfigurationKey)
            .ValidateOnStart();

        ConfigureEmailSender(services);

        return services;
    }

    private void ConfigureEmailSender(IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.AddOptions<SmtpEmailSenderOptions>()
            .BindConfiguration(SmtpEmailSenderOptions.ConfigurationSection);

        if (configuration.GetValue<bool>("Email:Smtp:Enabled"))
        {
            services.AddSingleton<IEmailSender<ApplicationUser>, IdentitySmtpEmailSender>();
        }
        else
        {
            services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
        }
    }
}
