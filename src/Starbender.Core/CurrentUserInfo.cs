using System.Security.Claims;

namespace Starbender.Core;

public sealed record CurrentUserInfo(
    ClaimsPrincipal Principal,
    bool IsAuthenticated,
    string? UserId);
