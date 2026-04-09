using Microsoft.AspNetCore.Mvc.Rendering;
using TaskManager.Models;
using TaskManager.Models.DTOs;
using TaskManager.Models.Enums;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Services.Interfaces;

public interface ITaskService
{
    Task<TaskIndexViewModel> GetIndexAsync(string userId, bool isAdmin, int? boardId, Priority? priority, TaskItemStatus? status);

    Task<TaskViewModel> BuildCreateViewModelAsync(string userId, bool isAdmin, int? boardId = null);

    Task<TaskViewModel?> BuildEditViewModelAsync(int id, string userId, bool isAdmin);

    Task<IReadOnlyCollection<SelectListItem>> GetListOptionsAsync(int boardId, string userId, bool isAdmin);

    Task<ServiceResult<int>> CreateAsync(TaskViewModel model, string userId, bool isAdmin);

    Task<ServiceResult> UpdateAsync(TaskViewModel model, string userId, bool isAdmin);

    Task<TaskDto?> GetDetailsAsync(int id, string userId, bool isAdmin);

    Task<ServiceResult> DeleteAsync(int id, string userId, bool isAdmin);

    Task<ServiceResult> MoveAsync(int taskId, int targetListId, string userId, bool isAdmin);
}
