using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Models.ViewModels;
using Career_Guidance_Platform.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;

        public ManageController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var testResults = await _context.TestResults
                .Include(tr => tr.RecommendedCareerPath)
                .Where(tr => tr.UserId == user.Id)
                .OrderByDescending(tr => tr.DateTaken)
                .ToListAsync();

            var resumes = await _context.Resumes
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();

            var jobApplications = await _context.JobApplications
                .Include(ja => ja.JobPosting)
                .Include(ja => ja.Resume)
                .Where(ja => ja.UserId == user.Id)
                .OrderByDescending(ja => ja.AppliedAt)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                TestResults = testResults,
                Resumes = resumes,
                JobApplications = jobApplications
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveResume([FromBody] SaveResumeDto model)
        {
            if (model == null || string.IsNullOrEmpty(model.ContentJson))
            {
                return BadRequest(new { success = false, message = "Dữ liệu CV không hợp lệ." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized(new { success = false, message = "Vui lòng đăng nhập để lưu CV." });
            }

            Resume resume;
            if (model.Id > 0)
            {
                resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == model.Id && r.UserId == user.Id);
                if (resume == null)
                {
                    return NotFound(new { success = false, message = "Không tìm thấy CV cần cập nhật." });
                }
                resume.Title = model.Title;
                resume.ContentJson = model.ContentJson;
                resume.UpdatedAt = System.DateTime.Now;
            }
            else
            {
                resume = new Resume
                {
                    UserId = user.Id,
                    Title = model.Title,
                    ContentJson = model.ContentJson,
                    CreatedAt = System.DateTime.Now
                };
                _context.Resumes.Add(resume);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, id = resume.Id, message = "Lưu CV thành công!" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteResume(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);
            if (resume == null)
            {
                return NotFound("Không tìm thấy CV.");
            }

            _context.Resumes.Remove(resume);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);
            return RedirectToAction("Index", new { Message = "Password changed successfully." });
        }
    }
}