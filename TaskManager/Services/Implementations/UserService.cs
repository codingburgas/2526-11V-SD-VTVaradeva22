using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Constants;
using TaskManager.Data.Context;
using TaskManager.Models;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IReadOnlyCollection<UserManagementViewModel>> GetAllAsync()
    {
        // Load users for the admin screen.
        var users = await _context.Users
            .OrderBy(u => u.FullName)
            .ToListAsync();

        var result = new List<UserManagementViewModel>();

        foreach (var user in users)
        {
            // Combine Identity role data with task statistics.
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserManagementViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Role = roles.FirstOrDefault() ?? RoleNames.User,
                AssignedTasks = await _context.Tasks.CountAsync(t => t.AssignedToId == user.Id),
                CompletedTasks = await _context.Tasks.CountAsync(t => t.AssignedToId == user.Id && t.Status == TaskItemStatus.Done)
            });
        }

        return result;
    }

    public async Task<EditUserRoleViewModel?> GetEditModelAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new EditUserRoleViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            SelectedRole = roles.FirstOrDefault() ?? RoleNames.User,
            AvailableRoles = new[]
            {
                new SelectListItem(RoleNames.Admin, RoleNames.Admin),
                new SelectListItem(RoleNames.User, RoleNames.User)
            }
        };
    }

    public async Task<ServiceResult> UpdateRoleAsync(EditUserRoleViewModel model)
    {
        // Find the target user before changing roles.
        var user = await _userManager.FindByIdAsync(model.Id);
        if (user == null)
        {
            return ServiceResult.Failure("User was not found.");
        }

        var currentRoles = await _userManager.GetRolesAsync(user);

        // Do not allow the system to lose its last admin.
        if (currentRoles.Contains(RoleNames.Admin) && model.SelectedRole != RoleNames.Admin)
        {
            var adminCount = await _context.UserRoles
                .Join(_context.Roles,
                    userRole => userRole.RoleId,
                    role => role.Id,
                    (userRole, role) => new { userRole.UserId, role.Name })
                .CountAsync(x => x.Name == RoleNames.Admin);

            if (adminCount <= 1)
            {
                return ServiceResult.Failure("At least one admin account must remain in the system.");
            }
        }

        // Identity stores roles separately, so we remove old ones first.
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
        {
            return ServiceResult.Failure("Could not remove the current role.");
        }

        // Then add the new selected role.
        var addResult = await _userManager.AddToRoleAsync(user, model.SelectedRole);
        if (!addResult.Succeeded)
        {
            return ServiceResult.Failure("Could not assign the selected role.");
        }

        return ServiceResult.Success("User role updated successfully.");
    }
}
