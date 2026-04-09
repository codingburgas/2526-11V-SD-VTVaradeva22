using System.Security.Claims;
using TaskManager.Models.Entities;

namespace TaskManager.Helpers;

public static class ExtensionMethods
{
    public static string? GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public static string GetDisplayName(this ApplicationUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            return user.FullName;
        }

        return user.UserName ?? user.Email ?? "Unknown user";
    }
}
