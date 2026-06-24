using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace Career_Guidance_Platform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> CareerTest()
    {
        var test = await _context.Tests
            .Include(t => t.QuestionTests)
                .ThenInclude(qt => qt.QuestionOptions)
            .Include(t => t.QuestionTests)
                .ThenInclude(qt => qt.QuestionType)
            .FirstOrDefaultAsync(t => t.Status == 1);

        if (test == null)
        {
            return NotFound("Không tìm thấy bài đánh giá nghề nghiệp nào đang hoạt động.");
        }

        var viewModel = new TakeTestViewModel
        {
            TestId = test.Id,
            Questions = test.QuestionTests.Select(qt => new TakeTestQuestionVm
            {
                QuestionId = qt.Id,
                Group = qt.QuestionType?.Name ?? "General",
                Content = qt.Content,
                Options = qt.QuestionOptions.Select(opt => new TakeTestOptionVm
                {
                    OptionId = opt.Id,
                    Content = opt.Content
                }).ToList()
            }).ToList()
        };

        return View(viewModel);
    }

    public IActionResult CareerPath() => View();

    public async Task<IActionResult> Training(int? careerPathId)
    {
        List<Resource> resources;
        if (careerPathId.HasValue)
        {
            resources = await _context.Resources
                .Where(r => r.PathId == careerPathId.Value && r.Status == 1)
                .Include(r => r.CareerPath)
                .ToListAsync();
            
            ViewBag.SelectedPath = await _context.CareerPaths.FindAsync(careerPathId.Value);
        }
        else
        {
            resources = await _context.Resources
                .Include(r => r.CareerPath)
                .Where(r => r.Status == 1)
                .ToListAsync();
        }
        return View(resources);
    }

    public IActionResult Jobs() => View();

    public async Task<IActionResult> Community()
    {
        var posts = await _context.CommunityPosts.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return View(posts);
    }

    public async Task<IActionResult> About()
    {
        var team = await _context.TeamMembers.ToListAsync();
        return View(team);
    }

    public IActionResult Contact()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Contact(string name, string email, string subject, string message)
    {
        TempData["ContactSuccess"] = "Cảm ơn bạn đã gửi tin nhắn! Đội ngũ hỗ trợ của CareerPath đã nhận được thông tin và sẽ phản hồi bạn qua email trong vòng 24 giờ tới.";
        return RedirectToAction("Contact");
    }

    public async Task<IActionResult> FAQ()
    {
        var faqs = await _context.FaqItems.ToListAsync();
        return View(faqs);
    }

    public IActionResult Pricing() => View();

    public async Task<IActionResult> News()
    {
        var model = new NewsViewModel
        {
            Articles = await _context.NewsArticles.OrderByDescending(a => a.PublishedDate).ToListAsync(),
            Events = await _context.CareerEvents.OrderBy(e => e.EventDate).ToListAsync()
        };
        return View(model);
    }

    public IActionResult Policy() => View();

    public IActionResult Terms() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}