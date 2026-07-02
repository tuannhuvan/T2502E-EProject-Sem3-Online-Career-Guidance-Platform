using System.Security.Claims;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers;

[Authorize]
public class GoalsController : Controller
{
    private readonly AppDbContext _db;

    public GoalsController(AppDbContext db)
    {
        _db = db;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<IActionResult> Index()
    {
        var userId = GetCurrentUserId();

        var goals = await _db.Goals
            .Include(g => g.CareerPath)
            .Where(g => g.StudentId == userId && g.Status != 3)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return View(goals);
    }

    public async Task<IActionResult> Details(int id)
    {
        var userId = GetCurrentUserId();

        var goal = await _db.Goals
            .Include(g => g.CareerPath)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        return View(goal);
    }

    public async Task<IActionResult> Create()
    {
        await LoadCareerPaths();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Goal goal)
    {
        var userId = GetCurrentUserId();

        goal.StudentId = userId;
        goal.CreatedAt = DateTime.Now;
        goal.CreatedBy = User.Identity?.Name ?? "User";
        goal.Status = 1;

        if (!ModelState.IsValid)
        {
            await LoadCareerPaths();
            return View(goal);
        }

        _db.Goals.Add(goal);
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();

        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        await LoadCareerPaths();
        return View(goal);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Goal input)
    {
        if (id != input.Id) return NotFound();

        var userId = GetCurrentUserId();

        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadCareerPaths();
            return View(input);
        }

        goal.Title = input.Title;
        goal.CareerPathId = input.CareerPathId;
        goal.Progress = input.Progress;
        goal.TargetDate = input.TargetDate;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "User";

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var goal = await _db.Goals
            .Include(g => g.CareerPath)
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        return View(goal);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetCurrentUserId();

        var goal = await _db.Goals
            .FirstOrDefaultAsync(g => g.Id == id && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        goal.Status = 3;
        goal.UpdatedAt = DateTime.Now;
        goal.UpdatedBy = User.Identity?.Name ?? "User";

        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCareerPaths()
    {
        ViewBag.CareerPaths = new SelectList(
            await _db.CareerPaths
                .Where(c => c.Status == 1)
                .OrderBy(c => c.Title)
                .ToListAsync(),
            "Id",
            "Title"
        );
    }
}