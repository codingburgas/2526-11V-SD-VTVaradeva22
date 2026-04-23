using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;

namespace TaskManager.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        // Logged-in users go straight to the dashboard.
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View();
    }

    public IActionResult Privacy()
    {
        // Simple static page.
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        // Show the current request id on the error page.
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
