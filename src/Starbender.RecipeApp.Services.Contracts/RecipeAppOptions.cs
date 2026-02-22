using Starbender.BlobStorage.Contracts;

namespace Starbender.RecipeApp.Services.Contracts;

public class RecipeAppOptions
{
    public const string ConfigurationKey = "RecipeApp";
    public BlobStoreType ImageStoreType {  get; set; }
    public string ImageContainerId { get; set; } = string.Empty;
    public bool EnableAuthentication { get; set; }
    public bool EnableSeeding { get; set; }
}
