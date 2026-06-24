using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Career_Guidance_Platform.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var testResultId = HttpContext.Session.GetInt32("TestResultId");
                    if (testResultId.HasValue)
                    {
                        var user = await _userManager.FindByEmailAsync(model.Email);
                        var testResult = await _context.TestResults.FindAsync(testResultId.Value);
                        if (testResult != null)
                        {
                            testResult.UserId = user.Id;
                            var existingCount = await _context.TestResults.CountAsync(tr => tr.UserId == user.Id && tr.TestId == testResult.TestId);
                            testResult.AttemptNumber = existingCount + 1;
                            await _context.SaveChangesAsync();
                            HttpContext.Session.Remove("TestResultId");
                            return RedirectToAction("GetResultDetail", "CareerTest", new { id = testResult.Id });
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = model.Email, Email = model.Email, FullName = model.FullName };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    var testResultId = HttpContext.Session.GetInt32("TestResultId");
                    if (testResultId.HasValue)
                    {
                        var testResult = await _context.TestResults.FindAsync(testResultId.Value);
                        if (testResult != null)
                        {
                            testResult.UserId = user.Id;
                            var existingCount = await _context.TestResults.CountAsync(tr => tr.UserId == user.Id && tr.TestId == testResult.TestId);
                            testResult.AttemptNumber = existingCount + 1;
                            await _context.SaveChangesAsync();
                            HttpContext.Session.Remove("TestResultId");
                            return RedirectToAction("GetResultDetail", "CareerTest", new { id = testResult.Id });
                        }
                    }
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}