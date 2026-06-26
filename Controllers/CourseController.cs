using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Career_Guidance_Platform.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _context;

        public CourseController(AppDbContext context)
        {
            _context = context;
        }

        // /Course/Detail/1
        public async Task<IActionResult> Detail(int id)
        {
            var course = await _context.CareerPathCourses
                .Include(c => c.CareerPath)
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == 1);

            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Resources = await _context.Resources
                .Where(r => r.PathId == course.CareerPathId && r.Status == 1)
                .ToListAsync();

            UserCourseProgress? progress = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userIdValue))
                {
                    var userId = int.Parse(userIdValue);

                    progress = await _context.UserCourseProgresses
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == id);

                    if (progress != null &&
                        progress.Status == "Learning" &&
                        DateTime.Now >= progress.DeadlineDate &&
                        progress.TestPassed == false)
                    {
                        progress.Status = "Expired";
                        _context.UserCourseProgresses.Update(progress);
                        await _context.SaveChangesAsync();
                    }
                }
            }

            ViewBag.Progress = progress;

            return View(course);
        }

        // /Course/Start/1
        [Authorize]
        public async Task<IActionResult> Start(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var progress = await _context.UserCourseProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == id);

            if (progress == null)
            {
                var course = await _context.CareerPathCourses.FindAsync(id);

                if (course == null)
                {
                    return NotFound();
                }

                progress = new UserCourseProgress
                {
                    UserId = userId,
                    CourseId = id,
                    StartDate = DateTime.Now,
                    DeadlineDate = DateTime.Now.AddDays(course.EstimatedDays),
                    ProgressPercent = 0,
                    Status = "Learning",
                    TestPassed = false,
                    TestScore = 0
                };

                _context.UserCourseProgresses.Add(progress);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Detail", new { id });
        }
    }
}