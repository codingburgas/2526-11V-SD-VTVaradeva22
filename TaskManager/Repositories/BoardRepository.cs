using Microsoft.EntityFrameworkCore;
using TaskManager.Data.Context;
using TaskManager.Models.Entities;

namespace TaskManager.Repositories;

public class BoardRepository : IBoardRepository
{
    private readonly ApplicationDbContext _context;

    public BoardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Board>> GetAllAsync(string userId, bool isAdmin)
    {
        // Load boards with owner and task data for the board list page.
        return await QueryAccessibleBoards(userId, isAdmin)
            .Include(b => b.Owner)
            .Include(b => b.Tasks)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Board?> GetByIdAsync(int id, string userId, bool isAdmin)
    {
        // Load one board with related data for summary use.
        return await QueryAccessibleBoards(userId, isAdmin)
            .Include(b => b.Owner)
            .Include(b => b.Lists)
            .Include(b => b.Tasks)
            .ThenInclude(t => t.AssignedTo)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task AddAsync(Board board)
    {
        // Save a new board to the database.
        _context.Boards.Add(board);
        await _context.SaveChangesAsync();
    }

    private IQueryable<Board> QueryAccessibleBoards(string userId, bool isAdmin)
    {
        // Start with all boards, then limit them for normal users.
        var query = _context.Boards.AsQueryable();
        if (!isAdmin)
        {
            query = query.Where(b => b.OwnerId == userId);
        }

        // Repository read queries do not need change tracking.
        return query.AsNoTracking();
    }
}
