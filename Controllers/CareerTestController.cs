using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Models.ViewModels;

namespace Career_Guidance_Platform.Controllers
{
    public class CareerTestController : Controller
    {
        private readonly AppDbContext _db;

        public CareerTestController(AppDbContext db)
        {
            _db = db;
        }

        // GET /CareerTest/TakeTest/{id}
        [HttpGet]
        public async Task<IActionResult> TakeTest(int id = 1)
        {
            var questions = await _db.QuestionTests
                .Where(q => q.TestId == id && q.Status == 1)
                .Include(q => q.QuestionOptions)
                .OrderBy(q => q.Id)
                .ToListAsync();

            var vm = new TakeTestViewModel
            {
                TestId = id,
                Questions = questions.Select(q => new TakeTestQuestionVm
                {
                    QuestionId = q.Id,
                    Group = q.QuestionType?.Name ?? string.Empty,
                    Content = q.Content,
                    Options = q.QuestionOptions.OrderBy(o => o.Id).Select(o => new TakeTestOptionVm
                    {
                        OptionId = o.Id,
                        Content = o.Content
                    }).ToList()
                }).ToList()
            };

            return View("~/Views/Home/CareerTest.cshtml", vm);
        }

        // POST /CareerTest/SubmitAnswers
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAnswers(UserSubmissionsModel model)
        {
            if (model == null || model.Answers == null || model.Answers.Count == 0)
            {
                ModelState.AddModelError("", "No answers submitted.");
                return RedirectToAction("TakeTest", new { id = model?.TestId ?? 1 });
            }

            // Basic server-side validation: ensure each question has an answer
            var questionIds = model.Answers.Select(a => a.QuestionId).Distinct().ToList();
            var questionCount = await _db.QuestionTests.CountAsync(q => q.TestId == model.TestId && q.Status == 1);
            if (questionIds.Count != questionCount)
            {
                // incomplete or tampered submission
                ModelState.AddModelError("", "Incomplete or invalid submission.");
                return RedirectToAction("TakeTest", new { id = model.TestId });
            }

            // Validate OptionId belongs to each QuestionId
            foreach (var ans in model.Answers)
            {
                var opt = await _db.QuestionOptions.FirstOrDefaultAsync(o => o.Id == ans.OptionId && o.QuestionId == ans.QuestionId && o.Status == 1);
                if (opt == null)
                {
                    ModelState.AddModelError("", "Invalid option for question.");
                    return RedirectToAction("TakeTest", new { id = model.TestId });
                }
            }

            // Get current user id (make sure authentication populates ClaimTypes.NameIdentifier)
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var studentId))
            {
                // user id missing: require authentication
                return Forbid();
            }

            // Create TestResult
            var result = new TestResult
            {
                StudentId = studentId,
                TestId = model.TestId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? userIdClaim ?? "system"
            };

            _db.TestResults.Add(result);
            await _db.SaveChangesAsync(); // generates result.Id

            // Save TestAnswers
            var answersToAdd = model.Answers.Select(a => new TestAnswer
            {
                ResultId = result.Id,
                QuestionId = a.QuestionId,
                OptionId = a.OptionId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = User.Identity?.Name ?? userIdClaim ?? "system"
            }).ToList();

            _db.TestAnswers.AddRange(answersToAdd);
            await _db.SaveChangesAsync();

            // Compute recommended career path using OptionCareerPath weights
            var selectedOptionIds = model.Answers.Select(a => a.OptionId).ToList();

            var scoreByCareer = await _db.OptionCareerPaths
                .Where(ocp => selectedOptionIds.Contains(ocp.OptionId) && ocp.Status == 1)
                .GroupBy(ocp => ocp.CareerPathId)
                .Select(g => new { CareerPathId = g.Key, WeightSum = g.Sum(x => x.Weight) })
                .OrderByDescending(x => x.WeightSum)
                .ToListAsync();

            int? recommendedCareerPathId = null;
            decimal? compatibilityScore = null;

            if (scoreByCareer.Any())
            {
                var top = scoreByCareer.First();
                recommendedCareerPathId = top.CareerPathId;
                var topWeight = top.WeightSum;

                // Compute max possible for chosen career path: sum of best weight per question (for that career)
                var questionIdsList = model.Answers.Select(a => a.QuestionId).Distinct().ToList();
                decimal maxPossible = 0;

                foreach (var qId in questionIdsList)
                {
                    // For this question, find the maximum weight among its options for the chosen career
                    var maxWeight = await _db.OptionCareerPaths
                        .Where(ocp => ocp.CareerPathId == recommendedCareerPathId && ocp.Status == 1)
                        .Join(_db.QuestionOptions,
                              ocp => ocp.OptionId,
                              opt => opt.Id,
                              (ocp, opt) => new { ocp, opt })
                        .Where(x => x.opt.QuestionId == qId)
                        .Select(x => (int?)x.ocp.Weight)
                        .MaxAsync();

                    if (maxWeight.HasValue)
                        maxPossible += maxWeight.Value;
                    else
                    {
                        // If no OptionCareerPath entries for this career/question, assume 1 as default
                        maxPossible += 1;
                    }
                }

                if (maxPossible > 0)
                {
                    compatibilityScore = Math.Round((decimal)topWeight / maxPossible * 100M, 2);
                }
                else
                {
                    compatibilityScore = 100M;
                }
            }

            // Update result
            result.RecommendedCareerPathId = recommendedCareerPathId;
            result.CompatibilityScore = compatibilityScore;
            result.UpdatedAt = DateTime.UtcNow;
            result.UpdatedBy = User.Identity?.Name ?? userIdClaim ?? "system";

            _db.TestResults.Update(result);
            await _db.SaveChangesAsync();

            // Redirect to view result detail
            return RedirectToAction("GetResultDetail", new { resultId = result.Id });
        }

        [Authorize]
        public async Task<IActionResult> GetResultDetail(int resultId)
        {
            var result = await _db.TestResults
                .Include(r => r.TestAnswers)
                .ThenInclude(ta => ta.QuestionOption)
                .Include(r => r.TestAnswers)
                .ThenInclude(ta => ta.QuestionTest)
                .Include(r => r.RecommendedCareerPath)
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.Id == resultId);

            if (result == null) return NotFound();

            // Ownership check: only the student who created result (or role "Admin") can view
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!User.IsInRole("Admin") && (userIdClaim == null || result.StudentId.ToString() != userIdClaim))
            {
                return Forbid();
            }

            return View(result);
        }
    }
}