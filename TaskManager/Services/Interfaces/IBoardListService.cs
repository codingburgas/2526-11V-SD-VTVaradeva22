using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Services.Interfaces;

public interface IBoardListService
{
    Task<BoardListViewModel?> GetEditModelAsync(int id, string userId, bool isAdmin);

    Task<ServiceResult<int>> CreateAsync(BoardListViewModel model, string userId, bool isAdmin);

    Task<ServiceResult> UpdateAsync(BoardListViewModel model, string userId, bool isAdmin);

    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin);
}
