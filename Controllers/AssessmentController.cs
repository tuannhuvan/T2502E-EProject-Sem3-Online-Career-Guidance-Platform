using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Career;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers;

public class AssessmentController : Controller
{
    private readonly IAssessmentService _assessmentService;
    private readonly AppDbContext _context;

    public AssessmentController(AppDbContext context, IAssessmentService assessmentService)
    {
        _context = context;
        _assessmentService = assessmentService;
    }

    // =========================
    // SUBMIT TEST
    // =========================
    [HttpPost]
    public async Task<IActionResult> Submit(AssessmentResultDto dto)
    {
        var resultId = await _assessmentService.SubmitTestAsync(dto);

        return RedirectToAction("Result", new { id = resultId });
    }

    // =========================
    // RESULT (FROM DATABASE)
    // =========================
    public async Task<IActionResult> Result(int id)
    {
        // 1. Lấy result từ DB
        var result = await _context.AssessmentResults
            .Include(x => x.UserAnswers)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (result == null)
            return NotFound();

        // 2. Convert answers
        var answerDtos = result.UserAnswers.Select(x => new UserAnswerDto
        {
            QuestionId = x.QuestionId,
            OptionId = x.OptionId
        }).ToList();

        // 3. Tính điểm
        var scores = await _assessmentService.CalculateScores(answerDtos);

        // 4. Top 3 career
        var top3 = scores
            .OrderByDescending(x => x.Value)
            .Take(3)
            .ToList();

        // 5. Load CareerPaths
        var careers = await _context.CareerPaths
            .Where(x => top3.Select(t => t.Key).Contains(x.Id))
            .ToListAsync();

        // 6. Map result
        var resultData = careers.Select(c => new TopCareerDto
        {
            CareerPathId = c.Id,
            Title = c.Title,
            Score = top3.First(x => x.Key == c.Id).Value
        }).ToList();

        return View("~/Views/Assessment/Result.cshtml",resultData);
    }
}