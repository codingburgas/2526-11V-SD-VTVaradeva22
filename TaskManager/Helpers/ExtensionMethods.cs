using System.Security.Claims;
using TaskManager.Models.Entities;

namespace TaskManager.Helpers;

public static class ExtensionMethods
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        // Read the logged-in user id from the Identity claims.
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string GetDisplayName(this ApplicationUser user)
    {
        // Prefer FullName, but fall back to UserName or Email if needed.
        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            return user.FullName;
        }

        return user.UserName ?? user.Email ?? "Unknown user";
    }
}
