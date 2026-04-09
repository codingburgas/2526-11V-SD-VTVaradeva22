using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Helpers;
using TaskManager.Models.Enums;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    public async Task<IActionResult> Index(int? boardId, Priority? priority, TaskItemStatus? status)
    {
        var model = await _taskService.GetIndexAsync(User.GetUserId()!, User.IsInRole("Admin"), boardId, priority, status);
        return View(model);
    }

    public async Task<IActionResult> Create(int? boardId)
    {
        var model = await _taskService.BuildCreateViewModelAsync(User.GetUserId()!, User.IsInRole("Admin"), boardId);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(TaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await RehydrateAsync(model));
        }

        var result = await _taskService.CreateAsync(model, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await RehydrateAsync(model));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = result.Data });
    }

    public async Task<IActionResult> Details(int id)
    {
        var task = await _taskService.GetDetailsAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _taskService.BuildEditViewModelAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await RehydrateAsync(model));
        }

        var result = await _taskService.UpdateAsync(model, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(await RehydrateAsync(model));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var task = await _taskService.GetDetailsAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _taskService.DeleteAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetLists(int boardId)
    {
        var lists = await _taskService.GetListOptionsAsync(boardId, User.GetUserId()!, User.IsInRole("Admin"));
        return Json(lists.Select(l => new { value = l.Value, text = l.Text }));
    }

    [HttpPost]
    public async Task<IActionResult> Move(int taskId, int targetListId)
    {
        var result = await _taskService.MoveAsync(taskId, targetListId, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Message });
        }

        return Ok(new { message = result.Message });
    }

    private async Task<TaskViewModel> RehydrateAsync(TaskViewModel model)
    {
        model.AvailableBoards = (await _taskService.BuildCreateViewModelAsync(User.GetUserId()!, User.IsInRole("Admin"), model.BoardId)).AvailableBoards;
        model.AvailableLists = await _taskService.GetListOptionsAsync(model.BoardId, User.GetUserId()!, User.IsInRole("Admin"));
        model.AvailableUsers = (await _taskService.BuildCreateViewModelAsync(User.GetUserId()!, User.IsInRole("Admin"), model.BoardId)).AvailableUsers;
        return model;
    }
}
