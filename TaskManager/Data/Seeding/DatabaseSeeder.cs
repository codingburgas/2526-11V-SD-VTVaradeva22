using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskManager.Constants;
using TaskManager.Data.Context;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Data.Seeding;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(WebApplication app)
    {
        // Create a small service scope so we can resolve DbContext and Identity services.
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Apply pending migrations before adding demo data.
        await context.Database.MigrateAsync();

        // Make sure the main roles are always present.
        foreach (var role in new[] { RoleNames.Admin, RoleNames.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await EnsureUserAsync(
            userManager,
            "admin@taskmanager.local",
            "Admin123!",
            "Project Admin",
            RoleNames.Admin);

        // Create one normal user for testing the user role.
        var demoUser = await EnsureUserAsync(
            userManager,
            "user@taskmanager.local",
            "User123!",
            "Demo User",
            RoleNames.User);

        // Stop here if the sample board was already added before.
        if (await context.Boards.AnyAsync())
        {
            return;
        }

        // Add one sample board for quick demo use.
        var board = new Board
        {
            Name = "School Project Board",
            Description = "Sample board for the Task Manager project.",
            OwnerId = admin.Id
        };

        context.Boards.Add(board);
        await context.SaveChangesAsync();

        // Create the default Kanban columns.
        var todoList = new BoardList
        {
            BoardId = board.Id,
            Title = "To Do",
            Status = TaskItemStatus.ToDo,
            Position = 1
        };

        var progressList = new BoardList
        {
            BoardId = board.Id,
            Title = "In Progress",
            Status = TaskItemStatus.InProgress,
            Position = 2
        };

        var doneList = new BoardList
        {
            BoardId = board.Id,
            Title = "Done",
            Status = TaskItemStatus.Done,
            Position = 3
        };

        context.BoardLists.AddRange(todoList, progressList, doneList);
        await context.SaveChangesAsync();

        // Add a few sample tasks so the app is not empty on first run.
        context.Tasks.AddRange(
            new TaskItem
            {
                Title = "Create board CRUD",
                Description = "Implement create, edit and delete for boards.",
                Priority = Priority.High,
                Status = TaskItemStatus.Done,
                CompletedAt = DateTime.UtcNow.AddDays(-1),
                Deadline = DateTime.UtcNow.AddDays(1),
                BoardId = board.Id,
                BoardListId = doneList.Id,
                AssignedToId = admin.Id,
                Position = 1
            },
            new TaskItem
            {
                Title = "Finish dashboard statistics",
                Description = "Add progress, top users and deadline alerts.",
                Priority = Priority.High,
                Status = TaskItemStatus.InProgress,
                Deadline = DateTime.UtcNow.AddDays(2),
                BoardId = board.Id,
                BoardListId = progressList.Id,
                AssignedToId = demoUser.Id,
                Position = 1
            },
            new TaskItem
            {
                Title = "Prepare GitHub wiki",
                Description = "Document architecture, setup and screenshots.",
                Priority = Priority.Medium,
                Status = TaskItemStatus.ToDo,
                Deadline = DateTime.UtcNow.AddDays(5),
                BoardId = board.Id,
                BoardListId = todoList.Id,
                AssignedToId = demoUser.Id,
                Position = 1
            });

        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> EnsureUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string fullName,
        string role)
    {
        // Reuse the user if it already exists.
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                // Show a clear error if seeding fails.
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Could not seed user {email}: {errors}");
            }
        }

        // Make sure the user has the expected role.
        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }
}
