using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Helpers;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Controllers;

[Authorize]
public class ListsController : Controller
{
    private readonly IBoardListService _listService;

    public ListsController(IBoardListService listService)
    {
        _listService = listService;
    }

    public IActionResult Create(int boardId)
    {
        return View(new BoardListViewModel { BoardId = boardId });
    }

    [HttpPost]
    public async Task<IActionResult> Create(BoardListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _listService.CreateAsync(model, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Details", "Boards", new { id = model.BoardId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _listService.GetEditModelAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BoardListViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _listService.UpdateAsync(model, User.GetUserId()!, User.IsInRole("Admin"));
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction("Details", "Boards", new { id = model.BoardId });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var model = await _listService.GetEditModelAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, int boardId)
    {
        var result = await _listService.DeleteAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction("Details", "Boards", new { id = boardId });
    }
}
