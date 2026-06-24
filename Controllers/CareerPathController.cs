using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Career_Guidance_Platform.Controllers
{
    public class CareerPathController : Controller
    {
        private readonly AppDbContext _context;

        public CareerPathController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var careerPath = await _context.CareerPaths
                .Include(cp => cp.Category)
                .FirstOrDefaultAsync(cp => cp.Id == id);

            if (careerPath == null)
            {
                return NotFound("Không tìm thấy lộ trình nghề nghiệp yêu cầu.");
            }

            // Retrieve partner jobs / businesses
            ViewBag.Jobs = await _context.Set<JobPosting>()
                .Where(jp => jp.CareerPathId == id && jp.Status == 1)
                .ToListAsync();

            return View(careerPath);
        }
    }
}
