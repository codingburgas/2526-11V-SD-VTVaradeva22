using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Models.DTOs;
using TaskManager.ViewModels;

namespace TaskManager.Services.Interfaces;

public interface IBoardService
{
    Task<IReadOnlyCollection<BoardDto>> GetAllAsync(string userId, bool isAdmin);

    Task<BoardDetailsViewModel?> GetDetailsAsync(int id, string userId, bool isAdmin);

    Task<BoardViewModel?> GetEditModelAsync(int id, string userId, bool isAdmin);

    Task<BoardDto?> GetSummaryAsync(int id, string userId, bool isAdmin);

    Task<int> CreateAsync(BoardViewModel model, string ownerId);

    Task<bool> UpdateAsync(BoardViewModel model, string userId, bool isAdmin);

    Task<bool> DeleteAsync(int id, string userId, bool isAdmin);

    Task<IReadOnlyCollection<SelectListItem>> GetBoardOptionsAsync(string userId, bool isAdmin);
}
