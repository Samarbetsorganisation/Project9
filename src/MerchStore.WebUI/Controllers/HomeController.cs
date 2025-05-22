using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MerchStore.WebUI.Models;
using Microsoft.AspNetCore.Authorization;

namespace MerchStore.WebUI.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [Authorize] // This attribute ensures that only authenticated users can access this action.
public IActionResult WhoAmI()
{
    return View();
}
}
