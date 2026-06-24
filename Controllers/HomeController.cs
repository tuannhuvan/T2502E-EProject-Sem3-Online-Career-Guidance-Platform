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

    public IActionResult Training() => View();

    public IActionResult Jobs() => View();

    public IActionResult Community() => View();

    public IActionResult About() => View();

    public IActionResult Contact() => View();

    public IActionResult FAQ() => View();

    public IActionResult Pricing() => View();

    public IActionResult News() => View();

    public IActionResult Policy() => View();

    public IActionResult Terms() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}