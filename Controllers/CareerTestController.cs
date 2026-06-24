using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers
{
    public class CareerTestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CareerTestController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(TakeTestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userIdStr = _userManager.GetUserId(User);
                int? userId = string.IsNullOrEmpty(userIdStr) ? (int?)null : int.Parse(userIdStr);

                var testResult = new TestResult
                {
                    TestId = model.TestId,
                    UserId = userId,
                    DateTaken = System.DateTime.Now,
                    CreatedAt = System.DateTime.Now,
                    CreatedBy = User.Identity?.Name ?? "Anonymous"
                };

                _context.TestResults.Add(testResult);
                await _context.SaveChangesAsync();

                foreach (var answer in model.Answers)
                {
                    var testAnswer = new TestAnswer
                    {
                        TestResultId = testResult.Id,
                        QuestionId = answer.QuestionId,
                        QuestionOptionId = answer.OptionId,
                        CreatedAt = System.DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "Anonymous"
                    };
                    _context.TestAnswers.Add(testAnswer);
                }
                await _context.SaveChangesAsync();

                // Compute the recommended career path based on Holland scores (weights mapping options to career paths)
                var optionIds = model.Answers.Select(a => a.OptionId).ToList();
                var pathScores = await _context.OptionCareerPaths
                    .Where(ocp => optionIds.Contains(ocp.OptionId))
                    .GroupBy(ocp => ocp.CareerPathId)
                    .Select(g => new { CareerPathId = g.Key, Score = g.Sum(ocp => ocp.Weight) })
                    .OrderByDescending(x => x.Score)
                    .ToListAsync();

                if (pathScores.Any())
                {
                    var best = pathScores.First();
                    testResult.RecommendedCareerPathId = best.CareerPathId;

                    var totalWeight = pathScores.Sum(x => x.Score);
                    testResult.CompatibilityScore = totalWeight > 0 
                        ? (decimal)System.Math.Round((double)best.Score / totalWeight * 100, 2) 
                        : 100;

                    await _context.SaveChangesAsync();
                }

                // Store the result ID in session
                HttpContext.Session.SetInt32("TestResultId", testResult.Id);

                // If not logged in, redirect to register with a notice message
                if (!userId.HasValue)
                {
                    TempData["RequireAuthForTestResult"] = "Vui lòng đăng nhập hoặc đăng ký tài khoản để xem kết quả đánh giá nghề nghiệp của bạn.";
                    return RedirectToAction("Register", "Account");
                }

                // If logged in, redirect to result detail page
                return RedirectToAction("GetResultDetail", new { id = testResult.Id });
            }

            return RedirectToAction("CareerTest", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> SubmitTest([FromBody] TakeTestViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var testResult = new TestResult
                {
                    TestId = model.TestId,
                    UserId = string.IsNullOrEmpty(userId) ? (int?)null : int.Parse(userId),
                    DateTaken = System.DateTime.Now
                };

                _context.TestResults.Add(testResult);
                await _context.SaveChangesAsync();

                foreach (var answer in model.Answers)
                {
                    var testAnswer = new TestAnswer
                    {
                        TestResultId = testResult.Id,
                        QuestionId = answer.QuestionId,
                        QuestionOptionId = answer.OptionId
                    };
                    _context.TestAnswers.Add(testAnswer);
                }
                await _context.SaveChangesAsync();

                // Compute scores
                var optionIds = model.Answers.Select(a => a.OptionId).ToList();
                var pathScores = await _context.OptionCareerPaths
                    .Where(ocp => optionIds.Contains(ocp.OptionId))
                    .GroupBy(ocp => ocp.CareerPathId)
                    .Select(g => new { CareerPathId = g.Key, Score = g.Sum(ocp => ocp.Weight) })
                    .OrderByDescending(x => x.Score)
                    .ToListAsync();

                if (pathScores.Any())
                {
                    var best = pathScores.First();
                    testResult.RecommendedCareerPathId = best.CareerPathId;

                    var totalWeight = pathScores.Sum(x => x.Score);
                    testResult.CompatibilityScore = totalWeight > 0 
                        ? (decimal)System.Math.Round((double)best.Score / totalWeight * 100, 2) 
                        : 100;

                    await _context.SaveChangesAsync();
                }

                // Store the result ID in the session and redirect to login/register
                HttpContext.Session.SetInt32("TestResultId", testResult.Id);

                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { redirectTo = Url.Action("Register", "Account") });
                }

                return Json(new { redirectTo = Url.Action("GetResultDetail", new { id = testResult.Id }) });
            }

            return BadRequest(ModelState);
        }

        public async Task<IActionResult> GetResultDetail(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                TempData["RequireAuthForTestResult"] = "Vui lòng đăng nhập hoặc đăng ký tài khoản để xem kết quả đánh giá nghề nghiệp của bạn.";
                return RedirectToAction("Register", "Account");
            }

            var testResult = await _context.TestResults
                .Include(tr => tr.Test)
                .Include(tr => tr.RecommendedCareerPath)
                .Include(tr => tr.User)
                .Include(tr => tr.TestAnswers)
                    .ThenInclude(ta => ta.QuestionTest)
                .Include(tr => tr.TestAnswers)
                    .ThenInclude(ta => ta.QuestionOption)
                        .ThenInclude(qo => qo.OptionCareerPaths)
                            .ThenInclude(ocp => ocp.CareerPath)
                .FirstOrDefaultAsync(tr => tr.Id == id && tr.UserId == int.Parse(userId));

            if (testResult == null)
            {
                return NotFound();
            }

            // Retrieve matching JobPostings
            if (testResult.RecommendedCareerPathId.HasValue)
            {
                ViewBag.Jobs = await _context.Set<JobPosting>()
                    .Where(jp => jp.CareerPathId == testResult.RecommendedCareerPathId.Value && jp.Status == 1)
                    .ToListAsync();
            }
            else
            {
                ViewBag.Jobs = new List<JobPosting>();
            }

            return View(testResult);
        }
    }
}