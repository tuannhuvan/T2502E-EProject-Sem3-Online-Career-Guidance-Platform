using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers
{
    public class CareerPathController : Controller
    {
        private readonly AppDbContext _context;

        public CareerPathController(AppDbContext context)
        {
            _context = context;
        }

        // Trang chi tiết Career Path
        public async Task<IActionResult> Details(int id)
        {
            var careerPath = await _context.CareerPaths
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == 1);

            if (careerPath == null)
            {
                return NotFound();
            }

            // Fetch Career Stages along with required Skills
            var stages = await _context.CareerStages
                .Include(cs => cs.CareerStageSkills)
                    .ThenInclude(css => css.Skill)
                .Where(cs => cs.CareerPathId == id)
                .OrderBy(cs => cs.SequenceOrder)
                .ToListAsync();

            ViewBag.Stages = stages;

            ViewBag.Jobs = await _context.JobPostings
                .Where(j => j.CareerPathId == id && j.Status == 1)
                .ToListAsync();

            return View(careerPath);
        }

        // Trang Roadmap
        public async Task<IActionResult> Roadmap(int id)
        {
            var careerPath = await _context.CareerPaths
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id && c.Status == 1);

            if (careerPath == null)
            {
                return NotFound();
            }

            var courses = await _context.CareerPathCourses
                .Where(c => c.CareerPathId == id && c.Status == 1)
                .OrderBy(c => c.SortOrder)
                .ToListAsync();

            ViewBag.Courses = courses;

            return View(careerPath);
        }
    }
}