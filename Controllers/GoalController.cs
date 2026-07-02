using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize]
    public class GoalController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public GoalController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Goal/GetSkillDetails
        [HttpGet]
        public async Task<IActionResult> GetSkillDetails(int skillId)
        {
            var skill = await _context.Skills
                .Include(s => s.Resources)
                .FirstOrDefaultAsync(s => s.Id == skillId && s.Status == 1);

            if (skill == null)
            {
                return NotFound(new { message = "Không tìm thấy kỹ năng." });
            }

            return Json(new
            {
                id = skill.Id,
                name = skill.Name,
                description = skill.Description,
                skillType = skill.SkillType,
                difficulty = skill.Difficulty,
                estimatedHours = skill.EstimatedHours,
                resources = skill.Resources.Select(r => new
                {
                    title = r.Title,
                    url = r.Url,
                    resourceType = r.ResourceType,
                    description = r.Description
                })
            });
        }

        // GET: /Goal
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Get user skills grouped by status
            var userSkills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId)
                .ToListAsync();

            var goals = userSkills.Where(us => us.Status == "In-Goals" || us.Status == "Learning").ToList();
            var completed = userSkills.Where(us => us.Status == "Completed" || us.Status == "Acquired").ToList();

            // Total Estimated Time = Sum of estimated hours of active skills in goals
            int totalEstimatedTime = goals.Sum(g => g.Skill?.EstimatedHours ?? 10);
            int totalCompletedTime = completed.Sum(c => c.Skill?.EstimatedHours ?? 10);

            ViewBag.TotalEstimatedTime = totalEstimatedTime;
            ViewBag.TotalCompletedTime = totalCompletedTime;
            ViewBag.Goals = goals;
            ViewBag.Completed = completed;

            // Fetch user's Resumes for CV management
            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();
            ViewBag.Resumes = resumes;

            return View();
        }

        // POST: /Goal/AddSkillToGoals
        [HttpPost]
        public async Task<IActionResult> AddSkillToGoals(int skillId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existing = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (existing != null)
            {
                if (existing.Status == "Completed" || existing.Status == "Acquired")
                {
                    return Json(new { success = false, message = "Kỹ năng này đã được bạn hoàn thành trước đó." });
                }
                if (existing.Status == "In-Goals" || existing.Status == "Learning")
                {
                    return Json(new { success = false, message = "Kỹ năng này đã có trong danh sách mục tiêu học tập của bạn." });
                }
                existing.Status = "In-Goals";
                _context.UserSkills.Update(existing);
            }
            else
            {
                var userSkill = new UserSkill
                {
                    UserId = userId,
                    SkillId = skillId,
                    Status = "In-Goals",
                    ProficiencyLevel = "Beginner"
                };
                _context.UserSkills.Add(userSkill);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã thêm kỹ năng vào danh sách Mục tiêu học tập!" });
        }

        // POST: /Goal/AddMultipleSkillsToGoals
        [HttpPost]
        public async Task<IActionResult> AddMultipleSkillsToGoals([FromBody] List<int> skillIds)
        {
            if (skillIds == null || !skillIds.Any())
            {
                return Json(new { success = false, message = "Không tìm thấy kỹ năng được chọn." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            int addedCount = 0;

            foreach (var skillId in skillIds)
            {
                var existing = await _context.UserSkills
                    .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

                if (existing != null)
                {
                    if (existing.Status != "Completed" && existing.Status != "Acquired" && existing.Status != "In-Goals" && existing.Status != "Learning")
                    {
                        existing.Status = "In-Goals";
                        _context.UserSkills.Update(existing);
                        addedCount++;
                    }
                }
                else
                {
                    var userSkill = new UserSkill
                    {
                        UserId = userId,
                        SkillId = skillId,
                        Status = "In-Goals",
                        ProficiencyLevel = "Beginner"
                    };
                    _context.UserSkills.Add(userSkill);
                    addedCount++;
                }
            }

            if (addedCount > 0)
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Đã thêm thành công {addedCount} kỹ năng vào Mục tiêu!" });
            }

            return Json(new { success = false, message = "Tất cả kỹ năng được chọn đã tồn tại trong danh sách của bạn." });
        }

        // POST: /Goal/StartLearning
        [HttpPost]
        public async Task<IActionResult> StartLearning(int skillId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (userSkill == null)
            {
                userSkill = new UserSkill
                {
                    UserId = userId,
                    SkillId = skillId,
                    Status = "Learning",
                    StartTimestamp = DateTime.Now,
                    ProficiencyLevel = "Beginner"
                };
                _context.UserSkills.Add(userSkill);
            }
            else
            {
                userSkill.Status = "Learning";
                userSkill.StartTimestamp = DateTime.Now;
                _context.UserSkills.Update(userSkill);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Learn", new { skillId });
        }

        // GET: /Goal/Learn/5
        public async Task<IActionResult> Learn(int skillId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var skill = await _context.Skills
                .Include(s => s.Resources)
                .FirstOrDefaultAsync(s => s.Id == skillId && s.Status == 1);

            if (skill == null)
            {
                return NotFound();
            }

            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            // Auto-transition to Learning if not already started
            if (userSkill == null)
            {
                userSkill = new UserSkill
                {
                    UserId = userId,
                    SkillId = skillId,
                    Status = "Learning",
                    StartTimestamp = DateTime.Now,
                    ProficiencyLevel = "Beginner"
                };
                _context.UserSkills.Add(userSkill);
                await _context.SaveChangesAsync();
            }
            else if (userSkill.Status == "In-Goals")
            {
                userSkill.Status = "Learning";
                userSkill.StartTimestamp = DateTime.Now;
                _context.UserSkills.Update(userSkill);
                await _context.SaveChangesAsync();
            }

            ViewBag.UserSkill = userSkill;
            return View(skill);
        }

        // GET: /Goal/Test/5
        public async Task<IActionResult> Test(int skillId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.Id == skillId && s.Status == 1);

            if (skill == null)
            {
                return NotFound();
            }

            var userSkill = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (userSkill == null || (userSkill.Status != "Learning" && userSkill.Status != "In-Goals" && userSkill.Status != "Completed"))
            {
                TempData["ErrorMessage"] = "Bạn cần bắt đầu học kỹ năng này trước khi làm kiểm tra.";
                return RedirectToAction("Learn", new { skillId });
            }

            // Check Cooldown status
            if (userSkill.CooldownUntil.HasValue && userSkill.CooldownUntil.Value > DateTime.Now)
            {
                var remainingSeconds = (int)(userSkill.CooldownUntil.Value - DateTime.Now).TotalSeconds;
                ViewBag.RemainingSeconds = remainingSeconds;
                ViewBag.CooldownUntil = userSkill.CooldownUntil.Value;
                return View("Cooldown", skill);
            }

            return View(skill);
        }

        // POST: /Goal/SubmitTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitTest(int skillId, List<string> answers)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var userSkill = await _context.UserSkills
                .Include(us => us.Skill)
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (userSkill == null)
            {
                return NotFound("Không tìm thấy tiến trình kỹ năng.");
            }

            // Validate Cooldown
            if (userSkill.CooldownUntil.HasValue && userSkill.CooldownUntil.Value > DateTime.Now)
            {
                TempData["ErrorMessage"] = "Bạn vẫn đang trong thời gian chờ kiểm tra lại.";
                return RedirectToAction("Test", new { skillId });
            }

            // Calculate Score (Simple mock checking answers: check if answers contain correct ones)
            // Each skill test has 5 questions. Correct options are predefined or mock graded.
            int score = 0;
            // Let's grade based on the selected answers: if they chose at least 4 options matching 'correct'
            if (answers != null)
            {
                int correctCount = answers.Count(a => a == "correct");
                score = correctCount * 20; // 5 questions, 20 points each
            }

            if (score >= 80)
            {
                // Passed
                userSkill.Status = "Completed";
                userSkill.ProficiencyLevel = "Intermediate"; // upgrade level
                userSkill.CooldownUntil = null; // clear cooldown
                _context.UserSkills.Update(userSkill);

                // Update goal milestones matching this skill
                var activeGoals = await _context.Goals
                    .Include(g => g.GoalMilestones)
                    .Where(g => g.StudentId == userId && g.Status == 1)
                    .ToListAsync();

                foreach (var goal in activeGoals)
                {
                    var milestones = goal.GoalMilestones
                        .Where(m => m.SkillId == skillId && m.Status != "Completed")
                        .ToList();

                    foreach (var m in milestones)
                    {
                        m.Status = "Completed";
                        m.UpdatedAt = DateTime.Now;
                        _context.GoalMilestones.Update(m);
                    }

                    if (goal.GoalMilestones.Any())
                    {
                        int completedMilestones = goal.GoalMilestones.Count(m => m.Status == "Completed");
                        goal.Progress = (int)Math.Round((double)completedMilestones / goal.GoalMilestones.Count * 100);
                        _context.Goals.Update(goal);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Chúc mừng! Bạn đã vượt qua bài kiểm tra kỹ năng '{userSkill.Skill?.Name}' với số điểm {score}/100!";
                return RedirectToAction("Index");
            }
            else
            {
                // Failed: Add cooldown of 120 seconds
                userSkill.CooldownUntil = DateTime.Now.AddSeconds(120);
                _context.UserSkills.Update(userSkill);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = $"Rất tiếc, bạn chỉ đạt {score}/100. Bạn cần tối thiểu 80/100 để hoàn tất kỹ năng này. Hãy ôn tập tài liệu và thử lại sau 2 phút.";
                return RedirectToAction("Learn", new { skillId });
            }
        }

        // GET: /Goal/ManageCV
        public async Task<IActionResult> ManageCV()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Fetch completed skills (inventory)
            var completedSkills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId && (us.Status == "Completed" || us.Status == "Acquired"))
                .Select(us => us.Skill.Name)
                .ToListAsync();

            var resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
                .ToListAsync();

            ViewBag.CompletedSkills = completedSkills;
            ViewBag.Resumes = resumes;

            return View();
        }

        // POST: /Goal/UpdateCV
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCV(int resumeId, List<string> selectedSkills)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);

            if (resume == null)
            {
                return NotFound("Không tìm thấy CV.");
            }

            if (selectedSkills == null)
            {
                selectedSkills = new List<string>();
            }

            // Sync skills to Resume ContentJson
            try
            {
                JsonNode jsonNode;
                if (string.IsNullOrEmpty(resume.ContentJson))
                {
                    jsonNode = new JsonObject();
                }
                else
                {
                    jsonNode = JsonNode.Parse(resume.ContentJson) ?? new JsonObject();
                }

                jsonNode["skills"] = string.Join(", ", selectedSkills);
                resume.ContentJson = jsonNode.ToJsonString();
                resume.UpdatedAt = DateTime.Now;

                _context.Resumes.Update(resume);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Đồng bộ thành công {selectedSkills.Count} kỹ năng vào CV '{resume.Title}'!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật CV: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // POST: /Goal/CreateCVFromSkills
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCVFromSkills(List<string> selectedSkills)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _userManager.GetUserAsync(User);

            if (selectedSkills == null)
            {
                selectedSkills = new List<string>();
            }

            // Generate CV details json
            var jsonObject = new JsonObject
            {
                ["fullName"] = user?.FullName ?? "",
                ["email"] = user?.Email ?? "",
                ["skills"] = string.Join(", ", selectedSkills),
                ["phone"] = user?.PhoneNumber ?? "",
                ["address"] = "",
                ["website"] = "",
                ["summary"] = $"Chuyên gia tiềm năng với các kỹ năng đã được kiểm chứng trên CareerPath: {string.Join(", ", selectedSkills)}.",
                ["experiences"] = new JsonArray(),
                ["educations"] = new JsonArray(),
                ["projects"] = new JsonArray()
            };

            var resume = new Resume
            {
                UserId = userId,
                Title = "CV - Lộ trình mục tiêu " + DateTime.Now.ToString("dd/MM/yyyy"),
                ContentJson = jsonObject.ToJsonString(),
                CreatedAt = DateTime.Now
            };

            _context.Resumes.Add(resume);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Tạo CV mẫu thành công! Bạn có thể chỉnh sửa thêm tại đây.";
            return RedirectToAction("ResumeBuilder", "Home", new { id = resume.Id });
        }
    }
}
