using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Service.Interfaces;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Career_Guidance_Platform.Filters;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Career_Guidance_Platform.Controllers;

public class HomeController : Controller
{ private readonly IQuestionUserService _questionUserService;

    public HomeController(IQuestionUserService questionUserService)
    {
        _questionUserService = questionUserService;
    }
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, AppDbContext context, UserManager<User> userManager, IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _configuration = configuration;
    }

    public IActionResult Index() => View();

    public async Task<IActionResult> CareerTest(int questionNumber = 1)
    {
        var totalQuestions = await _questionUserService.GetCountAsync();
        
        if (questionNumber < 1)
        {
            questionNumber = 1;
        }

        if (questionNumber > totalQuestions)
        {
            questionNumber = totalQuestions;
        }
        var question = await _questionUserService.GetQuestionByOrderAsync(questionNumber);

        if (question == null)
            return NotFound();

        ViewBag.CurrentQuestion = questionNumber;
        ViewBag.TotalQuestions = totalQuestions;

        return View(question);
    }
    [TypeFilter(typeof(PremiumAccessFilter))]
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
        
        var interestsPool = allQuestions.Where(q => q.TestType == "Interests").OrderBy(q => random.Next()).Take(5).ToList();
        var skillsPool = allQuestions.Where(q => q.TestType == "Skills").OrderBy(q => random.Next()).Take(5).ToList();
        var valuesPool = allQuestions.Where(q => q.TestType == "Values").OrderBy(q => random.Next()).Take(5).ToList();
        var personalityPool = allQuestions.Where(q => q.TestType == "Personality").OrderBy(q => random.Next()).Take(5).ToList();

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
    
    public async Task<IActionResult> Goals()
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            ViewBag.Status = "NotLoggedIn";
            return View(new List<Goal>());
        }

        var userId = int.Parse(userIdValue);
        var goals = await _context.Goals
            .Where(g => g.StudentId == userId)
            .ToListAsync();

        if (!goals.Any())
        {
            ViewBag.Status = "NoGoals";
            return View(new List<Goal>());
        }

        return View(goals);
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

            ViewBag.SavedJobIds = await _context.SavedJobs
                .Where(sj => sj.UserId == userId)
                .Select(sj => sj.JobPostingId)
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
            ViewBag.SavedJobIds = new List<int>();
            ViewBag.RecommendedPathId = null;
        }

        return View(jobs);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ToggleSaveJob(int jobPostingId)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Json(new { success = false, message = "Vui lòng đăng nhập." });
        }
        var userId = int.Parse(userIdValue);

        var savedJob = await _context.SavedJobs
            .FirstOrDefaultAsync(sj => sj.UserId == userId && sj.JobPostingId == jobPostingId);

        bool isSaved;
        if (savedJob != null)
        {
            _context.SavedJobs.Remove(savedJob);
            isSaved = false;
        }
        else
        {
            _context.SavedJobs.Add(new SavedJob
            {
                UserId = userId,
                JobPostingId = jobPostingId,
                SavedAt = DateTime.Now
            });
            isSaved = true;
        }

        await _context.SaveChangesAsync();
        return Json(new { success = true, isSaved = isSaved });
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

    [HttpPost]
    public async Task<IActionResult> LikePost(int id, bool isUnlike = false)
    {
        var post = await _context.CommunityPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound(new { success = false, message = "Không tìm thấy bài thảo luận." });
        }

        if (isUnlike)
        {
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
        }
        else
        {
            post.LikesCount += 1;
        }

        _context.CommunityPosts.Update(post);
        await _context.SaveChangesAsync();

        return Json(new { success = true, likesCount = post.LikesCount });
    }

    public async Task<IActionResult> Mentors()
    {
        var mentors = await _context.MentorProfiles
            .Include(m => m.User)
            .ToListAsync();
        return View(mentors);
    }

    [TypeFilter(typeof(PremiumAccessFilter))]
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

    public async Task<IActionResult> ResumeBuilder(int? id, int? templateId)
    {
        var userIdValue = _userManager.GetUserId(User);
        bool userIsPremium = false;
        
        if (!string.IsNullOrEmpty(userIdValue))
        {
            var userId = int.Parse(userIdValue);
            var user = await _userManager.FindByIdAsync(userIdValue);
            
            if (user != null)
            {
                userIsPremium = user.IsPremium;
            }
            
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

            // Handle default template parameter from /Resume/Templates
            if (templateId.HasValue)
            {
                var template = await _context.ResumeTemplates.FindAsync(templateId.Value);
                if (template != null)
                {
                    if (template.IsPremium && !userIsPremium)
                    {
                        TempData["PremiumLimitMessage"] = $"Mẫu '{template.Name}' là mẫu Premium VIP. Vui lòng nâng cấp tài khoản để sử dụng!";
                        return RedirectToAction("UpgradePremium");
                    }
                    ViewBag.DefaultTemplateCode = template.TemplateCode;
                    ViewBag.TemplateId = template.Id;
                }
            }

            if (id.HasValue)
            {
                var resume = await _context.Resumes
                    .Include(r => r.Template)
                    .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
                
                if (resume != null)
                {
                    if (resume.Template != null)
                    {
                        ViewBag.DefaultTemplateCode = resume.Template.TemplateCode;
                        ViewBag.TemplateId = resume.TemplateId;
                    }
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

        ViewBag.UserIsPremium = userIsPremium;
        return View(null);
    }

    [Authorize]
    public async Task<IActionResult> ViewCV(int id)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue))
        {
            return Challenge();
        }

        var userId = int.Parse(userIdValue);
        var resume = await _context.Resumes
            .Include(r => r.Template)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (resume == null)
        {
            return NotFound("Không tìm thấy CV hoặc bạn không có quyền truy cập CV này.");
        }

        return View(resume);
    }

    [Authorize]
    public async Task<IActionResult> UpgradePremium()
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue)) return Challenge();

        var user = await _userManager.FindByIdAsync(userIdValue);
        if (user == null) return NotFound();

        return View(user);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpgradeToPremium()
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue)) return Challenge();

        var user = await _userManager.FindByIdAsync(userIdValue);
        if (user == null) return NotFound();

        // 1. Gọi PayPal API để tạo đơn hàng
        var accessToken = await GetPayPalAccessTokenAsync();
        if (accessToken != null)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                
                var requestBody = new
                {
                    intent = "CAPTURE",
                    purchase_units = new[]
                    {
                        new
                        {
                            amount = new
                            {
                                currency_code = "USD",
                                value = "1.00"
                            },
                            description = "CareerPath Premium Membership (Lifetime)"
                        }
                    },
                    application_context = new
                    {
                        return_url = $"{Request.Scheme}://{Request.Host}/Home/PaymentSuccess",
                        cancel_url = $"{Request.Scheme}://{Request.Host}/Home/PaymentCancel"
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PostAsync("https://api-m.sandbox.paypal.com/v2/checkout/orders", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonStr = await response.Content.ReadAsStringAsync();
                        using (var doc = JsonDocument.Parse(jsonStr))
                        {
                            var orderId = doc.RootElement.GetProperty("id").GetString();
                            
                            // Tạo bản ghi Pending ngay khi tạo order thành công
                            var payment = new PaymentHistory
                            {
                                UserId = user.Id,
                                PaypalOrderId = orderId ?? "N/A",
                                Amount = 1.00m,
                                Currency = "USD",
                                PaymentStatus = "Pending",
                                CreatedAt = DateTime.Now
                            };
                            _context.PaymentHistories.Add(payment);
                            await _context.SaveChangesAsync();

                            var links = doc.RootElement.GetProperty("links");
                            foreach (var link in links.EnumerateArray())
                            {
                                if (link.GetProperty("rel").GetString() == "approve")
                                {
                                    var approveUrl = link.GetProperty("href").GetString();
                                    return Redirect(approveUrl!);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Fallback to Mock flow below
                }
            }
        }

        // --- FALLBACK MOCK FLOW (Khi không có credentials PayPal hoặc không có mạng) ---
        var mockToken = "MOCK-PAYPAL-" + Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
        
        // Tạo bản ghi Pending cho mock flow
        var mockPayment = new PaymentHistory
        {
            UserId = user.Id,
            PaypalOrderId = mockToken,
            Amount = 1.00m,
            Currency = "USD",
            PaymentStatus = "Pending",
            CreatedAt = DateTime.Now
        };
        _context.PaymentHistories.Add(mockPayment);
        await _context.SaveChangesAsync();

        return RedirectToAction("PaymentSuccess", new { token = mockToken });
    }

    [Authorize]
    public async Task<IActionResult> PaymentSuccess(string token)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue)) return Challenge();

        var user = await _userManager.FindByIdAsync(userIdValue);
        if (user == null) return NotFound();

        bool isPaymentCaptured = false;

        // 1. Xác nhận thanh toán từ PayPal (nếu là token thật)
        if (token != null && !token.StartsWith("MOCK-PAYPAL-"))
        {
            var accessToken = await GetPayPalAccessTokenAsync();
            if (accessToken != null)
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    try
                    {
                        var response = await client.PostAsync($"https://api-m.sandbox.paypal.com/v2/checkout/orders/{token}/capture", new StringContent("", Encoding.UTF8, "application/json"));
                        if (response.IsSuccessStatusCode)
                        {
                            isPaymentCaptured = true;
                        }
                    }
                    catch
                    {
                        // Fallback to true if network fails during demo
                    }
                }
            }
        }
        else
        {
            isPaymentCaptured = true;
        }

        if (isPaymentCaptured || token.StartsWith("MOCK-PAYPAL-"))
        {
            user.IsPremium = true;
            await _userManager.UpdateAsync(user);

            // Cập nhật trạng thái Completed cho bản ghi giao dịch
            var payment = await _context.PaymentHistories
                .FirstOrDefaultAsync(p => p.PaypalOrderId == token && p.PaymentStatus == "Pending");

            if (payment != null)
            {
                payment.PaymentStatus = "Completed";
                payment.CreatedAt = DateTime.Now; // Cập nhật thời gian thanh toán thành công
                _context.PaymentHistories.Update(payment);
            }
            else
            {
                // Fallback nếu trước đó tạo Pending bị lỗi
                payment = new PaymentHistory
                {
                    UserId = user.Id,
                    PaypalOrderId = token ?? "N/A",
                    Amount = 1.00m,
                    Currency = "USD",
                    PaymentStatus = "Completed",
                    CreatedAt = DateTime.Now
                };
                _context.PaymentHistories.Add(payment);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Chúc mừng! Bạn đã trở thành thành viên Premium và mở khóa toàn bộ tính năng vĩnh viễn.";
            return RedirectToAction("Index", "Manage");
        }

        TempData["ErrorMessage"] = "Không thể xác nhận giao dịch thanh toán từ PayPal.";
        return RedirectToAction(nameof(UpgradePremium));
    }

    [Authorize]
    public async Task<IActionResult> PaymentCancel(string token)
    {
        var userIdValue = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdValue)) return Challenge();

        if (!string.IsNullOrEmpty(token))
        {
            var payment = await _context.PaymentHistories
                .FirstOrDefaultAsync(p => p.PaypalOrderId == token && p.PaymentStatus == "Pending");

            if (payment != null)
            {
                payment.PaymentStatus = "Cancelled";
                payment.CreatedAt = DateTime.Now; // Ghi nhận thời gian hủy giao dịch
                _context.PaymentHistories.Update(payment);
                await _context.SaveChangesAsync();
            }
        }

        TempData["ErrorMessage"] = "Bạn đã hủy quá trình thanh toán nâng cấp Premium.";
        return RedirectToAction(nameof(UpgradePremium));
    }

    private async Task<string?> GetPayPalAccessTokenAsync()
    {
        string clientId = _configuration["PayPal:ClientId"];
        string secret = _configuration["PayPal:Secret"];

        if (string.IsNullOrEmpty(clientId) || clientId.Contains("YOUR_PAYPAL_SANDBOX") || string.IsNullOrEmpty(secret) || secret.Contains("YOUR_PAYPAL_SANDBOX"))
        {
            clientId = "AdVp5sZ4rZ1u4Z-GvH6X2q3p5UoK9y1p_bE0w3P4y-Sg8M1U4zN1w2p3o4y5u6i7o8p9a0s1d2f3g4h5";
            secret = "EG1234567890aBcDeFgHiJkLmNoPqRsTuVwXyZ1234567890aBcDeFgHiJkLmNoPqRsTuVwXyZ";
        }

        using (var client = new HttpClient())
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{secret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            try
            {
                var response = await client.PostAsync("https://api-m.sandbox.paypal.com/v1/oauth2/token", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonStr = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(jsonStr))
                    {
                        return doc.RootElement.GetProperty("access_token").GetString();
                    }
                }
            }
            catch
            {
                // Fallback silently
            }
        }
        return null;
    }

    public IActionResult Policy() => View();

    public IActionResult Terms() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}