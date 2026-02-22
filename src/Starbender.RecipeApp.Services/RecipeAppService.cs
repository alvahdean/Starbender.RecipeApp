using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Starbender.BlobStorage.Contracts;
using Starbender.Core;
using Starbender.RecipeApp.Domain.Shared.Entities;
using Starbender.RecipeApp.Services.Contracts;
using Starbender.RecipeApp.Services.Contracts.Authorization;
using Starbender.RecipeApp.Services.Contracts.Dtos;

namespace Starbender.RecipeApp.Services;

public class RecipeAppService : CrudAppService<Recipe, RecipeDto>, IRecipeAppService
{
    public const string ContentType = "image/png";

    private readonly RecipeAppOptions _options;
    private readonly IBlobContainer _container;
    private readonly IPermissionService _permissionService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public RecipeAppService(
        IMapper mapper,
        IOptions<RecipeAppOptions> options,
        IBlobContainerFactory containerFactory,
        IPermissionService permissionService,
        ICurrentUserAccessor currentUserAccessor,
        ILogger<RecipeAppService> logger,
        IRepository<Recipe> repo) : base(mapper, logger, repo)
    {
        _options = options.Value;
        _permissionService = permissionService;
        _currentUserAccessor = currentUserAccessor;
        _container = containerFactory.GetContainer(_options.ImageContainerId)
            ?? throw new Exception($"Blob container '{_options.ImageStoreType}.{_options.ImageContainerId}' not found");
    }

    public override async Task<RecipeDto?> GetAsync(int id, CancellationToken ct = default)
    {
        var entity = await Repo.GetAsync(id, ct);
        if (entity is null)
        {
            return null;
        }

        var access = await GetAccessAsync(ct);
        if (!CanView(entity, access))
        {
            return null;
        }

        return Mapper.Map<RecipeDto>(entity);
    }

    public override async Task<IReadOnlyList<RecipeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var access = await GetAccessAsync(ct);
        var entities = await Repo.GetAllAsync(ct);
        var visible = entities.Where(recipe => CanView(recipe, access));

        return Mapper.Map<IEnumerable<RecipeDto>>(visible).ToList();
    }

    public override async Task<RecipeDto> CreateAsync(RecipeDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var access = await GetAccessAsync(ct);
        if (!access.IsAuthenticated || string.IsNullOrWhiteSpace(access.UserId))
        {
            throw new UnauthorizedAccessException("Authentication is required to create recipes.");
        }

        if (!access.CanManageOwnedRecipe)
        {
            throw new UnauthorizedAccessException("You do not have permission to create recipes.");
        }

        dto.UserId = access.UserId;
        if (dto.IsPublic && !access.CanPublishOwnedRecipe)
        {
            dto.IsPublic = false;
        }

        return await base.CreateAsync(dto, ct);
    }

    public override async Task<RecipeDto> UpdateAsync(RecipeDto dto, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Id <= 0)
        {
            return await CreateAsync(dto, ct);
        }

        var existing = await Repo.GetAsync(dto.Id, ct);
        if (existing is null)
        {
            return await CreateAsync(dto, ct);
        }

        var access = await GetAccessAsync(ct);
        EnsureCanModify(existing, access);
        EnsureVisibilityTransitionAllowed(existing, dto, access);

        dto.UserId = existing.UserId;
        Mapper.Map(dto, existing);

        var updated = await Repo.UpdateAsync(existing, ct);
        return Mapper.Map<RecipeDto>(updated);
    }

    public override async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await Repo.GetAsync(id, ct);
        if (existing is null)
        {
            return;
        }

        var access = await GetAccessAsync(ct);
        EnsureCanModify(existing, access);

        await Repo.DeleteAsync(id, ct);
    }

    public virtual async Task<FullRecipeDto?> GetFullAsync(int recipeId, CancellationToken ct = default)
    {
        var thinDto = await GetAsync(recipeId, ct);
        var result = Mapper.Map<FullRecipeDto>(thinDto);

        if (result != null && !string.IsNullOrWhiteSpace(result.ImageBlobId))
        {
            result.Image = await GetRecipeImageAsync(recipeId, ct) ?? new();
        }

        return result;
    }

    public virtual async Task<FullRecipeDto> UpdateFullAsync(FullRecipeDto updated, CancellationToken ct = default)
    {
        var thinDto = await UpdateAsync(updated, ct);
        var result = Mapper.Map<FullRecipeDto>(thinDto);
        var recipeId = result.Id;

        var byteData = updated.Image.Content;
        if (byteData.Length > 0)
        {
            result.Image = await SetRecipeImageAsync(recipeId, byteData, ct);
        }

        return result;
    }

    public async Task<BlobContentDto> SetRecipeImageAsync(int recipeId, byte[] imageBytes, CancellationToken ct = default)
    {
        var recipe = await Repo.GetAsync(recipeId, ct)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} was not found.");

        var access = await GetAccessAsync(ct);
        EnsureCanModify(recipe, access);

        var blobId = recipe.ImageBlobId;
        BlobContentDto blobInfo;

        if (string.IsNullOrWhiteSpace(blobId))
        {
            blobInfo = await _container.CreateContentAsync(imageBytes, ContentType, ct);

            if (recipe.ImageBlobId != blobInfo.BlobId)
            {
                recipe.ImageBlobId = blobInfo.BlobId;
                await Repo.UpdateAsync(recipe, ct);
            }
        }
        else
        {
            blobInfo = await _container.UpdateContentAsync(blobId, imageBytes, ContentType, ct);

            if (blobInfo.BlobId != blobId)
            {
                recipe.ImageBlobId = blobInfo.BlobId;
                await Repo.UpdateAsync(recipe, ct);
            }
        }

        return blobInfo;
    }

    public async Task<BlobContentDto?> GetRecipeImageAsync(int recipeId, CancellationToken ct = default)
    {
        var recipe = await GetAsync(recipeId, ct);
        if (recipe == null)
        {
            return null;
        }

        var blobId = recipe.ImageBlobId;
        BlobContentDto? result = null;

        if (!string.IsNullOrWhiteSpace(blobId))
        {
            try
            {
                result = await _container.GetContentAsync(blobId, ct);
            }
            catch
            {
                Logger.LogWarning("ImageBlob not found in container, returning null");
            }
        }

        return result;
    }

    public async Task PublishAsync(int recipeId, CancellationToken ct = default)
    {
        var recipe = await Repo.GetAsync(recipeId, ct)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} was not found.");

        var access = await GetAccessAsync(ct);

        if (!access.CanModifyAnyRecipe)
        {
            if (!IsOwnedByCurrentUser(recipe, access) || !access.CanPublishOwnedRecipe)
            {
                throw new UnauthorizedAccessException("You do not have permission to publish this recipe.");
            }
        }

        if (!recipe.IsPublic)
        {
            recipe.IsPublic = true;
            await Repo.UpdateAsync(recipe, ct);
        }
    }

    public async Task UnpublishAsync(int recipeId, CancellationToken ct = default)
    {
        var recipe = await Repo.GetAsync(recipeId, ct)
            ?? throw new KeyNotFoundException($"Recipe {recipeId} was not found.");

        var access = await GetAccessAsync(ct);

        if (!access.CanModifyAnyRecipe)
        {
            if (!IsOwnedByCurrentUser(recipe, access) || !access.CanUnpublishOwnedRecipe)
            {
                throw new UnauthorizedAccessException("You do not have permission to unpublish this recipe.");
            }
        }

        if (recipe.IsPublic)
        {
            recipe.IsPublic = false;
            await Repo.UpdateAsync(recipe, ct);
        }
    }

    private async Task<RecipeAccess> GetAccessAsync(CancellationToken ct)
    {
        var currentUser = await _currentUserAccessor.GetCurrentUserAsync(ct);

        var canViewAnyRecipe = await _permissionService.HasPermissionAsync(RecipeAppPermissions.ViewAnyRecipe, ct);
        var canModifyAnyRecipe = await _permissionService.HasPermissionAsync(RecipeAppPermissions.ModifyAnyRecipe, ct);
        var canManageOwnedRecipe = await _permissionService.HasPermissionAsync(RecipeAppPermissions.ManageOwnedRecipe, ct);
        var canPublishOwnedRecipe = await _permissionService.HasPermissionAsync(RecipeAppPermissions.PublishOwnedRecipe, ct);
        var canUnpublishOwnedRecipe = await _permissionService.HasPermissionAsync(RecipeAppPermissions.UnpublishOwnedRecipe, ct);

        return new RecipeAccess(
            currentUser.IsAuthenticated,
            currentUser.UserId,
            canViewAnyRecipe,
            canModifyAnyRecipe,
            canManageOwnedRecipe,
            canPublishOwnedRecipe,
            canUnpublishOwnedRecipe);
    }

    private static bool CanView(Recipe recipe, RecipeAccess access)
    {
        if (access.CanViewAnyRecipe)
        {
            return true;
        }

        if (recipe.IsPublic)
        {
            return true;
        }

        return IsOwnedByCurrentUser(recipe, access);
    }

    private static bool CanModify(Recipe recipe, RecipeAccess access)
    {
        if (access.CanModifyAnyRecipe)
        {
            return true;
        }

        return access.CanManageOwnedRecipe && IsOwnedByCurrentUser(recipe, access);
    }

    private static bool IsOwnedByCurrentUser(Recipe recipe, RecipeAccess access)
        => !string.IsNullOrWhiteSpace(access.UserId)
           && string.Equals(recipe.UserId, access.UserId, StringComparison.Ordinal);

    private static void EnsureCanModify(Recipe recipe, RecipeAccess access)
    {
        if (!CanModify(recipe, access))
        {
            throw new UnauthorizedAccessException("You do not have permission to modify this recipe.");
        }
    }

    private static void EnsureVisibilityTransitionAllowed(Recipe existing, RecipeDto updated, RecipeAccess access)
    {
        if (existing.IsPublic == updated.IsPublic)
        {
            return;
        }

        if (access.CanModifyAnyRecipe)
        {
            return;
        }

        if (!IsOwnedByCurrentUser(existing, access))
        {
            throw new UnauthorizedAccessException("You do not have permission to change recipe visibility.");
        }

        if (updated.IsPublic && !access.CanPublishOwnedRecipe)
        {
            throw new UnauthorizedAccessException("You do not have permission to publish recipes you own.");
        }

        if (!updated.IsPublic && !access.CanUnpublishOwnedRecipe)
        {
            throw new UnauthorizedAccessException("You do not have permission to unpublish recipes you own.");
        }
    }

    private sealed record RecipeAccess(
        bool IsAuthenticated,
        string? UserId,
        bool CanViewAnyRecipe,
        bool CanModifyAnyRecipe,
        bool CanManageOwnedRecipe,
        bool CanPublishOwnedRecipe,
        bool CanUnpublishOwnedRecipe);
}
