using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public AdminController(
        AppDbContext context,
        UserManager<User> userManager,
        RoleManager<IdentityRole<int>> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalMentors = await _context.MentorProfiles.CountAsync();
        ViewBag.VerifiedMentors = await _context.MentorProfiles.CountAsync(m => m.IsVerified);
        ViewBag.TotalTests = await _context.TestResults.CountAsync();
        ViewBag.TotalJobs = await _context.JobPostings.CountAsync();

        // Top Career Paths from test results
        var pathStats = await _context.TestResults
            .Include(tr => tr.RecommendedCareerPath)
            .Where(tr => tr.RecommendedCareerPathId != null)
            .GroupBy(tr => tr.RecommendedCareerPath!.Title)
            .Select(g => new { PathTitle = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync();

        ViewBag.PathLabels = pathStats.Select(x => x.PathTitle).ToList();
        ViewBag.PathCounts = pathStats.Select(x => x.Count).ToList();

        // Recent Test Results
        var recentResults = await _context.TestResults
            .Include(tr => tr.User)
            .Include(tr => tr.RecommendedCareerPath)
            .OrderByDescending(tr => tr.DateTaken)
            .Take(5)
            .ToListAsync();

        ViewBag.RecentResults = recentResults;

        return View();
    }

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserAdminDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.FullName))
        {
            return Json(new { success = false, message = "Invalid input data." });
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return Json(new { success = false, message = "Email already exists." });
        }

        var statusVal = dto.Status == "Active" ? 1 : (dto.Status == "Pending" ? 2 : 0);
        var dbRole = dto.Role == "User" ? "Student" : dto.Role;

        var user = new User
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName,
            Role = dbRole,
            Status = statusVal,
            EmailConfirmed = true
        };

        var password = string.IsNullOrWhiteSpace(dto.Password) ? "User@123456" : dto.Password;
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync(dbRole))
            {
                await _roleManager.CreateAsync(new IdentityRole<int> { Name = dbRole });
            }
            await _userManager.AddToRoleAsync(user, dbRole);
            return Json(new { success = true, message = "User created successfully." });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = $"Failed to create user: {errors}" });
    }

    [HttpPost]
    public async Task<IActionResult> EditUser([FromBody] UserAdminDto dto)
    {
        if (dto == null || dto.Id <= 0 || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.FullName))
        {
            return Json(new { success = false, message = "Invalid input data." });
        }

        var user = await _userManager.FindByIdAsync(dto.Id.ToString());
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            return Json(new { success = false, message = "Email already in use by another user." });
        }

        var oldRole = user.Role;
        var dbRole = dto.Role == "User" ? "Student" : dto.Role;
        var statusVal = dto.Status == "Active" ? 1 : (dto.Status == "Pending" ? 2 : 0);

        user.Email = dto.Email;
        user.UserName = dto.Email;
        user.FullName = dto.FullName;
        user.Role = dbRole;
        user.Status = statusVal;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            if (oldRole != dbRole)
            {
                await _userManager.RemoveFromRoleAsync(user, oldRole);
                if (!await _roleManager.RoleExistsAsync(dbRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole<int> { Name = dbRole });
                }
                await _userManager.AddToRoleAsync(user, dbRole);
            }
            return Json(new { success = true, message = "User updated successfully." });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = $"Failed to update user: {errors}" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            return Json(new { success = false, message = "User not found." });
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return Json(new { success = true, message = "User deleted successfully." });
        }

        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = $"Failed to delete user: {errors}" });
    }
    
    public async Task<IActionResult> Mentors()
    {
        var mentors = await _context.MentorProfiles
            .Include(m => m.User)
            .ToListAsync();
        return View(mentors);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveMentor(int id)
    {
        var profile = await _context.MentorProfiles.FindAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        profile.IsVerified = true;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã phê duyệt hồ sơ cố vấn thành công!";
        return RedirectToAction(nameof(Mentors));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectMentor(int id)
    {
        var profile = await _context.MentorProfiles.FindAsync(id);
        if (profile == null)
        {
            return NotFound();
        }

        profile.IsVerified = false;
        await _context.SaveChangesAsync();
        TempData["Success"] = "Đã thu hồi phê duyệt hồ sơ cố vấn!";
        return RedirectToAction(nameof(Mentors));
    }
    
    public async Task<IActionResult> CareerTests()
    {
        var questions = await _context.QuestionTests
            .Include(q => q.Test)
            .Include(q => q.QuestionOptions)
                .ThenInclude(o => o.OptionCareerPaths)
                    .ThenInclude(ocp => ocp.CareerPath)
            .OrderByDescending(q => q.Id)
            .ToListAsync();

        ViewBag.Tests = await _context.Tests
            .OrderBy(t => t.Title)
            .ToListAsync();

        ViewBag.CareerPaths = await _context.CareerPaths
            .OrderBy(cp => cp.Title)
            .ToListAsync();

        return View(questions);
    }

    [HttpPost]
    public async Task<IActionResult> SaveQuestion([FromBody] Career_Guidance_Platform.Models.ViewModels.QuestionAdminDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Content))
        {
            return Json(new { success = false, message = "Question content cannot be empty." });
        }

        var test = await _context.Tests.FindAsync(dto.TestId);
        if (test == null)
        {
            return Json(new { success = false, message = "Target Test does not exist." });
        }

        var singleChoiceType = await _context.Set<QuestionType>().FirstOrDefaultAsync(qt => qt.Name == "Single Choice");
        if (singleChoiceType == null)
        {
            singleChoiceType = new QuestionType { Name = "Single Choice", Description = "Chọn một đáp án" };
            _context.Set<QuestionType>().Add(singleChoiceType);
            await _context.SaveChangesAsync();
        }

        QuestionTest question;
        bool isNew = false;

        if (dto.Id.HasValue && dto.Id.Value > 0)
        {
            question = await _context.QuestionTests
                .Include(q => q.QuestionOptions)
                    .ThenInclude(o => o.OptionCareerPaths)
                .FirstOrDefaultAsync(q => q.Id == dto.Id.Value);

            if (question == null)
            {
                return Json(new { success = false, message = "Question not found for editing." });
            }

            question.UpdatedAt = DateTime.Now;
            question.UpdatedBy = User.Identity?.Name ?? "Admin";
        }
        else
        {
            question = new QuestionTest
            {
                CreatedAt = DateTime.Now,
                CreatedBy = User.Identity?.Name ?? "Admin"
            };
            isNew = true;
        }

        question.TestId = dto.TestId;
        question.QuestionTypeId = singleChoiceType.Id;
        question.Content = dto.Content;
        question.TestType = dto.TestType;
        question.Status = dto.Status == "Active" ? 1 : 0;

        if (isNew)
        {
            _context.QuestionTests.Add(question);
        }

        await _context.SaveChangesAsync();

        // Update options
        if (!isNew)
        {
            foreach (var opt in question.QuestionOptions)
            {
                _context.Set<OptionCareerPath>().RemoveRange(opt.OptionCareerPaths);
            }
            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
            await _context.SaveChangesAsync();
        }

        if (dto.Options != null)
        {
            foreach (var optDto in dto.Options)
            {
                if (string.IsNullOrWhiteSpace(optDto.Content)) continue;

                var option = new QuestionOption
                {
                    QuestionId = question.Id,
                    Content = optDto.Content,
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Admin",
                    Status = 1
                };

                _context.QuestionOptions.Add(option);
                await _context.SaveChangesAsync();

                if (optDto.CareerPathId.HasValue && optDto.CareerPathId.Value > 0)
                {
                    var optPath = new OptionCareerPath
                    {
                        OptionId = option.Id,
                        CareerPathId = optDto.CareerPathId.Value,
                        Weight = optDto.Weight,
                        CreatedAt = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "Admin",
                        Status = 1
                    };
                    _context.Set<OptionCareerPath>().Add(optPath);
                }
            }
            await _context.SaveChangesAsync();
        }

        return Json(new { success = true, message = "Question saved successfully!" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await _context.QuestionTests
            .Include(q => q.QuestionOptions)
                .ThenInclude(o => o.OptionCareerPaths)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (question == null)
        {
            return Json(new { success = false, message = "Question not found." });
        }

        var hasAnswers = await _context.TestAnswers.AnyAsync(ta => ta.QuestionId == id);
        if (hasAnswers)
        {
            return Json(new { success = false, message = "Cannot delete this question. Users have already answered it." });
        }

        foreach (var opt in question.QuestionOptions)
        {
            _context.Set<OptionCareerPath>().RemoveRange(opt.OptionCareerPaths);
        }
        _context.QuestionOptions.RemoveRange(question.QuestionOptions);
        _context.QuestionTests.Remove(question);

        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Question deleted successfully!" });
    }
    public async Task<IActionResult> Jobs()
    {
        var jobs = await _context.JobPostings.Include(j => j.CareerPath).ToListAsync();
        return View(jobs);
    }

    public async Task<IActionResult> CreateJob()
    {
        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(cp => cp.Status == 1).OrderBy(cp => cp.Title).ToListAsync(),
            "Id",
            "Title"
        );
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateJob(JobPosting job)
    {
        if (ModelState.IsValid)
        {
            job.CreatedAt = DateTime.Now;
            job.CreatedBy = User.Identity?.Name ?? "Admin";
            job.Status = 1;
            _context.JobPostings.Add(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job created successfully.";
            return RedirectToAction(nameof(Jobs));
        }

        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(cp => cp.Status == 1).OrderBy(cp => cp.Title).ToListAsync(),
            "Id",
            "Title",
            job.CareerPathId
        );
        return View(job);
    }

    public async Task<IActionResult> EditJob(int id)
    {
        var job = await _context.JobPostings.FindAsync(id);
        if (job == null)
        {
            return NotFound();
        }

        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(cp => cp.Status == 1).OrderBy(cp => cp.Title).ToListAsync(),
            "Id",
            "Title",
            job.CareerPathId
        );
        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditJob(int id, JobPosting job)
    {
        if (id != job.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingJob = await _context.JobPostings.FindAsync(id);
                if (existingJob == null)
                {
                    return NotFound();
                }

                existingJob.Title = job.Title;
                existingJob.CompanyName = job.CompanyName;
                existingJob.JobType = job.JobType;
                existingJob.Location = job.Location;
                existingJob.ExperienceLevel = job.ExperienceLevel;
                existingJob.ApplicationUrl = job.ApplicationUrl;
                existingJob.Salary = job.Salary;
                existingJob.Description = job.Description;
                existingJob.ExpiredAt = job.ExpiredAt;
                existingJob.CareerPathId = job.CareerPathId;
                existingJob.Status = job.Status;
                existingJob.UpdatedAt = DateTime.Now;
                existingJob.UpdatedBy = User.Identity?.Name ?? "Admin";

                _context.JobPostings.Update(existingJob);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Job updated successfully.";
                return RedirectToAction(nameof(Jobs));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating job: {ex.Message}");
            }
        }

        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(cp => cp.Status == 1).OrderBy(cp => cp.Title).ToListAsync(),
            "Id",
            "Title",
            job.CareerPathId
        );
        return View(job);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.JobPostings.FindAsync(id);
        if (job != null)
        {
            _context.JobPostings.Remove(job);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Job deleted successfully.";
        }
        else
        {
            TempData["Error"] = "Job not found.";
        }
        return RedirectToAction(nameof(Jobs));
    }
    public IActionResult Reports() => View();
    public IActionResult Settings() => View();
    public IActionResult Terms() => View();

    // GET: Admin/TestDetails/5
    public async Task<IActionResult> TestDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var test = await _context.Tests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (test == null)
        {
            return NotFound();
        }

        return View(test);
    }

    // GET: Admin/CreateTest
    public IActionResult CreateTest()
    {
        return View();
    }

    // POST: Admin/CreateTest
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTest([Bind("Id,Title")] Test test)
    {
        if (ModelState.IsValid)
        {
            test.CreatedAt = System.DateTime.Now;
            test.CreatedBy = User.Identity?.Name ?? "Admin";
            _context.Add(test);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(CareerTests));
        }
        return View(test);
    }

    // GET: Admin/EditTest/5
    public async Task<IActionResult> EditTest(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var test = await _context.Tests.FindAsync(id);
        if (test == null)
        {
            return NotFound();
        }
        return View(test);
    }

    // POST: Admin/EditTest/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditTest(int id, [Bind("Id,Title")] Test test)
    {
        if (id != test.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                test.UpdatedAt = System.DateTime.Now;
                test.UpdatedBy = User.Identity?.Name ?? "Admin";
                _context.Update(test);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tests.Any(e => e.Id == test.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(CareerTests));
        }
        return View(test);
    }

    // GET: Admin/DeleteTest/5
    public async Task<IActionResult> DeleteTest(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var test = await _context.Tests
            .FirstOrDefaultAsync(m => m.Id == id);
        if (test == null)
        {
            return NotFound();
        }

        return View(test);
    }

    // POST: Admin/DeleteTest/5
    [HttpPost, ActionName("DeleteTest")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTestConfirmed(int id)
    {
        var test = await _context.Tests.FindAsync(id);
        if (test != null)
        {
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(CareerTests));
    }

    // GET: Admin/Categories
    public async Task<IActionResult> Categories()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }

    // GET: Admin/CreateCategory
    public IActionResult CreateCategory()
    {
        return View();
    }

    // POST: Admin/CreateCategory
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCategory([Bind("Id,Name,Description,Status")] Category category)
    {
        if (ModelState.IsValid)
        {
            category.CreatedAt = System.DateTime.Now;
            category.CreatedBy = User.Identity?.Name ?? "Admin";
            _context.Add(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Categories));
        }
        return View(category);
    }

    // GET: Admin/EditCategory/5
    public async Task<IActionResult> EditCategory(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }
        return View(category);
    }

    // POST: Admin/EditCategory/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCategory(int id, [Bind("Id,Name,Description,Status")] Category category)
    {
        if (id != category.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                category.UpdatedAt = System.DateTime.Now;
                category.UpdatedBy = User.Identity?.Name ?? "Admin";
                _context.Update(category);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(e => e.Id == category.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Categories));
        }
        return View(category);
    }

    // GET: Admin/DeleteCategory/5
    public async Task<IActionResult> DeleteCategory(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = await _context.Categories
            .FirstOrDefaultAsync(m => m.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Admin/DeleteCategory/5
    [HttpPost, ActionName("DeleteCategory")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategoryConfirmed(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Categories));
    }
    
        // GET: /Admin/Courses
    public async Task<IActionResult> Courses()
    {
        var courses = await _context.CareerPathCourses
            .Include(c => c.CareerPath)
            .OrderBy(c => c.CareerPathId)
            .ThenBy(c => c.SortOrder)
            .ToListAsync();

        return View(courses);
    }

    // GET: /Admin/CreateCourse
    public async Task<IActionResult> CreateCourse()
    {
        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Title"
        );

        return View();
    }

    // POST: /Admin/CreateCourse
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CareerPathCourse course)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CareerPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Title",
                course.CareerPathId
            );

            return View(course);
        }

        course.Status = 1;
        _context.CareerPathCourses.Add(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Courses));
    }

    // GET: /Admin/EditCourse/1
    public async Task<IActionResult> EditCourse(int id)
    {
        var course = await _context.CareerPathCourses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Title",
            course.CareerPathId
        );

        return View(course);
    }

    // POST: /Admin/EditCourse/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, CareerPathCourse course)
    {
        if (id != course.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.CareerPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Title",
                course.CareerPathId
            );

            return View(course);
        }

        _context.CareerPathCourses.Update(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Courses));
    }

    // POST: /Admin/DeleteCourse/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var course = await _context.CareerPathCourses.FindAsync(id);

        if (course == null)
        {
            return NotFound();
        }

        _context.CareerPathCourses.Remove(course);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Courses));
    }
    
    //CRUD CAREPATH
    public async Task<IActionResult> CareerPaths()
    {
        var careerPaths = await _context.CareerPaths
            .Include(c => c.Category)
            .OrderByDescending(c => c.Id)
            .ToListAsync();

        // Pass statistics to display on the index view (e.g. stage count, skill count)
        ViewBag.StageCounts = await _context.CareerStages
            .GroupBy(cs => cs.CareerPathId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        ViewBag.SkillCounts = await _context.CareerPathSkills
            .GroupBy(cps => cps.CareerPathId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return View(careerPaths);
    }

    public async Task<IActionResult> CreateCareerPath()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Name"
        );

        ViewBag.ParentPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Title"
        );

        ViewBag.Skills = await _context.Skills
            .Where(s => s.Status == 1)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(new CareerPath { Status = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCareerPath(CareerPath careerPath)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Name",
                careerPath.CategoryId
            );

            ViewBag.ParentPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Title",
                careerPath.ParentPathId
            );

            ViewBag.Skills = await _context.Skills
                .Where(s => s.Status == 1)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(careerPath);
        }

        careerPath.Status = 1;
        careerPath.CreatedAt = DateTime.Now;
        careerPath.CreatedBy = User.Identity?.Name ?? "Admin";

        _context.CareerPaths.Add(careerPath);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CareerPaths));
    }

    public async Task<IActionResult> EditCareerPath(int id)
    {
        var careerPath = await _context.CareerPaths
            .Include(cp => cp.CareerPathSkills)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        if (careerPath == null)
        {
            return NotFound();
        }

        var stages = await _context.CareerStages
            .Include(cs => cs.CareerStageSkills)
                .ThenInclude(css => css.Skill)
            .Where(cs => cs.CareerPathId == id)
            .OrderBy(cs => cs.SequenceOrder)
            .ToListAsync();

        ViewBag.Stages = stages;

        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Name",
            careerPath.CategoryId
        );

        ViewBag.ParentPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1 && c.Id != id).ToListAsync(),
            "Id",
            "Title",
            careerPath.ParentPathId
        );

        ViewBag.Skills = await _context.Skills
            .Where(s => s.Status == 1)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(careerPath);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCareerPath(int id, CareerPath careerPath)
    {
        if (id != careerPath.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Name",
                careerPath.CategoryId
            );

            ViewBag.ParentPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1 && c.Id != id).ToListAsync(),
                "Id",
                "Title",
                careerPath.ParentPathId
            );

            ViewBag.Skills = await _context.Skills
                .Where(s => s.Status == 1)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(careerPath);
        }

        careerPath.UpdatedAt = DateTime.Now;
        careerPath.UpdatedBy = User.Identity?.Name ?? "Admin";

        _context.CareerPaths.Update(careerPath);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CareerPaths));
    }

    [HttpPost]
    [Route("Admin/SaveCareerPath")]
    public async Task<IActionResult> SaveCareerPath([FromBody] SaveCareerPathDto dto)
    {
        if (dto == null)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
        {
            return Json(new { success = false, message = "Tên Lộ trình không được để trống." });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            CareerPath? careerPath;
            if (dto.Id > 0)
            {
                careerPath = await _context.CareerPaths.FindAsync(dto.Id);
                if (careerPath == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy Lộ trình nghề nghiệp." });
                }
            }
            else
            {
                careerPath = new CareerPath
                {
                    CreatedAt = DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Admin"
                };
                _context.CareerPaths.Add(careerPath);
            }

            // Update basic fields
            careerPath.Title = dto.Title;
            careerPath.CategoryId = dto.CategoryId;
            careerPath.ParentPathId = dto.ParentPathId;
            careerPath.Content = dto.Content;
            careerPath.SalaryMin = dto.SalaryMin;
            careerPath.SalaryMax = dto.SalaryMax;
            careerPath.JobOutlook = dto.JobOutlook;
            careerPath.Status = dto.Status;
            careerPath.UpdatedAt = DateTime.Now;
            careerPath.UpdatedBy = User.Identity?.Name ?? "Admin";

            // Save first if new
            if (dto.Id == 0)
            {
                await _context.SaveChangesAsync();
            }

            // Update general CareerPathSkills
            var existingPathSkills = await _context.CareerPathSkills
                .Where(cps => cps.CareerPathId == careerPath.Id)
                .ToListAsync();
            _context.CareerPathSkills.RemoveRange(existingPathSkills);

            if (dto.Skills != null)
            {
                foreach (var sk in dto.Skills)
                {
                    _context.CareerPathSkills.Add(new CareerPathSkill
                    {
                        CareerPathId = careerPath.Id,
                        SkillId = sk.SkillId,
                        ImportanceLevel = sk.ImportanceLevel
                    });
                }
            }

            // Update Stages
            var existingStages = await _context.CareerStages
                .Include(cs => cs.CareerStageSkills)
                .Where(cs => cs.CareerPathId == careerPath.Id)
                .ToListAsync();

            var dtoStageIds = dto.Stages.Where(s => s.Id > 0).Select(s => s.Id).ToList();
            var stagesToRemove = existingStages.Where(s => !dtoStageIds.Contains(s.Id)).ToList();

            foreach (var stToRemove in stagesToRemove)
            {
                _context.CareerStageSkills.RemoveRange(stToRemove.CareerStageSkills);
                _context.CareerStages.Remove(stToRemove);
            }

            if (dto.Stages != null)
            {
                foreach (var stageDto in dto.Stages)
                {
                    CareerStage? stage;
                    if (stageDto.Id > 0)
                    {
                        stage = existingStages.FirstOrDefault(s => s.Id == stageDto.Id);
                        if (stage == null)
                        {
                            stage = new CareerStage { CareerPathId = careerPath.Id };
                            _context.CareerStages.Add(stage);
                        }
                    }
                    else
                    {
                        stage = new CareerStage { CareerPathId = careerPath.Id };
                        _context.CareerStages.Add(stage);
                    }

                    stage.Title = stageDto.Title;
                    stage.Description = stageDto.Description;
                    stage.SequenceOrder = stageDto.SequenceOrder;

                    // Save stage to get Id if new
                    if (stage.Id == 0)
                    {
                        await _context.SaveChangesAsync();
                    }

                    // Update stage skills
                    var existingStageSkills = await _context.CareerStageSkills
                        .Where(css => css.CareerStageId == stage.Id)
                        .ToListAsync();
                    _context.CareerStageSkills.RemoveRange(existingStageSkills);

                    if (stageDto.Skills != null)
                    {
                        foreach (var skillDto in stageDto.Skills)
                        {
                            _context.CareerStageSkills.Add(new CareerStageSkill
                            {
                                CareerStageId = stage.Id,
                                SkillId = skillDto.SkillId,
                                ProficiencyRequired = skillDto.ProficiencyRequired
                            });
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Json(new { success = true, message = "Lưu lộ trình nghề nghiệp thành công!" });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCareerPath(int id)
    {
        var careerPath = await _context.CareerPaths.FindAsync(id);

        if (careerPath == null)
        {
            return NotFound();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Delete OptionCareerPaths
            var optionCareerPaths = await _context.OptionCareerPaths
                .Where(o => o.CareerPathId == id)
                .ToListAsync();
            _context.OptionCareerPaths.RemoveRange(optionCareerPaths);

            // 2. Delete TestResultScores
            var testResultScores = await _context.TestResultScores
                .Where(s => s.CareerPathId == id)
                .ToListAsync();
            _context.TestResultScores.RemoveRange(testResultScores);

            // 3. Delete SuccessStories
            var successStories = await _context.SuccessStories
                .Where(s => s.CareerPathId == id)
                .ToListAsync();
            _context.SuccessStories.RemoveRange(successStories);

            // 4. Delete CareerPathCourses
            var courses = await _context.CareerPathCourses
                .Where(c => c.CareerPathId == id)
                .ToListAsync();
            _context.CareerPathCourses.RemoveRange(courses);

            // 5. Delete CareerPathSkills
            var pathSkills = await _context.CareerPathSkills
                .Where(ps => ps.CareerPathId == id)
                .ToListAsync();
            _context.CareerPathSkills.RemoveRange(pathSkills);

            // 6. Delete CareerStages and their skills
            var stages = await _context.CareerStages
                .Include(cs => cs.CareerStageSkills)
                .Where(cs => cs.CareerPathId == id)
                .ToListAsync();
            foreach (var stage in stages)
            {
                _context.CareerStageSkills.RemoveRange(stage.CareerStageSkills);
            }
            _context.CareerStages.RemoveRange(stages);

            // 7. Nullify references in Goals
            var goals = await _context.Goals
                .Where(g => g.CareerPathId == id)
                .ToListAsync();
            foreach (var goal in goals)
            {
                goal.CareerPathId = null;
            }

            // 8. Nullify references in JobPostings
            var jobs = await _context.JobPostings
                .Where(j => j.CareerPathId == id)
                .ToListAsync();
            foreach (var job in jobs)
            {
                job.CareerPathId = null;
            }

            // 9. Nullify references in TestResults
            var testResults = await _context.TestResults
                .Where(t => t.RecommendedCareerPathId == id)
                .ToListAsync();
            foreach (var tr in testResults)
            {
                tr.RecommendedCareerPathId = null;
            }

            // 10. Nullify ParentPathId in child CareerPaths
            var childPaths = await _context.CareerPaths
                .Where(c => c.ParentPathId == id)
                .ToListAsync();
            foreach (var child in childPaths)
            {
                child.ParentPathId = null;
            }

            // Remove CareerPath
            _context.CareerPaths.Remove(careerPath);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đã xóa lộ trình nghề nghiệp thành công!";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi khi xóa lộ trình nghề nghiệp: " + ex.Message;
        }

        return RedirectToAction(nameof(CareerPaths));
    }
    
    // ==========================================
// CRUD RESOURCES with Parent-Child Support
// ==========================================

public async Task<IActionResult> Resources(int? categoryId, int? skillId, string? search)
{
    var query = _context.Resources
        .Include(r => r.Category)
        .Include(r => r.ParentResource)
        .Include(r => r.ChildResources)
        .Include(r => r.Skill)
        .AsQueryable();

    if (categoryId.HasValue)
    {
        query = query.Where(r => r.CategoryId == categoryId);
    }

    if (skillId.HasValue)
    {
        query = query.Where(r => r.SkillId == skillId);
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        var cleanSearch = search.Trim().ToLower();
        query = query.Where(r => (r.Title != null && r.Title.ToLower().Contains(cleanSearch)) || 
                                 (r.Description != null && r.Description.ToLower().Contains(cleanSearch)));
    }

    var resources = await query
        .Where(r => r.Status == 1)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    ViewBag.Categories = await _context.Categories
        .Where(c => c.Status == 1)
        .OrderBy(c => c.Name)
        .ToListAsync();

    ViewBag.Skills = await _context.Skills
        .Where(s => s.Status == 1)
        .OrderBy(s => s.Name)
        .ToListAsync();
    
    ViewBag.SelectedCategoryId = categoryId;
    ViewBag.SelectedSkillId = skillId;
    ViewBag.Search = search;

    return View(resources);
}

public async Task<IActionResult> CreateResource()
{
    await LoadResourceDropdownData();
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateResource(Resource resource)
{
    ValidateResourceModel(resource);

    if (!ModelState.IsValid)
    {
        await LoadResourceDropdownData();
        return View(resource);
    }

    try
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Admin";
        resource.CreatedBy = userId;
        resource.CreatedAt = DateTime.Now;
        resource.Status = 1;
        resource.PathId = resource.PathId == 0 ? 0 : resource.PathId;

        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Lưu resource thành công!";
        return RedirectToAction(nameof(Resources));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"Lỗi khi lưu resource: {ex.Message}");
        await LoadResourceDropdownData();
        return View(resource);
    }
}

public async Task<IActionResult> EditResource(int id)
{
    var resource = await _context.Resources.FindAsync(id);

    if (resource == null)
    {
        return NotFound();
    }

    await LoadResourceDropdownData(resource.Id);
    return View(resource);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> EditResource(int id, Resource resource)
{
    if (id != resource.Id)
    {
        return NotFound();
    }

    ValidateResourceModel(resource);

    if (!ModelState.IsValid)
    {
        await LoadResourceDropdownData(resource.Id);
        return View(resource);
    }

    try
    {
        var existingResource = await _context.Resources.FindAsync(id);
        if (existingResource == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.Identity?.Name ?? "Admin";

        existingResource.Title = resource.Title;
        existingResource.Description = resource.Description;
        existingResource.ResourceType = resource.ResourceType;
        existingResource.Url = resource.Url;
        existingResource.CategoryId = resource.CategoryId;
        existingResource.ParentResourceId = resource.ParentResourceId;
        existingResource.PathId = resource.PathId == 0 ? 0 : resource.PathId;
        existingResource.SkillId = resource.SkillId;
        existingResource.UpdatedAt = DateTime.Now;
        existingResource.UpdatedBy = userId;

        _context.Resources.Update(existingResource);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật resource thành công!";
        return RedirectToAction(nameof(Resources));
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"Lỗi khi cập nhật resource: {ex.Message}");
        await LoadResourceDropdownData(resource.Id);
        return View(resource);
    }
}

public async Task<IActionResult> DeleteResource(int id)
{
    var resource = await _context.Resources
        .Include(r => r.Category)
        .Include(r => r.ParentResource)
        .Include(r => r.ChildResources)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (resource == null)
    {
        return NotFound();
    }

    return View(resource);
}

[HttpPost]
[ValidateAntiForgeryToken]
[ActionName("DeleteResource")]
public async Task<IActionResult> DeleteResourceConfirmed(int id)
{
    var resource = await _context.Resources
        .Include(r => r.ChildResources)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (resource == null)
    {
        return NotFound();
    }

    if (resource.ChildResources.Any())
    {
        TempData["ErrorMessage"] = "Không thể xóa resource này vì nó có các resource con liên kết. Vui lòng xóa các resource con trước.";
        return RedirectToAction(nameof(DeleteResource), new { id = id });
    }

    try
    {
        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa resource thành công!";
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = $"Lỗi khi xóa resource: {ex.Message}";
    }

    return RedirectToAction(nameof(Resources));
}

private void ValidateResourceModel(Resource resource)
{
    if (string.IsNullOrWhiteSpace(resource.Title))
    {
        ModelState.AddModelError(nameof(resource.Title), "Tên resource là bắt buộc");
    }

    if (string.IsNullOrWhiteSpace(resource.ResourceType))
    {
        ModelState.AddModelError(nameof(resource.ResourceType), "Loại resource là bắt buộc");
    }

    if (!string.IsNullOrWhiteSpace(resource.Url) && !Uri.TryCreate(resource.Url, UriKind.Absolute, out _))
    {
        ModelState.AddModelError(nameof(resource.Url), "URL không hợp lệ");
    }
}

private async Task LoadResourceDropdownData(int? currentResourceId = null)
{
    ViewBag.Categories = new SelectList(
        await _context.Categories
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Name)
            .ToListAsync(),
        "Id",
        "Name"
    );

    ViewBag.CareerPaths = new SelectList(
        await _context.CareerPaths
            .Where(c => c.Status == 1)
            .OrderBy(c => c.Title)
            .ToListAsync(),
        "Id",
        "Title"
    );

    ViewBag.Skills = new SelectList(
        await _context.Skills
            .Where(s => s.Status == 1)
            .OrderBy(s => s.Name)
            .ToListAsync(),
        "Id",
        "Name"
    );

    var parentResources = await _context.Resources
        .Where(r => r.Status == 1 && (currentResourceId == null || r.Id != currentResourceId))
        .OrderBy(r => r.Title)
        .ToListAsync();

    ViewBag.ParentResources = new SelectList(parentResources, "Id", "Title");
}

    // ==========================================
    // CRUD SKILLS (MANAGEMENT)
    // ==========================================

    public async Task<IActionResult> Skills()
    {
        var skills = await _context.Skills
            .Include(s => s.Resources)
            .OrderByDescending(s => s.Id)
            .ToListAsync();

        ViewBag.PathCounts = await _context.CareerPathSkills
            .GroupBy(cps => cps.SkillId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        ViewBag.StageCounts = await _context.CareerStageSkills
            .GroupBy(css => css.SkillId)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return View(skills);
    }

    public async Task<IActionResult> CreateSkill()
    {
        ViewBag.Resources = await _context.Resources
            .Where(r => r.Status == 1)
            .OrderBy(r => r.Title)
            .ToListAsync();
        return View(new Skill { Status = 1, EstimatedHours = 10, Difficulty = "Medium", SkillType = "Hard Skill" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSkill(Skill skill, int[]? selectedResourceIds)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Resources = await _context.Resources
                .Where(r => r.Status == 1)
                .OrderBy(r => r.Title)
                .ToListAsync();
            return View(skill);
        }

        skill.CreatedAt = DateTime.Now;
        skill.CreatedBy = User.Identity?.Name ?? "Admin";

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();

        if (selectedResourceIds != null && selectedResourceIds.Length > 0)
        {
            var resourcesToUpdate = await _context.Resources
                .Where(r => selectedResourceIds.Contains(r.Id))
                .ToListAsync();
            foreach (var res in resourcesToUpdate)
            {
                res.SkillId = skill.Id;
            }
            await _context.SaveChangesAsync();
        }

        TempData["Success"] = "Thêm kỹ năng thành công!";
        return RedirectToAction(nameof(Skills));
    }

    public async Task<IActionResult> EditSkill(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
        {
            return NotFound();
        }
        ViewBag.Resources = await _context.Resources
            .Where(r => r.Status == 1)
            .OrderBy(r => r.Title)
            .ToListAsync();
        ViewBag.CurrentResourceIds = await _context.Resources
            .Where(r => r.SkillId == id)
            .Select(r => r.Id)
            .ToListAsync();
        return View(skill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditSkill(int id, Skill skill, int[]? selectedResourceIds)
    {
        if (id != skill.Id)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Resources = await _context.Resources
                .Where(r => r.Status == 1)
                .OrderBy(r => r.Title)
                .ToListAsync();
            ViewBag.CurrentResourceIds = await _context.Resources
                .Where(r => r.SkillId == id)
                .Select(r => r.Id)
                .ToListAsync();
            return View(skill);
        }

        try
        {
            var existingSkill = await _context.Skills.FindAsync(id);
            if (existingSkill == null)
            {
                return NotFound();
            }

            existingSkill.Name = skill.Name;
            existingSkill.Description = skill.Description;
            existingSkill.SkillType = skill.SkillType;
            existingSkill.Difficulty = skill.Difficulty;
            existingSkill.EstimatedHours = skill.EstimatedHours;
            existingSkill.Status = skill.Status;
            existingSkill.UpdatedAt = DateTime.Now;
            existingSkill.UpdatedBy = User.Identity?.Name ?? "Admin";

            _context.Skills.Update(existingSkill);
            await _context.SaveChangesAsync();

            // Handle resource association updates
            var currentAssociated = await _context.Resources.Where(r => r.SkillId == id).ToListAsync();
            foreach (var res in currentAssociated)
            {
                if (selectedResourceIds == null || !selectedResourceIds.Contains(res.Id))
                {
                    res.SkillId = null;
                }
            }

            if (selectedResourceIds != null && selectedResourceIds.Length > 0)
            {
                var targetResources = await _context.Resources.Where(r => selectedResourceIds.Contains(r.Id)).ToListAsync();
                foreach (var res in targetResources)
                {
                    res.SkillId = id;
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Cập nhật kỹ năng thành công!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "Lỗi khi cập nhật kỹ năng: " + ex.Message;
        }

        return RedirectToAction(nameof(Skills));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
        {
            return NotFound();
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Delete from CareerPathSkills
            var pathSkills = await _context.CareerPathSkills.Where(cps => cps.SkillId == id).ToListAsync();
            _context.CareerPathSkills.RemoveRange(pathSkills);

            // 2. Delete from CareerStageSkills
            var stageSkills = await _context.CareerStageSkills.Where(css => css.SkillId == id).ToListAsync();
            _context.CareerStageSkills.RemoveRange(stageSkills);

            // 3. Delete from UserSkills
            var userSkills = await _context.UserSkills.Where(us => us.SkillId == id).ToListAsync();
            _context.UserSkills.RemoveRange(userSkills);

            // 4. Nullify SkillId in Resources
            var resources = await _context.Resources.Where(r => r.SkillId == id).ToListAsync();
            foreach (var res in resources)
            {
                res.SkillId = null;
            }

            // 5. Delete Skill
            _context.Skills.Remove(skill);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            TempData["Success"] = "Đã xóa kỹ năng thành công!";
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            TempData["Error"] = "Lỗi khi xóa kỹ năng: " + ex.Message;
        }

        return RedirectToAction(nameof(Skills));
    }
    // GET: /Admin/Goals
    public async Task<IActionResult> Goals(string? search, string? type, int? status)
    {
        var query = _context.Goals
            .Include(g => g.Student)
            .Include(g => g.CareerPath)
            .Include(g => g.GoalMilestones)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(g =>
                g.Title.Contains(search) ||
                g.Student!.FullName.Contains(search) ||
                g.Student.Email!.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(g => g.GoalType == type);
        }

        if (status.HasValue)
        {
            query = query.Where(g => g.Status == status.Value);
        }
        else
        {
            query = query.Where(g => g.Status != 3);
        }

        var goals = await query
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        ViewBag.Search = search;
        ViewBag.Type = type;
        ViewBag.Status = status;

        return View(goals);
    }

    // GET: /Admin/GoalDetails/5
    public async Task<IActionResult> GoalDetails(int id)
    {
        var goal = await _context.Goals
            .Include(g => g.Student)
            .Include(g => g.CareerPath)
            .Include(g => g.GoalMilestones)
                .ThenInclude(m => m.Skill)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (goal == null) return NotFound();

        return View(goal);
    }

    // GET: /Admin/EditGoal/5
    public async Task<IActionResult> EditGoal(int id)
    {
        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == id);

        if (goal == null) return NotFound();

        await LoadAdminGoalData();
        return View(goal);
    }

    // POST: /Admin/EditGoal/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditGoal(int id, Goal input)
    {
        if (id != input.Id) return NotFound();

        var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id);

        if (goal == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadAdminGoalData();
            return View(input);
        }

        goal.Title = input.Title;
        goal.GoalType = input.GoalType;
        goal.CareerPathId = input.CareerPathId;
        goal.Progress = input.Progress;
        goal.TargetDate = input.TargetDate;
        goal.Status = input.Status;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Cập nhật Goal thành công!";
        return RedirectToAction(nameof(Goals));
    }

    // GET: /Admin/DeleteGoal/5
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var goal = await _context.Goals
            .Include(g => g.Student)
            .Include(g => g.CareerPath)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (goal == null) return NotFound();

        return View(goal);
    }

    // POST: /Admin/DeleteGoal/5
    [HttpPost, ActionName("DeleteGoal")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGoalConfirmed(int id)
    {
        var goal = await _context.Goals.FirstOrDefaultAsync(g => g.Id == id);

        if (goal == null) return NotFound();

        goal.Status = 3;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "Admin";

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Xóa Goal thành công!";
        return RedirectToAction(nameof(Goals));
    }

    private async Task LoadAdminGoalData()
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
