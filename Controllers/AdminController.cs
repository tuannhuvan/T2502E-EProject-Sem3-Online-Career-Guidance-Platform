using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

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
    public IActionResult Mentors() => View();
    
    public async Task<IActionResult> CareerTests()
    {
        var tests = await _context.Tests.ToListAsync();
        return View(tests);
    }
    public IActionResult Jobs() => View();
    public IActionResult Resources() => View();
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
}
