using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    public IActionResult Index() => View();

    public IActionResult CareerTest() => View();

    public IActionResult CareerPath() => View();

    public IActionResult Training() => View();

    public IActionResult Jobs() => View();

    public IActionResult Community() => View();

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult FAQ() => View();

    public IActionResult Pricing() => View();

    public IActionResult News() => View();

    public IActionResult Policy() => View();

    public IActionResult Terms() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}