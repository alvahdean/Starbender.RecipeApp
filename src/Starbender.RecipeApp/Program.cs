using Azure.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Starbender.Core.Extensions;
using Starbender.RecipeApp.Components;
using Starbender.RecipeApp.Components.Account;
using Starbender.RecipeApp.EntityFrameworkCore;

namespace Starbender.RecipeApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var keyVaultSection = builder.Configuration.GetSection("KeyVault");
            var useKeyVault = keyVaultSection.GetValue("Enabled", false);
            if (useKeyVault)
            {
                var vaultUri = keyVaultSection["VaultUri"];
                if (string.IsNullOrWhiteSpace(vaultUri))
                {
                    throw new InvalidOperationException("KeyVault:VaultUri is required when KeyVault is enabled.");
                }

                builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
            }

            builder.Services.AddHttpClient();

            var dataProtectionBuilder = builder.Services.AddDataProtection()
                .SetApplicationName("Starbender.RecipeApp");

            var dataProtectionKeysPath = builder.Configuration["DataProtection:PersistKeysToFileSystem:Path"];
            
            if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
            {
                Directory.CreateDirectory(dataProtectionKeysPath);
                dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
            }

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            builder.Services.AddCascadingAuthenticationState();
            builder.Services.AddScoped<IdentityRedirectManager>();
            builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

            var authenticationBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            });

            authenticationBuilder.AddIdentityCookies();

            var entraIdSection = builder.Configuration.GetSection("Authentication:EntraId");
            if (entraIdSection.GetValue<bool>("Enabled"))
            {
                var clientId = entraIdSection["ClientId"];
                var clientSecret = entraIdSection["ClientSecret"];
                var authority = entraIdSection["Authority"];
                var tenantId = entraIdSection["TenantId"];

                if (string.IsNullOrWhiteSpace(clientId))
                {
                    throw new InvalidOperationException("Authentication:EntraId:ClientId is required when Entra ID authentication is enabled.");
                }

                if (string.IsNullOrWhiteSpace(clientSecret))
                {
                    throw new InvalidOperationException("Authentication:EntraId:ClientSecret is required when Entra ID authentication is enabled.");
                }

                if (string.IsNullOrWhiteSpace(authority))
                {
                    if (string.IsNullOrWhiteSpace(tenantId))
                    {
                        throw new InvalidOperationException("Authentication:EntraId:TenantId is required when Authority is not provided.");
                    }

                    authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                }

                authenticationBuilder.AddOpenIdConnect("EntraId", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.Authority = authority;
                    options.ClientId = clientId;
                    options.ClientSecret = clientSecret;
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.UsePkce = true;
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.CallbackPath = entraIdSection["CallbackPath"] ?? "/signin-oidc";
                    options.SignedOutCallbackPath = entraIdSection["SignedOutCallbackPath"] ?? "/signout-callback-oidc";
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                });
            }

            var connectionString = builder.Configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Connection string 'Default' not found.");
            
            var sqlServerSection = builder.Configuration.GetSection("Database:SqlServer");
            
            var commandTimeoutSeconds = sqlServerSection.GetValue<int?>("CommandTimeoutSeconds") ?? 180;
            var maxRetryCount = sqlServerSection.GetValue<int?>("MaxRetryCount") ?? 10;
            var maxRetryDelaySeconds = sqlServerSection.GetValue<int?>("MaxRetryDelaySeconds") ?? 30;

            builder.Services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(commandTimeoutSeconds);
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: maxRetryCount,
                        maxRetryDelay: TimeSpan.FromSeconds(maxRetryDelaySeconds),
                        errorNumbersToAdd: null);
                }),
                contextLifetime: ServiceLifetime.Transient,
                optionsLifetime: ServiceLifetime.Scoped);

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentityCore<ApplicationUser>(options =>
                {
                    options.SignIn.RequireConfirmedAccount = true;
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders();

            builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

            // Custom module dependency loader
            builder.Services.InitializeAppModules();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

            var httpsPort = app.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT")
                ?? app.Configuration.GetValue<int?>("HTTPS_PORT");
            if (httpsPort.HasValue)
            {
                app.UseHttpsRedirection();
            }

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            // Add additional endpoints required by the Identity /Account Razor components.
            app.MapAdditionalIdentityEndpoints();

            app.Run();
        }
    }
}
