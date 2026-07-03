using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;


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

        return View(careerPaths);
    }

    public async Task<IActionResult> CreateCareerPath()
    {
        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Name"
        );

        return View();
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

            return View(careerPath);
        }

        careerPath.Status = 1;
        careerPath.CreatedAt = DateTime.Now;
        careerPath.CreatedBy = "Admin";

        _context.CareerPaths.Add(careerPath);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CareerPaths));
    }

    public async Task<IActionResult> EditCareerPath(int id)
    {
        var careerPath = await _context.CareerPaths.FindAsync(id);

        if (careerPath == null)
        {
            return NotFound();
        }

        ViewBag.Categories = new SelectList(
            await _context.Categories.Where(c => c.Status == 1).ToListAsync(),
            "Id",
            "Name",
            careerPath.CategoryId
        );

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

            return View(careerPath);
        }

        careerPath.UpdatedAt = DateTime.Now;
        careerPath.UpdatedBy = "Admin";

        _context.CareerPaths.Update(careerPath);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(CareerPaths));
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

        _context.CareerPaths.Remove(careerPath);
        await _context.SaveChangesAsync();

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
