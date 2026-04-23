using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Models;
using TaskManager.Models.Entities;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Implementations;

public class BoardListService : IBoardListService
{
    private readonly ApplicationDbContext _context;

    public BoardListService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BoardListViewModel?> GetEditModelAsync(int id, string userId, bool isAdmin)
    {
        // Load the list only if the current user can access its board.
        var list = await _context.BoardLists
            .Include(l => l.Board)
            .FirstOrDefaultAsync(l => l.Id == id && (isAdmin || l.Board.OwnerId == userId));

        if (list == null)
        {
            return null;
        }

        return new BoardListViewModel
        {
            Id = list.Id,
            BoardId = list.BoardId,
            Title = list.Title,
            Status = list.Status
        };
    }

    public async Task<ServiceResult<int>> CreateAsync(BoardListViewModel model, string userId, bool isAdmin)
    {
        // Only owners and admins can add lists to a board.
        var board = await _context.Boards
            .FirstOrDefaultAsync(b => b.Id == model.BoardId && (isAdmin || b.OwnerId == userId));

        if (board == null)
        {
            return ServiceResult<int>.Failure("You cannot create lists in this board.");
        }

        // Put the new list at the end of the current board.
        var maxPosition = await _context.BoardLists
            .Where(l => l.BoardId == model.BoardId)
            .MaxAsync(l => (int?)l.Position);

        var list = new BoardList
        {
            BoardId = model.BoardId,
            Title = model.Title.Trim(),
            Status = model.Status,
            Position = (maxPosition ?? 0) + 1
        };

        _context.BoardLists.Add(list);
        await _context.SaveChangesAsync();
        return ServiceResult<int>.Success(list.Id, "List created successfully.");
    }

    public async Task<ServiceResult> UpdateAsync(BoardListViewModel model, string userId, bool isAdmin)
    {
        if (model.Id == null)
        {
            return ServiceResult.Failure("List id is missing.");
        }

        var list = await _context.BoardLists
            .Include(l => l.Board)
            .Include(l => l.Tasks)
            .FirstOrDefaultAsync(l => l.Id == model.Id.Value && (isAdmin || l.Board.OwnerId == userId));

        if (list == null)
        {
            return ServiceResult.Failure("List was not found.");
        }

        list.Title = model.Title.Trim();
        if (list.Status != model.Status)
        {
            // If the list status changes, update all tasks inside it too.
            foreach (var task in list.Tasks)
            {
                task.Status = model.Status;
                task.CompletedAt = model.Status == TaskItemStatus.Done ? DateTime.UtcNow : null;
            }
        }

        list.Status = model.Status;
        await _context.SaveChangesAsync();
        return ServiceResult.Success("List updated successfully.");
    }

    public async Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin)
    {
        // Load the list together with its tasks before delete.
        var list = await _context.BoardLists
            .Include(l => l.Board)
            .Include(l => l.Tasks)
            .FirstOrDefaultAsync(l => l.Id == id && (isAdmin || l.Board.OwnerId == userId));

        if (list == null)
        {
            return ServiceResult.Failure("List was not found.");
        }

        if (list.Tasks.Any())
        {
            // Keep users from losing tasks by accident.
            return ServiceResult.Failure("Delete or move the tasks in this list before removing it.");
        }

        _context.BoardLists.Remove(list);
        await _context.SaveChangesAsync();
        return ServiceResult.Success("List deleted successfully.");
    }
}
