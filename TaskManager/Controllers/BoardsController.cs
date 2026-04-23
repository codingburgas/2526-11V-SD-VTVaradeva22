using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Helpers;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Controllers;

[Authorize]
public class BoardsController : Controller
{
    private readonly IBoardService _boardService;

    public BoardsController(IBoardService boardService)
    {
        _boardService = boardService;
    }

    public async Task<IActionResult> Index()
    {
        // Show all boards the current user can access.
        var boards = await _boardService.GetAllAsync(User.GetUserId()!, User.IsInRole("Admin"));
        return View(boards);
    }

    public async Task<IActionResult> Details(int id)
    {
        // Open one board with its lists and tasks.
        var board = await _boardService.GetDetailsAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (board == null)
        {
            return NotFound();
        }

        return View(board);
    }

    public IActionResult Create()
    {
        // Show an empty board form.
        return View(new BoardViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(BoardViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the board for the logged-in user.
        var boardId = await _boardService.CreateAsync(model, User.GetUserId()!);
        TempData["SuccessMessage"] = "Board created successfully.";
        return RedirectToAction(nameof(Details), new { id = boardId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        // Load the board data into the edit form.
        var model = await _boardService.GetEditModelAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(BoardViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var updated = await _boardService.UpdateAsync(model, User.GetUserId()!, User.IsInRole("Admin"));
        if (!updated)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Board updated successfully.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        // Show a small board summary before delete.
        var board = await _boardService.GetSummaryAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (board == null)
        {
            return NotFound();
        }

        return View(board);
    }

    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Remove the board if the current user has access to it.
        var deleted = await _boardService.DeleteAsync(id, User.GetUserId()!, User.IsInRole("Admin"));
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Board deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
