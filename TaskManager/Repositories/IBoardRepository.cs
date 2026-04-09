using TaskManager.Models.Entities;

namespace TaskManager.Repositories;

public interface IBoardRepository
{
    Task<IReadOnlyCollection<Board>> GetAllAsync(string userId, bool isAdmin);

    Task<Board?> GetByIdAsync(int id, string userId, bool isAdmin);

    Task AddAsync(Board board);
}
