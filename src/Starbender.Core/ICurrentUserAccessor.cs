namespace Starbender.Core;

public interface ICurrentUserAccessor
{
    Task<CurrentUserInfo> GetCurrentUserAsync(CancellationToken ct = default);
}
