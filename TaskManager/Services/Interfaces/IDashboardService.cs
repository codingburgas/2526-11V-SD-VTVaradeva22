using TaskManager.ViewModels;

namespace TaskManager.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(string userId, bool isAdmin);
}
