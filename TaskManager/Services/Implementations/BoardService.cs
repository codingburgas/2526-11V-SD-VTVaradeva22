using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Models.DTOs;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskManager.Repositories;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class BoardService : IBoardService
{
    private readonly ApplicationDbContext _context;
    private readonly IBoardRepository _repository;

    public BoardService(ApplicationDbContext context, IBoardRepository repository)
    {
        _context = context;
        _repository = repository;
    }

    public async Task<IReadOnlyCollection<BoardDto>> GetAllAsync(string userId, bool isAdmin)
    {
        var boards = await _repository.GetAllAsync(userId, isAdmin);
        return boards.Select(MapBoard).ToList();
    }

    public async Task<BoardDetailsViewModel?> GetDetailsAsync(int id, string userId, bool isAdmin)
    {
        var board = await _context.Boards
            .Include(b => b.Owner)
            .Include(b => b.Lists)
            .ThenInclude(l => l.Tasks)
            .ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(b => b.Id == id && (isAdmin || b.OwnerId == userId));

        if (board == null)
        {
            return null;
        }

        return new BoardDetailsViewModel
        {
            Board = MapBoard(board),
            Lists = board.Lists
                .OrderBy(l => l.Position)
                .Select(list => new BoardListDto
                {
                    Id = list.Id,
                    Title = list.Title,
                    Status = list.Status,
                    Position = list.Position,
                    Tasks = list.Tasks
                        .OrderBy(t => t.Position)
                        .Select(MapTask)
                        .ToList()
                })
                .ToList()
        };
    }

    public async Task<BoardViewModel?> GetEditModelAsync(int id, string userId, bool isAdmin)
    {
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == id && (isAdmin || b.OwnerId == userId));

        if (board == null)
        {
            return null;
        }

        return new BoardViewModel
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description
        };
    }

    public async Task<BoardDto?> GetSummaryAsync(int id, string userId, bool isAdmin)
    {
        var board = await _repository.GetByIdAsync(id, userId, isAdmin);
        return board == null ? null : MapBoard(board);
    }

    public async Task<int> CreateAsync(BoardViewModel model, string ownerId)
    {
        var board = new Board
        {
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            OwnerId = ownerId
        };

        await _repository.AddAsync(board);

        _context.BoardLists.AddRange(
            new BoardList
            {
                BoardId = board.Id,
                Title = "To Do",
                Status = TaskItemStatus.ToDo,
                Position = 1
            },
            new BoardList
            {
                BoardId = board.Id,
                Title = "In Progress",
                Status = TaskItemStatus.InProgress,
                Position = 2
            },
            new BoardList
            {
                BoardId = board.Id,
                Title = "Done",
                Status = TaskItemStatus.Done,
                Position = 3
            });

        await _context.SaveChangesAsync();
        return board.Id;
    }

    public async Task<bool> UpdateAsync(BoardViewModel model, string userId, bool isAdmin)
    {
        if (model.Id == null)
        {
            return false;
        }

        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == model.Id.Value && (isAdmin || b.OwnerId == userId));

        if (board == null)
        {
            return false;
        }

        board.Name = model.Name.Trim();
        board.Description = model.Description.Trim();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id, string userId, bool isAdmin)
    {
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == id && (isAdmin || b.OwnerId == userId));

        if (board == null)
        {
            return false;
        }

        _context.Boards.Remove(board);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyCollection<SelectListItem>> GetBoardOptionsAsync(string userId, bool isAdmin)
    {
        var boards = await _repository.GetAllAsync(userId, isAdmin);
        return boards
            .Select(b => new SelectListItem(b.Name, b.Id.ToString()))
            .ToList();
    }

    private static BoardDto MapBoard(Board board)
    {
        return new BoardDto
        {
            Id = board.Id,
            Name = board.Name,
            Description = board.Description,
            OwnerName = board.Owner.FullName,
            TotalTasks = board.Tasks.Count,
            CompletedTasks = board.Tasks.Count(t => t.Status == TaskItemStatus.Done),
            PendingTasks = board.Tasks.Count(t => t.Status != TaskItemStatus.Done)
        };
    }

    private static TaskDto MapTask(TaskItem task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            BoardName = task.Board?.Name ?? string.Empty,
            ListTitle = task.BoardList?.Title ?? string.Empty,
            AssignedToName = task.AssignedTo?.FullName,
            Deadline = task.Deadline,
            Priority = task.Priority,
            Status = task.Status,
            IsOverdue = task.Deadline.HasValue && task.Deadline.Value.Date < DateTime.UtcNow.Date && task.Status != TaskItemStatus.Done,
            CreatedAt = task.CreatedAt
        };
    }
}
