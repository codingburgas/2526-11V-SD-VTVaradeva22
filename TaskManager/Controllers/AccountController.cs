using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Constants;
using TaskManager.Models.Entities;
using TaskManager.ViewModels;

namespace TaskManager.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public IActionResult Login()
    {
        // Show an empty login form
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Try to sign in with the entered email and password.
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // Go back to the requested page if it is safe.
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Default login target.
        return RedirectToAction("Index", "Dashboard");
    }

    [AllowAnonymous]
    public IActionResult Register()
    {
        // Show an empty register form.
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Create the new app user from the form data.
        var user = new ApplicationUser
        {
            FullName = model.FullName.Trim(),
            Email = model.Email.Trim(),
            UserName = model.Email.Trim(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            // Show all Identity validation errors in the form.
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // New registrations start as normal users.
        await _userManager.AddToRoleAsync(user, RoleNames.User);

        // Sign in right after successful registration.
        await _signInManager.SignInAsync(user, false);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        // End the current login session.
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
