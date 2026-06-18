    using System.Diagnostics;
    using Microsoft.AspNetCore.Mvc;
    using Career_Guidance_Platform.Models;

    namespace Career_Guidance_Platform.Controllers;

    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }
        public IActionResult Dashboard() => View();
        public IActionResult Users() => View();
        public IActionResult Mentors() => View();
        public IActionResult CareerTests() => View();
        public IActionResult Jobs() => View();
        public IActionResult Resources() => View();
        public IActionResult Reports() => View();
        public IActionResult Settings() => View();


        public IActionResult Terms() => View();
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }