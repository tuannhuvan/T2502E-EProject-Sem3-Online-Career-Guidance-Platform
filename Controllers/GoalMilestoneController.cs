using System.Security.Claims;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers;

[Authorize]
public class GoalMilestoneController : Controller
{
    private readonly AppDbContext _context;

    public GoalMilestoneController(AppDbContext context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    public async Task<IActionResult> Create(int goalId)
    {
        var userId = GetCurrentUserId();

        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == goalId && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        await LoadSelectLists();

        return View(new GoalMilestone
        {
            GoalId = goalId,
            Status = "In Progress",
            SequenceOrder = 1
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GoalMilestone milestone)
    {
        var userId = GetCurrentUserId();

        var goal = await _context.Goals
            .FirstOrDefaultAsync(g => g.Id == milestone.GoalId && g.StudentId == userId && g.Status != 3);

        if (goal == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(milestone);
        }

        milestone.Status = string.IsNullOrWhiteSpace(milestone.Status)
            ? "In Progress"
            : milestone.Status;

        _context.GoalMilestones.Add(milestone);
        await _context.SaveChangesAsync();

        await UpdateGoalProgress(milestone.GoalId);

        return RedirectToAction("Index", "Goal");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var userId = GetCurrentUserId();

        var milestone = await _context.GoalMilestones
            .Include(m => m.Goal)
            .FirstOrDefaultAsync(m => m.Id == id && m.Goal!.StudentId == userId);

        if (milestone == null) return NotFound();

        await LoadSelectLists();
        return View(milestone);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, GoalMilestone input)
    {
        if (id != input.Id) return NotFound();

        var userId = GetCurrentUserId();

        var milestone = await _context.GoalMilestones
            .Include(m => m.Goal)
            .FirstOrDefaultAsync(m => m.Id == id && m.Goal!.StudentId == userId);

        if (milestone == null) return NotFound();

        if (!ModelState.IsValid)
        {
            await LoadSelectLists();
            return View(input);
        }

        milestone.Title = input.Title;
        milestone.SkillId = input.SkillId;
        milestone.ResourceId = input.ResourceId;
        milestone.SequenceOrder = input.SequenceOrder;
        milestone.Status = input.Status;
        milestone.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await UpdateGoalProgress(milestone.GoalId);

        return RedirectToAction("Index", "Goal");
    }

    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        var milestone = await _context.GoalMilestones
            .Include(m => m.Goal)
            .Include(m => m.Skill)
            .Include(m => m.Resource)
            .FirstOrDefaultAsync(m => m.Id == id && m.Goal!.StudentId == userId);

        if (milestone == null) return NotFound();

        return View(milestone);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = GetCurrentUserId();

        var milestone = await _context.GoalMilestones
            .Include(m => m.Goal)
            .FirstOrDefaultAsync(m => m.Id == id && m.Goal!.StudentId == userId);

        if (milestone == null) return NotFound();

        var goalId = milestone.GoalId;

        _context.GoalMilestones.Remove(milestone);
        await _context.SaveChangesAsync();

        await UpdateGoalProgress(goalId);

        return RedirectToAction("Index", "Goal");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = GetCurrentUserId();

        var milestone = await _context.GoalMilestones
            .Include(m => m.Goal)
            .FirstOrDefaultAsync(m => m.Id == id && m.Goal!.StudentId == userId);

        if (milestone == null) return NotFound();

        milestone.Status = "Completed";
        milestone.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        await UpdateGoalProgress(milestone.GoalId);

        return RedirectToAction("Index", "Goal");
    }

    private async Task UpdateGoalProgress(int goalId)
    {
        var goal = await _context.Goals
            .Include(g => g.GoalMilestones)
            .FirstOrDefaultAsync(g => g.Id == goalId);

        if (goal == null) return;

        var total = goal.GoalMilestones.Count;
        var completed = goal.GoalMilestones.Count(m => m.Status == "Completed");

        goal.Progress = total == 0
            ? 0
            : (int)Math.Round((double)completed / total * 100);

        goal.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    private async Task LoadSelectLists()
    {
        ViewBag.Skills = new SelectList(
            await _context.Skills
                .Where(s => s.Status == 1)
                .OrderBy(s => s.Name)
                .ToListAsync(),
            "Id",
            "Name"
        );

        ViewBag.Resources = new SelectList(
            await _context.Resources
                .Where(r => r.Status == 1)
                .OrderBy(r => r.Title)
                .ToListAsync(),
            "Id",
            "Title"
        );
    }
}