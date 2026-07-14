using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Career_Guidance_Platform.Filters;

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
        [TypeFilter(typeof(PremiumAccessFilter))]
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
        [TypeFilter(typeof(PremiumAccessFilter))]
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

        // GET: /Course/Test/5
        [Authorize]
        public async Task<IActionResult> Test(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var progress = await _context.UserCourseProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == id);

            if (progress == null || progress.Status != "Learning")
            {
                TempData["CourseTestMessage"] = "Bạn cần bắt đầu học khóa học trước khi thực hiện bài kiểm tra.";
                return RedirectToAction("Detail", new { id });
            }

            var course = await _context.CareerPathCourses
                .Include(c => c.CareerPath)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: /Course/SubmitTest
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int courseId, int score)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var progress = await _context.UserCourseProgresses
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId);

            if (progress == null)
            {
                return NotFound("Không tìm thấy tiến độ khóa học.");
            }

            var course = await _context.CareerPathCourses.FindAsync(courseId);
            if (course == null)
            {
                return NotFound("Không tìm thấy khóa học.");
            }

            // Update course progress to completed
            progress.Status = "Completed";
            progress.ProgressPercent = 100;
            progress.TestPassed = true;
            progress.TestScore = score;
            _context.UserCourseProgresses.Update(progress);

            // Fetch user goals and milestones to mark matching ones as Completed
            var userGoals = await _context.Goals
                .Include(g => g.GoalMilestones)
                .Where(g => g.StudentId == userId && g.Status == 1)
                .ToListAsync();

            foreach (var goal in userGoals)
            {
                var milestonesToUpdate = goal.GoalMilestones
                    .Where(m => m.Status != "Completed" && 
                               (m.Title.Contains(course.Title) || m.ResourceId == course.Id))
                    .ToList();

                foreach (var milestone in milestonesToUpdate)
                {
                    milestone.Status = "Completed";
                    milestone.UpdatedAt = DateTime.Now;
                    _context.GoalMilestones.Update(milestone);
                }

                if (goal.GoalMilestones.Any())
                {
                    int completedCount = goal.GoalMilestones.Count(m => m.Status == "Completed");
                    goal.Progress = (int)Math.Round((double)completedCount / goal.GoalMilestones.Count * 100);
                    _context.Goals.Update(goal);
                }
            }

            // Add user skills from this course's career path
            // For simplicity, we assign a skill related to the course to the user
            var pathSkills = await _context.CareerPathSkills
                .Where(cps => cps.CareerPathId == course.CareerPathId)
                .Select(cps => cps.SkillId)
                .ToListAsync();

            if (pathSkills.Any())
            {
                // Give user the first skill as a reward
                var targetSkillId = pathSkills.First();
                var hasSkill = await _context.UserSkills
                    .AnyAsync(us => us.UserId == userId && us.SkillId == targetSkillId);

                if (!hasSkill)
                {
                    var userSkill = new UserSkill
                    {
                        UserId = userId,
                        SkillId = targetSkillId,
                        ProficiencyLevel = "Basic"
                    };
                    _context.UserSkills.Add(userSkill);
                }
            }

            await _context.SaveChangesAsync();

            TempData["CourseSuccessMessage"] = $"Chúc mừng bạn đã hoàn thành xuất sắc khóa học '{course.Title}' với điểm số {score}/100!";
            return RedirectToAction("Detail", new { id = courseId });
        }
    }
}