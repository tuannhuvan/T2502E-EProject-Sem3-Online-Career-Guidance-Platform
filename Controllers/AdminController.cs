using System;
using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Career_Guidance_Platform.Models.ViewModels;


namespace Career_Guidance_Platform.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Dashboard");
    }
    public IActionResult Dashboard() => View();
    public IActionResult Users() => View();
    
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
        var tests = await _context.Tests.ToListAsync();
        return View(tests);
    }
    public IActionResult Jobs() => View();
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
    
    // crud resources
    public async Task<IActionResult> Resources()
    {
        var resources = await _context.Resources
            .Include(r => r.CareerPath)
            .OrderByDescending(r => r.Id)
            .ToListAsync();

        return View(resources);
    }

    public async Task<IActionResult> CreateResource()
    {
        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Title"
        );

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateResource(Resource resource)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CareerPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Title",
                resource.PathId
            );

            return View(resource);
        }

        resource.Status = 1;
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Resources));
    }

    public async Task<IActionResult> EditResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        ViewBag.CareerPaths = new SelectList(
            await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Title",
            resource.PathId
        );

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

        if (!ModelState.IsValid)
        {
            ViewBag.CareerPaths = new SelectList(
                await _context.CareerPaths.Where(c => c.Status == 1).ToListAsync(),
                "Id",
                "Title",
                resource.PathId
            );

            return View(resource);
        }

        _context.Resources.Update(resource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Resources));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteResource(int id)
    {
        var resource = await _context.Resources.FindAsync(id);

        if (resource == null)
        {
            return NotFound();
        }

        _context.Resources.Remove(resource);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Resources));
    }
}
