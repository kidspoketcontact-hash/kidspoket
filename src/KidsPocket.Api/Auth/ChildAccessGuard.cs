using System.Security.Claims;
using KidsPocket.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KidsPocket.Api.Auth;

// בדיקת בעלות אחידה על ילד - Child token יכול לגעת רק בעצמו, Parent token
// יכול לגעת בכל ילד שמקושר (דרך ChildAdult) לבית האב שבטוקן שלו.
public static class ChildAccessGuard
{
    public static async Task<bool> CanAccessChildAsync(this ClaimsPrincipal user, Guid childId, KidsPocketDbContext db)
    {
        if (user.IsChild())
            return user.GetUserId() == childId;

        if (user.IsParent())
        {
            var householdId = user.GetHouseholdId();
            return await db.ChildAdults.AnyAsync(ca => ca.ChildId == childId && ca.HouseholdId == householdId);
        }

        return false;
    }
}
