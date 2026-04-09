using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Helpers;
using TaskManager.Models.DTOs;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(string userId, bool isAdmin)
    {
        var query = _context.Tasks
            .Include(t => t.Board)
            .Include(t => t.BoardList)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(t => t.Board.OwnerId == userId || t.AssignedToId == userId);
        }

        var tasks = await query.ToListAsync();
        var weekStart = Utilities.StartOfWeekUtc(DateTime.UtcNow);
        var nextWeek = DateTime.UtcNow.Date.AddDays(7);

        return new DashboardViewModel
        {
            TotalTasks = tasks.Count,
            CompletedTasks = tasks.Count(t => t.Status == TaskItemStatus.Done),
            UpcomingDeadlinesCount = tasks.Count(t =>
                t.Deadline.HasValue &&
                t.Deadline.Value.Date >= DateTime.UtcNow.Date &&
                t.Deadline.Value.Date <= nextWeek &&
                t.Status != TaskItemStatus.Done),
            CompletionPercentage = tasks.Count == 0
                ? 0
                : Math.Round(tasks.Count(t => t.Status == TaskItemStatus.Done) * 100.0 / tasks.Count, 2),
            TasksByStatus = tasks
                .GroupBy(t => t.Status)
                .ToDictionary(group => Utilities.GetStatusLabel(group.Key), group => group.Count()),
            TopUsersThisWeek = tasks
                .Where(t => t.Status == TaskItemStatus.Done && t.CompletedAt.HasValue && t.CompletedAt >= weekStart && t.AssignedTo != null)
                .GroupBy(t => new { t.AssignedTo!.FullName, t.AssignedTo!.Email })
                .OrderByDescending(group => group.Count())
                .Take(5)
                .Select(group => new TopUserDto
                {
                    FullName = group.Key.FullName,
                    Email = group.Key.Email ?? string.Empty,
                    CompletedTasks = group.Count()
                })
                .ToList(),
            DeadlineAlerts = tasks
                .Where(t =>
                    t.Deadline.HasValue &&
                    t.Deadline.Value.Date >= DateTime.UtcNow.Date &&
                    t.Deadline.Value.Date <= nextWeek &&
                    t.Status != TaskItemStatus.Done)
                .OrderBy(t => t.Deadline)
                .Take(10)
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    BoardName = t.Board.Name,
                    ListTitle = t.BoardList.Title,
                    AssignedToName = t.AssignedTo?.FullName,
                    Deadline = t.Deadline,
                    Priority = t.Priority,
                    Status = t.Status,
                    IsOverdue = false,
                    CreatedAt = t.CreatedAt
                })
                .ToList()
        };
    }
}
