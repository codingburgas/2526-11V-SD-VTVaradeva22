using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Constants;
using TaskManager.Services.Interfaces;
using TaskManager.ViewModels;

namespace TaskManager.Controllers;

[Authorize(Roles = RoleNames.Admin)]
public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllAsync();
        return View(users);
    }

    public async Task<IActionResult> Edit(string id)
    {
        var model = await _userService.GetEditModelAsync(id);
        if (model == null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditUserRoleViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _userService.UpdateRoleAsync(model);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            var reloaded = await _userService.GetEditModelAsync(model.Id);
            return View(reloaded ?? model);
        }

        TempData["SuccessMessage"] = result.Message;
        return RedirectToAction(nameof(Index));
    }
}
