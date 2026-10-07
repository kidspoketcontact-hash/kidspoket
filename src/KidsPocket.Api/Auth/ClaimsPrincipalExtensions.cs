using System.Security.Claims;

namespace KidsPocket.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public static Guid? GetHouseholdId(this ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(AppClaimTypes.HouseholdId);
        return value is null ? null : Guid.Parse(value);
    }

    public static bool IsParent(this ClaimsPrincipal user) => user.IsInRole(AppRoles.Parent);
    public static bool IsChild(this ClaimsPrincipal user) => user.IsInRole(AppRoles.Child);
}
