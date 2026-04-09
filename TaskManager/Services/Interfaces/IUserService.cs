using TaskManager.Models;
using TaskManager.ViewModels;

namespace TaskManager.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyCollection<UserManagementViewModel>> GetAllAsync();

    Task<EditUserRoleViewModel?> GetEditModelAsync(string id);

    Task<ServiceResult> UpdateRoleAsync(EditUserRoleViewModel model);
}
