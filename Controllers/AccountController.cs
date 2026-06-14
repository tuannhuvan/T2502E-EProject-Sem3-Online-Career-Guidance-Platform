using Microsoft.AspNetCore.Mvc;

namespace Career_Guidance_Platform.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }

    public IActionResult Register()
    {
        return View();
    }
}

