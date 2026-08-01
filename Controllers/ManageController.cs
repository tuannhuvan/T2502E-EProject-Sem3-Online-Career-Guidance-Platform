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
        private readonly IConfiguration _configuration;

        public ManageController(UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext context, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
        }
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var isPremiumOrSpecial = user.IsPremium || User.IsInRole("Admin") || User.IsInRole("Mentor");
            var testResultsQuery = _context.TestResults
                .Include(tr => tr.RecommendedCareerPath)
                .Where(tr => tr.UserId == user.Id)
                .OrderBy(tr => tr.AttemptNumber);

            var testResults = isPremiumOrSpecial
                ? await testResultsQuery.ToListAsync()
                : await testResultsQuery.Take(3).ToListAsync();

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

            var savedJobs = await _context.SavedJobs
                .Include(sj => sj.JobPosting)
                    .ThenInclude(jp => jp.CareerPath)
                .Where(sj => sj.UserId == user.Id)
                .OrderByDescending(sj => sj.SavedAt)
                .ToListAsync();

            var goals = await _context.Goals
                .Include(g => g.CareerPath)
                .Where(g => g.StudentId == user.Id)
                .OrderByDescending(g => g.CreatedAt)
                .ToListAsync();

            var mentorshipMeetings = await _context.MentorshipMeetings
                .Include(mm => mm.Mentor)
                .Where(mm => mm.MenteeId == user.Id)
                .OrderByDescending(mm => mm.ScheduledTime)
                .ToListAsync();

            var viewModel = new ProfileViewModel
            {
                User = user,
                TestResults = testResults,
                Resumes = resumes,
                JobApplications = jobApplications,
                SavedJobs = savedJobs,
                Goals = goals,
                MentorshipMeetings = mentorshipMeetings
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
                resume.TemplateId = model.TemplateId;
                resume.UpdatedAt = System.DateTime.Now;
            }
            else
            {
                resume = new Resume
                {
                    UserId = user.Id,
                    Title = model.Title,
                    ContentJson = model.ContentJson,
                    TemplateId = model.TemplateId,
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

        [HttpPost]
        public async Task<IActionResult> UpdateAvatar(IFormFile avatarFile)
        {
            if (avatarFile == null || avatarFile.Length == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn một tệp hình ảnh." });
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = System.IO.Path.GetExtension(avatarFile.FileName).ToLower();
            if (!allowedExtensions.Contains(extension))
            {
                return Json(new { success = false, message = "Định dạng file không được hỗ trợ. Chỉ chấp nhận JPG, PNG, GIF, WEBP." });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var cloudName = _configuration["CloudinarySettings:CloudName"];
            var apiKey = _configuration["CloudinarySettings:ApiKey"];
            var apiSecret = _configuration["CloudinarySettings:ApiSecret"];

            if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            {
                return Json(new { success = false, message = "Cấu hình Cloudinary không hợp lệ hoặc thiếu thông tin." });
            }

            var account = new CloudinaryDotNet.Account(cloudName, apiKey, apiSecret);
            var cloudinary = new CloudinaryDotNet.Cloudinary(account);

            using (var stream = avatarFile.OpenReadStream())
            {
                var uploadParams = new CloudinaryDotNet.Actions.ImageUploadParams()
                {
                    File = new CloudinaryDotNet.FileDescription(avatarFile.FileName, stream),
                    Folder = "avatars",
                    Transformation = new CloudinaryDotNet.Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                };

                var uploadResult = await cloudinary.UploadAsync(uploadParams);
                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    user.AvatarUrl = uploadResult.SecureUrl.ToString();
                    await _userManager.UpdateAsync(user);
                    return Json(new { success = true, avatarUrl = user.AvatarUrl, message = "Cập nhật ảnh đại diện lên Cloudinary thành công!" });
                }
                else
                {
                    return Json(new { success = false, message = $"Lỗi tải lên Cloudinary: {uploadResult.Error?.Message}" });
                }
            }
        }
    }
}