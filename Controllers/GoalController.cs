using System.Security.Claims;
using System.Text.Json.Nodes;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers;

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

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    // GET: /Goal
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Goals", "Home");
        }

        var userId = GetCurrentUserId();

        var personalGoals = await _context.Goals
            .Include(g => g.CareerPath)
            .Include(g => g.GoalMilestones)
            .ThenInclude(m => m.Skill)
            .Where(g => g.StudentId == userId && g.Status != 3)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var completed = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId &&
                         (us.Status == "Completed" || us.Status == "Acquired"))
            .ToListAsync();

        var allSkills = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId)
            .ToListAsync();

        var resumes = await _context.Resumes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync();

        ViewBag.Completed = completed;
        ViewBag.AllSkills = allSkills;
        ViewBag.Resumes = resumes;

        return View(personalGoals);
    }

    // GET: /Goal/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();

        var goal = await _context.Goals
            .Include(g => g.CareerPath)
            .Include(g => g.GoalMilestones)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        return View(goal);
    }

    // GET: /Goal/Create
    public async Task<IActionResult> Create()
    {
        var userId = GetCurrentUserId();

        // Premium limitation: Free users can only create 3 goals
        var totalGoalsCount = await _context.Goals
            .CountAsync(g => g.StudentId == userId && g.Status != 3);

        if (totalGoalsCount >= 3)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsPremium)
            {
                TempData["PremiumLimitMessage"] = "Tính năng tạo nhiều hơn 3 mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
                return RedirectToAction("UpgradePremium", "Home");
            }
        }

        var hasActiveGoal = await _context.Goals
            .AnyAsync(g => g.StudentId == userId && g.Status != 3 && g.Progress < 100);

        if (hasActiveGoal)
        {
            TempData["ErrorMessage"] = "Bạn chưa hoàn thành mục tiêu trước đó. Hãy hoàn thành mục tiêu hiện tại trước khi tạo mục tiêu mới!";
            return RedirectToAction(nameof(Index));
        }

        await LoadCareerPaths();
        return View();
    }

    // POST: /Goal/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Goal goal, List<int> selectedSkills)
    {
        var userId = GetCurrentUserId();

        // Premium limitation: Free users can only create 3 goals
        var totalGoalsCount = await _context.Goals
            .CountAsync(g => g.StudentId == userId && g.Status != 3);

        if (totalGoalsCount >= 3)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsPremium)
            {
                TempData["PremiumLimitMessage"] = "Tính năng tạo nhiều hơn 3 mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
                return RedirectToAction("UpgradePremium", "Home");
            }
        }

        var hasActiveGoal = await _context.Goals
            .AnyAsync(g => g.StudentId == userId && g.Status != 3 && g.Progress < 100);

        if (hasActiveGoal)
        {
            TempData["ErrorMessage"] = "Bạn chưa hoàn thành mục tiêu trước đó. Hãy hoàn thành mục tiêu hiện tại trước khi tạo mục tiêu mới!";
            return RedirectToAction(nameof(Index));
        }

        goal.StudentId = userId;
        goal.CreatedAt = DateTime.Now;
        goal.CreatedBy = User.Identity?.Name ?? "User";
        goal.Status = 1;

        selectedSkills ??= new List<int>();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .ToDictionaryAsync(us => us.SkillId, us => us.Status);

        int completedCount = 0;
        int totalCount = selectedSkills.Count;

        foreach (var skillId in selectedSkills)
        {
            var skill = await _context.Skills.FindAsync(skillId);
            if (skill != null)
            {
                var isCompleted = userSkills.ContainsKey(skillId) && 
                                  (userSkills[skillId] == "Completed" || userSkills[skillId] == "Acquired");
                if (isCompleted)
                {
                    completedCount++;
                }

                goal.GoalMilestones.Add(new GoalMilestone
                {
                    Title = $"Hoàn thành kỹ năng {skill.Name}",
                    SkillId = skillId,
                    Status = isCompleted ? "Completed" : "In Progress",
                    SequenceOrder = goal.GoalMilestones.Count + 1
                });
            }
        }

        goal.Progress = totalCount > 0 ? (int)Math.Round((double)completedCount / totalCount * 100) : 0;

        ModelState.Remove(nameof(goal.StudentId));
        ModelState.Remove(nameof(goal.Student));

        if (!ModelState.IsValid)
        {
            await LoadCareerPaths();
            return View(goal);
        }

        _context.Goals.Add(goal);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tạo mục tiêu thành công!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Goal/GetSkillsByCareerPath?careerPathId=5
    [HttpGet]
    public async Task<IActionResult> GetSkillsByCareerPath(int careerPathId)
    {
        var userId = GetCurrentUserId();

        var pathSkills = await _context.CareerPathSkills
            .Include(cps => cps.Skill)
            .Where(cps => cps.CareerPathId == careerPathId && cps.Skill != null && cps.Skill.Status == 1)
            .Select(cps => cps.Skill!)
            .ToListAsync();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .ToDictionaryAsync(us => us.SkillId, us => us.Status);

        var result = pathSkills.Select(s => new {
            s.Id,
            s.Name,
            s.Description,
            Status = userSkills.ContainsKey(s.Id) ? userSkills[s.Id] : "Not Started"
        });

        return Json(result);
    }

    // GET: /Goal/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsPremium)
        {
            TempData["PremiumLimitMessage"] = "Tính năng chỉnh sửa hoặc xóa mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
            return RedirectToAction("UpgradePremium", "Home");
        }

        var goal = await _context.Goals
            .Include(g => g.GoalMilestones)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        await LoadCareerPaths();
        return View(goal);
    }

    // POST: /Goal/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Goal input, List<int> selectedSkills)
    {
        if (id != input.Id) return NotFound();

        var userId = GetCurrentUserId();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsPremium)
        {
            TempData["PremiumLimitMessage"] = "Tính năng chỉnh sửa hoặc xóa mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
            return RedirectToAction("UpgradePremium", "Home");
        }

        var goal = await _context.Goals
            .Include(g => g.GoalMilestones)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        selectedSkills ??= new List<int>();

        // Remove old milestones associated with skills
        _context.GoalMilestones.RemoveRange(goal.GoalMilestones);
        goal.GoalMilestones.Clear();

        var userSkills = await _context.UserSkills
            .Where(us => us.UserId == userId)
            .ToDictionaryAsync(us => us.SkillId, us => us.Status);

        int completedCount = 0;
        int totalCount = selectedSkills.Count;

        foreach (var skillId in selectedSkills)
        {
            var skill = await _context.Skills.FindAsync(skillId);
            if (skill != null)
            {
                var isCompleted = userSkills.ContainsKey(skillId) && 
                                  (userSkills[skillId] == "Completed" || userSkills[skillId] == "Acquired");
                if (isCompleted)
                {
                    completedCount++;
                }

                goal.GoalMilestones.Add(new GoalMilestone
                {
                    Title = $"Hoàn thành kỹ năng {skill.Name}",
                    SkillId = skillId,
                    Status = isCompleted ? "Completed" : "In Progress",
                    SequenceOrder = goal.GoalMilestones.Count + 1
                });
            }
        }

        input.Progress = totalCount > 0 ? (int)Math.Round((double)completedCount / totalCount * 100) : 0;

        ModelState.Remove(nameof(input.StudentId));
        ModelState.Remove(nameof(input.Student));

        if (!ModelState.IsValid)
        {
            await LoadCareerPaths();
            return View(input);
        }

        goal.Title = input.Title;
        goal.GoalType = input.GoalType;
        goal.CareerPathId = input.CareerPathId;
        goal.Progress = input.Progress;
        goal.TargetDate = input.TargetDate;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "User";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật mục tiêu thành công!";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Goal/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsPremium)
        {
            TempData["PremiumLimitMessage"] = "Tính năng chỉnh sửa hoặc xóa mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
            return RedirectToAction("UpgradePremium", "Home");
        }

        var goal = await _context.Goals
            .Include(g => g.CareerPath)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        return View(goal);
    }

    // POST: /Goal/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetCurrentUserId();

        var user = await _context.Users.FindAsync(userId);
        if (user == null || !user.IsPremium)
        {
            TempData["PremiumLimitMessage"] = "Tính năng chỉnh sửa hoặc xóa mục tiêu (Goal) yêu cầu tài khoản Premium VIP. Vui lòng nâng cấp để tiếp tục.";
            return RedirectToAction("UpgradePremium", "Home");
        }

        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        goal.Status = 3;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "User";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa mục tiêu thành công!";
        return RedirectToAction(nameof(Index));
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

    [HttpPost]
    public async Task<IActionResult> AddSkillToGoals(int skillId)
    {
        var userId = GetCurrentUserId();

        var existing = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (existing != null)
        {
            if (existing.Status == "Completed" || existing.Status == "Acquired")
                return Json(new { success = false, message = "Kỹ năng này đã được hoàn thành." });

            if (existing.Status == "In-Goals" || existing.Status == "Learning")
                return Json(new { success = false, message = "Kỹ năng này đã có trong mục tiêu." });

            existing.Status = "In-Goals";
            _context.UserSkills.Update(existing);
        }
        else
        {
            _context.UserSkills.Add(new UserSkill
            {
                UserId = userId,
                SkillId = skillId,
                Status = "In-Goals",
                ProficiencyLevel = "Beginner"
            });
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, message = "Đã thêm kỹ năng vào mục tiêu học tập!" });
    }

    [HttpPost]
    public async Task<IActionResult> AddMultipleSkillsToGoals([FromBody] List<int> skillIds)
    {
        if (skillIds == null || !skillIds.Any())
            return Json(new { success = false, message = "Không tìm thấy kỹ năng được chọn." });

        var userId = GetCurrentUserId();
        int addedCount = 0;

        foreach (var skillId in skillIds)
        {
            var existing = await _context.UserSkills
                .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

            if (existing == null)
            {
                _context.UserSkills.Add(new UserSkill
                {
                    UserId = userId,
                    SkillId = skillId,
                    Status = "In-Goals",
                    ProficiencyLevel = "Beginner"
                });

                addedCount++;
            }
        }

        await _context.SaveChangesAsync();

        return Json(new
        {
            success = addedCount > 0,
            message = addedCount > 0
                ? $"Đã thêm {addedCount} kỹ năng vào mục tiêu."
                : "Tất cả kỹ năng đã tồn tại."
        });
    }

    [HttpPost]
    public async Task<IActionResult> StartLearning(int skillId)
    {
        var userId = GetCurrentUserId();

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
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Learn", new { skillId });
    }

    public async Task<IActionResult> Learn(int skillId)
    {
        var userId = GetCurrentUserId();

        var skill = await _context.Skills
            .Include(s => s.Resources)
            .FirstOrDefaultAsync(s => s.Id == skillId && s.Status == 1);

        if (skill == null) return NotFound();

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
            await _context.SaveChangesAsync();
        }

        ViewBag.UserSkill = userSkill;
        return View(skill);
    }

    public async Task<IActionResult> Test(int skillId)
    {
        var userId = GetCurrentUserId();

        var skill = await _context.Skills
            .FirstOrDefaultAsync(s => s.Id == skillId && s.Status == 1);

        if (skill == null) return NotFound();

        var userSkill = await _context.UserSkills
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (userSkill == null)
        {
            TempData["ErrorMessage"] = "Bạn cần bắt đầu học trước khi kiểm tra.";
            return RedirectToAction("Learn", new { skillId });
        }

        if (userSkill.CooldownUntil.HasValue && userSkill.CooldownUntil.Value > DateTime.Now)
        {
            ViewBag.RemainingSeconds = (int)(userSkill.CooldownUntil.Value - DateTime.Now).TotalSeconds;
            return View("Cooldown", skill);
        }

        return View(skill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitTest(int skillId, List<string> answers)
    {
        var userId = GetCurrentUserId();

        var userSkill = await _context.UserSkills
            .Include(us => us.Skill)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId);

        if (userSkill == null) return NotFound();

        int score = answers?.Count(a => a == "correct") * 20 ?? 0;

        if (score >= 80)
        {
            userSkill.Status = "Completed";
            userSkill.ProficiencyLevel = "Intermediate";
            userSkill.CooldownUntil = null;

            var activeGoals = await _context.Goals
                .Include(g => g.GoalMilestones)
                .Where(g => g.StudentId == userId && g.Status == 1)
                .ToListAsync();

            foreach (var goal in activeGoals)
            {
                foreach (var milestone in goal.GoalMilestones.Where(m => m.SkillId == skillId && m.Status != "Completed"))
                {
                    milestone.Status = "Completed";
                    milestone.UpdatedAt = DateTime.Now;
                }

                if (goal.GoalMilestones.Any())
                {
                    int completed = goal.GoalMilestones.Count(m => m.Status == "Completed");
                    goal.Progress = (int)Math.Round((double)completed / goal.GoalMilestones.Count * 100);
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Bạn đã hoàn thành kỹ năng với điểm {score}/100!";
            return RedirectToAction("Index");
        }

        userSkill.CooldownUntil = DateTime.Now.AddSeconds(120);
        await _context.SaveChangesAsync();

        TempData["ErrorMessage"] = $"Bạn đạt {score}/100. Cần tối thiểu 80/100.";
        return RedirectToAction("Learn", new { skillId });
    }

    public async Task<IActionResult> ManageCV()
    {
        var userId = GetCurrentUserId();

        ViewBag.CompletedSkills = await _context.UserSkills
            .Include(us => us.Skill)
            .Where(us => us.UserId == userId && (us.Status == "Completed" || us.Status == "Acquired"))
            .Select(us => us.Skill.Name)
            .ToListAsync();

        ViewBag.Resumes = await _context.Resumes
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.UpdatedAt ?? r.CreatedAt)
            .ToListAsync();

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCV(int resumeId, List<string> selectedSkills)
    {
        var userId = GetCurrentUserId();

        var resume = await _context.Resumes
            .FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);

        if (resume == null) return NotFound();

        selectedSkills ??= new List<string>();

        JsonNode jsonNode = string.IsNullOrEmpty(resume.ContentJson)
            ? new JsonObject()
            : JsonNode.Parse(resume.ContentJson) ?? new JsonObject();

        jsonNode["skills"] = string.Join(", ", selectedSkills);

        resume.ContentJson = jsonNode.ToJsonString();
        resume.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đồng bộ kỹ năng vào CV thành công!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCVFromSkills(List<string> selectedSkills)
    {
        var userId = GetCurrentUserId();
        var user = await _userManager.GetUserAsync(User);

        selectedSkills ??= new List<string>();

        var jsonObject = new JsonObject
        {
            ["fullName"] = user?.FullName ?? "",
            ["email"] = user?.Email ?? "",
            ["skills"] = string.Join(", ", selectedSkills),
            ["phone"] = user?.PhoneNumber ?? "",
            ["summary"] = $"Các kỹ năng đã hoàn thành: {string.Join(", ", selectedSkills)}.",
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

        return RedirectToAction("ResumeBuilder", "Home", new { id = resume.Id });
    }

    [HttpGet("/Goals/Overview")]
    [AllowAnonymous]
    public async Task<IActionResult> Overview()
    {
        var userIdValue = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userIdValue))
        {
            return RedirectToAction(nameof(Index));
        }
        return View("~/Views/Goal/Overview.cshtml");
    }

    private async Task LoadCareerPaths()
    {
        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths
                .Where(c => c.Status == 1)
                .OrderBy(c => c.Title)
                .ToListAsync(),
            "Id",
            "Title"
        );
    }
}