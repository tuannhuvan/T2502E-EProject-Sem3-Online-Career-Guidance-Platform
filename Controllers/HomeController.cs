using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System.Linq;

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

        // Fetch all active questions with options for this test
        var allQuestions = await _context.QuestionTests
            .Where(q => q.TestId == test.Id && q.Status == 1)
            .Include(q => q.QuestionOptions)
            .ToListAsync();

        var random = new Random();
        
        var interestsPool = allQuestions.Where(q => q.TestType == "Interests").OrderBy(q => random.Next()).Take(5).ToList();
        var skillsPool = allQuestions.Where(q => q.TestType == "Skills").OrderBy(q => random.Next()).Take(5).ToList();
        var valuesPool = allQuestions.Where(q => q.TestType == "Values").OrderBy(q => random.Next()).Take(5).ToList();
        var personalityPool = allQuestions.Where(q => q.TestType == "Personality").OrderBy(q => random.Next()).Take(5).ToList();

        var selectedQuestions = interestsPool
            .Concat(skillsPool)
            .Concat(valuesPool)
            .Concat(personalityPool)
            .OrderBy(q => random.Next()) // Shuffle final selection
            .ToList();

        var viewModel = new TakeTestViewModel
        {
            TestId = test.Id,
            Questions = selectedQuestions.Select(qt => new TakeTestQuestionVm
            {
                QuestionId = qt.Id,
                Group = qt.TestType == "Interests" ? "Sở thích"
                        : qt.TestType == "Skills" ? "Kỹ năng"
                        : qt.TestType == "Values" ? "Giá trị"
                        : qt.TestType == "Personality" ? "Tính cách"
                        : qt.TestType,
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

        // Get matching career paths for selection dropdown
        var recommendedPaths = await _context.CareerPaths
            .Where(cp => pathIds.Contains(cp.Id) && cp.Status == 1)
            .ToListAsync();
        ViewBag.RecommendedPaths = recommendedPaths;

        List<Resource> resources;
        if (careerPathId.HasValue)
        {
            if (pathIds.Contains(careerPathId.Value))
            {
                resources = await _context.Resources
                    .Where(r => r.PathId == careerPathId.Value && r.Status == 1)
                    .Include(r => r.CareerPath)
                    .ToListAsync();
                
                ViewBag.SelectedPath = recommendedPaths.FirstOrDefault(p => p.Id == careerPathId.Value);
            }
            else
            {
                resources = await _context.Resources
                    .Where(r => pathIds.Contains(r.PathId) && r.Status == 1)
                    .Include(r => r.CareerPath)
                    .ToListAsync();
            }
        }
        else
        {
            resources = await _context.Resources
                .Include(r => r.CareerPath)
                .Where(r => pathIds.Contains(r.PathId) && r.Status == 1)
                .ToListAsync();
        }
        return View(resources);
    }

    public async Task<IActionResult> Jobs()
    {
        var jobs = await _context.JobPostings
            .Include(j => j.CareerPath)
            .Where(j => j.Status == 1)
            .ToListAsync();

        var userIdValue = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userIdValue))
        {
            var userId = int.Parse(userIdValue);
            
            // Get latest test result to find recommended career path
            var latestResult = await _context.TestResults
                .Where(tr => tr.UserId == userId)
                .OrderByDescending(tr => tr.DateTaken)
                .FirstOrDefaultAsync();

            if (latestResult != null && latestResult.RecommendedCareerPathId.HasValue)
            {
                ViewBag.RecommendedPathId = latestResult.RecommendedCareerPathId.Value;
                // Sort jobs: matching recommended path first
                jobs = jobs.OrderByDescending(j => j.CareerPathId == latestResult.RecommendedCareerPathId.Value).ToList();
            }

            // Fetch user's saved resumes
            ViewBag.Resumes = await _context.Resumes
                .Where(r => r.UserId == userId)
                .ToListAsync();

            // Fetch already applied job postings IDs
            ViewBag.AppliedJobIds = await _context.JobApplications
                .Where(ja => ja.UserId == userId)
                .Select(ja => ja.JobPostingId)
                .ToListAsync();
        }
        else
        {
            ViewBag.Resumes = new List<Resume>();
            ViewBag.AppliedJobIds = new List<int>();
        }

        return View(jobs);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyJob(int jobPostingId, int? resumeId, string? notes)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);

        // Check if already applied
        var existing = await _context.JobApplications
            .FirstOrDefaultAsync(ja => ja.UserId == userId && ja.JobPostingId == jobPostingId);

        if (existing != null)
        {
            TempData["ApplyWarning"] = "Bạn đã nộp đơn ứng tuyển cho công việc này trước đó rồi!";
            return RedirectToAction("Jobs");
        }

        var application = new JobApplication
        {
            UserId = userId,
            JobPostingId = jobPostingId,
            ResumeId = resumeId,
            Status = "Applied",
            AppliedAt = DateTime.Now,
            Notes = notes
        };

        _context.JobApplications.Add(application);
        await _context.SaveChangesAsync();

        TempData["ApplySuccess"] = "Đơn ứng tuyển của bạn đã được gửi thành công! Nhà tuyển dụng sẽ xem xét hồ sơ của bạn.";
        return RedirectToAction("Jobs");
    }

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

    public async Task<IActionResult> ResumeBuilder(int? id)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (!string.IsNullOrEmpty(userIdValue))
        {
            var userId = int.Parse(userIdValue);
            
            // Get completed courses
            var completedCourses = await _context.UserCourseProgresses
                .Include(up => up.Course)
                .Where(up => up.UserId == userId && up.Status == "Completed")
                .Select(up => up.Course.Title)
                .ToListAsync();

            // Get user skills
            var userSkills = await _context.UserSkills
                .Include(us => us.Skill)
                .Where(us => us.UserId == userId)
                .Select(us => us.Skill.Name)
                .ToListAsync();

            ViewBag.CompletedCourses = completedCourses;
            ViewBag.UserSkills = userSkills;

            var userObj = await _userManager.GetUserAsync(User);
            if (userObj != null)
            {
                ViewBag.FullName = userObj.FullName;
                ViewBag.Email = userObj.Email;
            }
        }
        else
        {
            ViewBag.CompletedCourses = new List<string>();
            ViewBag.UserSkills = new List<string>();
        }

        if (id.HasValue)
        {
            if (string.IsNullOrEmpty(userIdValue))
            {
                return Challenge();
            }

            var resume = await _context.Resumes
                .FirstOrDefaultAsync(r => r.Id == id.Value && r.UserId == int.Parse(userIdValue));

            if (resume == null)
            {
                return NotFound("Không tìm thấy CV hoặc bạn không có quyền chỉnh sửa.");
            }

            return View(resume);
        }

        return View();
    }

    public async Task<IActionResult> Mentors()
    {
        var mentors = await _context.MentorProfiles
            .Include(m => m.User)
            .ToListAsync();
        return View(mentors);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> BookMentor(int mentorId, string meetingDate, string meetingTime, string notes)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);
        var mentorUser = await _userManager.FindByIdAsync(mentorId.ToString());
        if (mentorUser == null)
        {
            return NotFound("Không tìm thấy thông tin Mentor.");
        }

        var parsedTime = DateTime.Parse($"{meetingDate} {meetingTime}");

        var meeting = new MentorshipMeeting
        {
            MenteeId = userId,
            MentorId = mentorId,
            Title = $"Tư vấn định hướng nghề nghiệp cùng Mentor {mentorUser.FullName}",
            Description = notes,
            ScheduledTime = parsedTime,
            MeetingUrl = "https://zoom.us/j/9998881234",
            Status = "Scheduled",
            CreatedAt = DateTime.Now
        };

        _context.MentorshipMeetings.Add(meeting);
        await _context.SaveChangesAsync();

        TempData["BookingSuccess"] = $"Đặt lịch hẹn tư vấn thành công với Mentor {mentorUser.FullName} vào lúc {parsedTime:dd/MM/yyyy HH:mm}! Link Zoom: {meeting.MeetingUrl}";
        return RedirectToAction("Mentors");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}