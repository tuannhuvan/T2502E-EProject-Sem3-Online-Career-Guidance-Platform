using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;

namespace Career_Guidance_Platform.Controllers
{
    public class ResumeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public ResumeController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Resume/Templates
        public async Task<IActionResult> Templates()
        {
            var templates = await _context.ResumeTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Id)
                .ToListAsync();

            bool isPremium = false;
            var userIdValue = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userIdValue))
            {
                var user = await _userManager.FindByIdAsync(userIdValue);
                if (user != null)
                {
                    isPremium = user.IsPremium;
                }
            }

            ViewBag.IsPremium = isPremium;
            return View(templates);
        }
    }
}
