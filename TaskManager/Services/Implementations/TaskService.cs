using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Models;
using TaskManager.Models.DTOs;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TaskIndexViewModel> GetIndexAsync(string userId, bool isAdmin, int? boardId, Priority? priority, TaskItemStatus? status)
    {
        // Start with only the tasks this user is allowed to see.
        var query = QueryAccessibleTasks(userId, isAdmin);

        // Apply optional filters from the tasks page.
        if (boardId.HasValue)
        {
            query = query.Where(t => t.BoardId == boardId.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        // Build the final list model for the UI table.
        var tasks = await query
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Deadline)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                BoardName = t.Board.Name,
                ListTitle = t.BoardList.Title,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                Deadline = t.Deadline,
                Priority = t.Priority,
                Status = t.Status,
                IsOverdue = t.Deadline.HasValue && t.Deadline.Value.Date < DateTime.UtcNow.Date && t.Status != TaskItemStatus.Done,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new TaskIndexViewModel
        {
            Tasks = tasks,
            SelectedBoardId = boardId,
            SelectedPriority = priority,
            SelectedStatus = status,
            Boards = await GetBoardOptionsForTaskFormsAsync(userId, isAdmin)
        };
    }

    public async Task<TaskViewModel> BuildCreateViewModelAsync(string userId, bool isAdmin, int? boardId = null)
    {
        // Pick the requested board, or the first available one if none was passed.
        var boards = await GetBoardOptionsForTaskFormsAsync(userId, isAdmin);
        var resolvedBoardId = boardId ?? boards.Select(b => int.Parse(b.Value)).FirstOrDefault();

        return new TaskViewModel
        {
            BoardId = resolvedBoardId,
            AvailableBoards = boards,
            AvailableLists = resolvedBoardId == 0
                ? []
                : await GetListOptionsAsync(resolvedBoardId, userId, isAdmin),
            AvailableUsers = await GetUserOptionsAsync(userId, isAdmin)
        };
    }

    public async Task<TaskViewModel?> BuildEditViewModelAsync(int id, string userId, bool isAdmin)
    {
        var task = await QueryAccessibleTasks(userId, isAdmin)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return null;
        }

        return new TaskViewModel
        {
            Id = task.Id,
            BoardId = task.BoardId,
            BoardListId = task.BoardListId,
            Title = task.Title,
            Description = task.Description,
            Deadline = task.Deadline,
            Priority = task.Priority,
            AssignedToId = task.AssignedToId,
            AvailableBoards = await GetBoardOptionsForTaskFormsAsync(userId, isAdmin),
            AvailableLists = await GetListOptionsAsync(task.BoardId, userId, isAdmin),
            AvailableUsers = await GetUserOptionsAsync(userId, isAdmin)
        };
    }

    public async Task<IReadOnlyCollection<SelectListItem>> GetListOptionsAsync(int boardId, string userId, bool isAdmin)
    {
        // Users can open lists only for boards they own, can access, or manage as admin.
        var hasAccess = await _context.Boards
            .AnyAsync(b => b.Id == boardId && (isAdmin || b.OwnerId == userId || b.Tasks.Any(t => t.AssignedToId == userId)));

        if (!hasAccess)
        {
            return [];
        }

        return await _context.BoardLists
            .Where(l => l.BoardId == boardId)
            .OrderBy(l => l.Position)
            .Select(l => new SelectListItem($"{l.Title} ({l.Status})", l.Id.ToString()))
            .ToListAsync();
    }

    public async Task<ServiceResult<int>> CreateAsync(TaskViewModel model, string userId, bool isAdmin)
    {
        // Check board access before creating a new task.
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == model.BoardId && (isAdmin || b.OwnerId == userId || b.Tasks.Any(t => t.AssignedToId == userId)));

        if (board == null)
        {
            return ServiceResult<int>.Failure("You cannot create tasks in this board.");
        }

        var list = await _context.BoardLists
            .FirstOrDefaultAsync(l => l.Id == model.BoardListId && l.BoardId == board.Id);

        if (list == null)
        {
            return ServiceResult<int>.Failure("The selected list does not belong to this board.");
        }

        // Only admins can freely assign tasks to another user.
        var assignedToId = isAdmin ? model.AssignedToId : userId;
        var position = await GetNextTaskPositionAsync(list.Id);

        // The list controls the current task status.
        var task = new TaskItem
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Deadline = model.Deadline,
            Priority = model.Priority,
            Status = list.Status,
            CompletedAt = list.Status == TaskItemStatus.Done ? DateTime.UtcNow : null,
            BoardId = board.Id,
            BoardListId = list.Id,
            AssignedToId = string.IsNullOrWhiteSpace(assignedToId) ? userId : assignedToId,
            Position = position
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return ServiceResult<int>.Success(task.Id, "Task created successfully.");
    }

    public async Task<ServiceResult> UpdateAsync(TaskViewModel model, string userId, bool isAdmin)
    {
        if (model.Id == null)
        {
            return ServiceResult.Failure("Task id is missing.");
        }

        var task = await QueryAccessibleTasks(userId, isAdmin)
            .FirstOrDefaultAsync(t => t.Id == model.Id.Value);

        if (task == null)
        {
            return ServiceResult.Failure("Task was not found.");
        }

        // A normal user cannot move the task to a board they do not have access to.
        var targetBoardId = isAdmin || await UserHasBoardAccessAsync(model.BoardId, userId)
            ? model.BoardId
            : task.BoardId;

        var list = await _context.BoardLists
            .FirstOrDefaultAsync(l => l.Id == model.BoardListId && l.BoardId == targetBoardId);

        if (list == null)
        {
            return ServiceResult.Failure("The selected list is invalid.");
        }

        // Save the old status so we can update CompletedAt correctly.
        var oldStatus = task.Status;
        task.Title = model.Title.Trim();
        task.Description = model.Description.Trim();
        task.Deadline = model.Deadline;
        task.Priority = model.Priority;
        task.BoardId = targetBoardId;
        task.BoardListId = list.Id;
        task.Status = list.Status;
        task.AssignedToId = isAdmin ? model.AssignedToId : task.AssignedToId ?? userId;
        task.CompletedAt = ResolveCompletedAt(oldStatus, task.Status, task.CompletedAt);

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Task updated successfully.");
    }

    public async Task<TaskDto?> GetDetailsAsync(int id, string userId, bool isAdmin)
    {
        return await QueryAccessibleTasks(userId, isAdmin)
            .Where(t => t.Id == id)
            .Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                BoardName = t.Board.Name,
                ListTitle = t.BoardList.Title,
                AssignedToName = t.AssignedTo != null ? t.AssignedTo.FullName : null,
                Deadline = t.Deadline,
                Priority = t.Priority,
                Status = t.Status,
                IsOverdue = t.Deadline.HasValue && t.Deadline.Value.Date < DateTime.UtcNow.Date && t.Status != TaskItemStatus.Done,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var task = await QueryAccessibleTasks(userId, isAdmin)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            return ServiceResult.Failure("Task was not found.");
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return ServiceResult.Success("Task deleted successfully.");
    }

    public async Task<ServiceResult> MoveAsync(int taskId, int targetListId, string userId, bool isAdmin)
    {
        // Used when a task is dragged to another list.
        var task = await QueryAccessibleTasks(userId, isAdmin)
            .FirstOrDefaultAsync(t => t.Id == taskId);

        if (task == null)
        {
            return ServiceResult.Failure("Task was not found.");
        }

        var targetList = await _context.BoardLists
            .FirstOrDefaultAsync(l => l.Id == targetListId && l.BoardId == task.BoardId);

        if (targetList == null)
        {
            return ServiceResult.Failure("Cannot move task to the selected list.");
        }

        // Moving to another list also changes the task status.
        var oldStatus = task.Status;
        task.BoardListId = targetList.Id;
        task.Status = targetList.Status;
        task.Position = await GetNextTaskPositionAsync(targetList.Id);
        task.CompletedAt = ResolveCompletedAt(oldStatus, task.Status, task.CompletedAt);

        await _context.SaveChangesAsync();
        return ServiceResult.Success("Task moved successfully.");
    }

    private IQueryable<TaskItem> QueryAccessibleTasks(string userId, bool isAdmin)
    {
        // Load all related data needed by the task pages.
        var query = _context.Tasks
            .Include(t => t.Board)
            .Include(t => t.BoardList)
            .Include(t => t.AssignedTo)
            .AsQueryable();

        // Normal users can only see owned boards or tasks assigned to them.
        if (!isAdmin)
        {
            query = query.Where(t => t.Board.OwnerId == userId || t.AssignedToId == userId);
        }

        return query;
    }

    private async Task<IReadOnlyCollection<SelectListItem>> GetBoardOptionsForTaskFormsAsync(string userId, bool isAdmin)
    {
        return await _context.Boards
            .Where(b => isAdmin || b.OwnerId == userId || b.Tasks.Any(t => t.AssignedToId == userId))
            .OrderBy(b => b.Name)
            .Select(b => new SelectListItem(b.Name, b.Id.ToString()))
            .ToListAsync();
    }

    private async Task<IReadOnlyCollection<SelectListItem>> GetUserOptionsAsync(string userId, bool isAdmin)
    {
        if (!isAdmin)
        {
            // Normal users can only assign tasks to themselves.
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new SelectListItem(u.FullName, u.Id))
                .ToListAsync();
        }

        return await _context.Users
            .OrderBy(u => u.FullName)
            .Select(u => new SelectListItem($"{u.FullName} ({u.Email})", u.Id))
            .ToListAsync();
    }

    private async Task<int> GetNextTaskPositionAsync(int boardListId)
    {
        // Keep task order inside the selected list.
        var maxPosition = await _context.Tasks
            .Where(t => t.BoardListId == boardListId)
            .MaxAsync(t => (int?)t.Position);

        return (maxPosition ?? 0) + 1;
    }

    private async Task<bool> UserHasBoardAccessAsync(int boardId, string userId)
    {
        return await _context.Boards.AnyAsync(b => b.Id == boardId && (b.OwnerId == userId || b.Tasks.Any(t => t.AssignedToId == userId)));
    }

    private static DateTime? ResolveCompletedAt(TaskItemStatus oldStatus, TaskItemStatus newStatus, DateTime? currentCompletedAt)
    {
        // Set completion time only when the task enters Done.
        if (newStatus == TaskItemStatus.Done && oldStatus != TaskItemStatus.Done)
        {
            return DateTime.UtcNow;
        }

        // Clear completion time if the task leaves Done.
        if (newStatus != TaskItemStatus.Done)
        {
            return null;
        }

        return currentCompletedAt;
    }
}
