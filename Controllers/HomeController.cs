using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Career_Guidance_Platform.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> CareerTest()
    {
        var test = await _context.Tests
            .FirstOrDefaultAsync(t => t.Status == 1);

        if (test == null)
        {
            return NotFound("Không tìm thấy bài đánh giá nghề nghiệp nào đang hoạt động.");
        }

        var allQuestions = await _context.QuestionTests
            .Where(q => q.TestId == test.Id && q.Status == 1)
            .Include(q => q.QuestionOptions)
            .ToListAsync();

        var random = new Random();
        
        var interestsPool = allQuestions.Where(q => q.QuestionType.Name == "Interests").OrderBy(q => random.Next()).Take(5).ToList();
        var skillsPool = allQuestions.Where(q => q.QuestionType.Name == "Skills").OrderBy(q => random.Next()).Take(5).ToList();
        var valuesPool = allQuestions.Where(q => q.QuestionType.Name == "Values").OrderBy(q => random.Next()).Take(5).ToList();
        var personalityPool = allQuestions.Where(q => q.QuestionType.Name == "Personality").OrderBy(q => random.Next()).Take(5).ToList();

        var selectedQuestions = interestsPool
            .Concat(skillsPool)
            .Concat(valuesPool)
            .Concat(personalityPool)
            .OrderBy(q => random.Next())
            .ToList();

        var viewModel = new TakeTestViewModel
        {
            TestId = test.Id,
            Questions = selectedQuestions.Select(qt => new TakeTestQuestionVm
            {
                QuestionId = qt.Id,
                Group = qt.QuestionType.Name == "Interests" ? "Sở thích"
                        : qt.QuestionType.Name == "Skills" ? "Kỹ năng"
                        : qt.QuestionType.Name == "Values" ? "Giá trị"
                        : qt.QuestionType.Name == "Personality" ? "Tính cách"
                        : qt.QuestionType.Name,
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

    public async Task<IActionResult> CareerPath()
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            ViewBag.Status = "NotLoggedIn";
            return View(new List<CareerPath>());
        }

        var userId = int.Parse(userIdValue);
        var pathIds = await _context.TestResults
            .Where(tr => tr.UserId == userId && tr.RecommendedCareerPathId.HasValue)
            .Select(tr => tr.RecommendedCareerPathId.Value)
            .Distinct()
            .ToListAsync();

        if (!pathIds.Any())
        {
            ViewBag.Status = "NoTestResults";
            return View(new List<CareerPath>());
        }

        ViewBag.Status = "HasResults";
        var paths = await _context.CareerPaths
            .Include(cp => cp.Category)
            .Where(cp => pathIds.Contains(cp.Id) && cp.Status == 1)
            .ToListAsync();

        return View(paths);
    }
    
    public async Task<IActionResult> About()
    {
        var members = await _context.TeamMembers.ToListAsync();
        return View(members);
    }

    public IActionResult Contact() => View();

    public async Task<IActionResult> FAQ()
    {
        var faqs = await _context.FaqItems.ToListAsync();
        return View(faqs);
    }

    public async Task<IActionResult> News()
    {
        var viewModel = new NewsViewModel
        {
            Articles = await _context.NewsArticles.OrderByDescending(a => a.PublishedDate).ToListAsync(),
            Events = await _context.CareerEvents.OrderByDescending(e => e.EventDate).ToListAsync()
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Jobs()
    {
        var jobs = await _context.JobPostings
            .Include(j => j.CareerPath)
            .Where(j => j.Status == 1)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        var userIdValue = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userIdValue))
        {
            var userId = int.Parse(userIdValue);
            ViewBag.Resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.AppliedJobIds = await _context.JobApplications
                .Where(ja => ja.UserId == userId)
                .Select(ja => ja.JobPostingId)
                .ToListAsync();

            var latestResult = await _context.TestResults
                .Where(tr => tr.UserId == userId && tr.RecommendedCareerPathId.HasValue)
                .OrderByDescending(tr => tr.CreatedAt)
                .FirstOrDefaultAsync();

            ViewBag.RecommendedPathId = latestResult?.RecommendedCareerPathId;
        }
        else
        {
            ViewBag.Resumes = new List<Resume>();
            ViewBag.AppliedJobIds = new List<int>();
            ViewBag.RecommendedPathId = null;
        }

        return View(jobs);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> ApplyJob(int jobPostingId, int resumeId, string? notes)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);

        // Verify resume belongs to the user
        var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
        if (resume == null)
        {
            TempData["ApplyWarning"] = "Hồ sơ CV chọn không hợp lệ hoặc không thuộc quyền sở hữu của bạn.";
            return RedirectToAction(nameof(Jobs));
        }

        // Check if already applied
        var alreadyApplied = await _context.JobApplications.AnyAsync(ja => ja.UserId == userId && ja.JobPostingId == jobPostingId);
        if (alreadyApplied)
        {
            TempData["ApplyWarning"] = "Bạn đã ứng tuyển công việc này trước đó.";
            return RedirectToAction(nameof(Jobs));
        }

        var application = new JobApplication
        {
            UserId = userId,
            JobPostingId = jobPostingId,
            ResumeId = resumeId,
            Notes = notes,
            Status = "Applied",
            AppliedAt = DateTime.Now
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        TempData["ApplySuccess"] = "Đơn ứng tuyển của bạn đã được gửi thành công!";
        return RedirectToAction(nameof(Jobs));
    }

    public async Task<IActionResult> Community()
    {
        var posts = await _context.CommunityPosts
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(posts);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> CreatePost(string title, string content, string category)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);
        var user = await _userManager.FindByIdAsync(userIdValue);

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(category))
        {
            TempData["ErrorMessage"] = "Tiêu đề, nội dung và chuyên mục không được để trống!";
            return RedirectToAction(nameof(Community));
        }

        var post = new CommunityPost
        {
            Title = title.Trim(),
            Content = content.Trim(),
            Category = category.Trim(),
            AuthorName = user?.FullName ?? user?.UserName ?? "Thành viên",
            AuthorId = userId,
            CreatedAt = DateTime.Now,
            LikesCount = 0,
            RepliesCount = 0
        };

        _context.CommunityPosts.Add(post);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Đăng bài thảo luận mới thành công!";
        return RedirectToAction(nameof(Community));
    }

    public async Task<IActionResult> PostDetails(int id)
    {
        var post = await _context.CommunityPosts
            .Include(p => p.Comments)
                .ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound("Không tìm thấy bài thảo luận này.");
        }

        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(int postId, string content)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);

        var post = await _context.CommunityPosts.FindAsync(postId);
        if (post == null)
        {
            return NotFound("Không tìm thấy bài thảo luận.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Bình luận không được để trống!";
            return RedirectToAction(nameof(PostDetails), new { id = postId });
        }

        var comment = new CommunityComment
        {
            PostId = postId,
            AuthorId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.CommunityComments.Add(comment);
        
        // Increment reply count
        post.RepliesCount += 1;
        _context.CommunityPosts.Update(post);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Gửi phản hồi thành công!";
        return RedirectToAction(nameof(PostDetails), new { id = postId });
    }

    public async Task<IActionResult> Mentors()
    {
        var mentors = await _context.MentorProfiles
            .Include(m => m.User)
            .ToListAsync();
        return View(mentors);
    }

    public async Task<IActionResult> Training(int? careerPathId)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            ViewBag.Status = "NotLoggedIn";
            return View(new List<Resource>());
        }

        var userId = int.Parse(userIdValue);
        var pathIds = await _context.TestResults
            .Where(tr => tr.UserId == userId && tr.RecommendedCareerPathId.HasValue)
            .Select(tr => tr.RecommendedCareerPathId.Value)
            .Distinct()
            .ToListAsync();

        if (!pathIds.Any())
        {
            ViewBag.Status = "NoTestResults";
            return View(new List<Resource>());
        }

        ViewBag.Status = "HasResults";

        var recommendedPaths = await _context.CareerPaths
            .Where(cp => pathIds.Contains(cp.Id) && cp.Status == 1)
            .ToListAsync();
        ViewBag.RecommendedPaths = recommendedPaths;

        CareerPath? selectedPath = null;
        if (careerPathId.HasValue && pathIds.Contains(careerPathId.Value))
        {
            selectedPath = recommendedPaths.FirstOrDefault(p => p.Id == careerPathId.Value);
        }
        else if (recommendedPaths.Any())
        {
            selectedPath = recommendedPaths.First();
        }
        ViewBag.SelectedPath = selectedPath;

        var resources = new List<Resource>();
        if (selectedPath != null)
        {
            resources = await _context.Resources
                .Include(r => r.CareerPath)
                .Where(r => r.PathId == selectedPath.Id && r.Status == 1)
                .ToListAsync();
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

    public async Task<IActionResult> ResumeBuilder(int? id)
    {
        var userIdValue = _userManager.GetUserId(User);
        
        if (!string.IsNullOrEmpty(userIdValue))
        {
            var userId = int.Parse(userIdValue);
            var user = await _userManager.FindByIdAsync(userIdValue);
            
            ViewBag.FullName = user?.FullName;
            ViewBag.Email = user?.Email;

            // Fetch user's completed/acquired skills
            ViewBag.UserSkills = await _context.UserSkills
                .Where(us => us.UserId == userId && (us.Status == "Completed" || us.Status == "Acquired"))
                .Include(us => us.Skill)
                .Select(us => us.Skill!.Name)
                .ToListAsync();

            // Fetch user's completed courses
            ViewBag.CompletedCourses = await _context.UserCourseProgresses
                .Where(ucp => ucp.UserId == userId && ucp.Status == "Completed")
                .Include(ucp => ucp.Course)
                .Select(ucp => ucp.Course!.Title)
                .ToListAsync();

            if (id.HasValue)
            {
                var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
                if (resume != null)
                {
                    return View(resume);
                }
            }
        }
        else
        {
            ViewBag.FullName = null;
            ViewBag.Email = null;
            ViewBag.UserSkills = new List<string>();
            ViewBag.CompletedCourses = new List<string>();
        }

        return View(null);
    }

    public IActionResult Policy() => View();

    public IActionResult Terms() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}