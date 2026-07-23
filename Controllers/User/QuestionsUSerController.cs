using System.Security.Claims;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Career;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Career_Guidance_Platform.Controllers;


public class QuestionsController : Controller
{
    private readonly IQuestionUserService _service;
    private readonly AppDbContext _context;

    public QuestionsController(IQuestionUserService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }

    // =========================
    // START TEST
    // =========================
    public async Task<IActionResult> Index(int questionNumber = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"INDEX USER = {userId}");
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login", "Account");

        // 1. tạo hoặc lấy result
        var result = await _context.AssessmentResults
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.CareerTestId == 1);
        Console.WriteLine(result == null
            ? "INDEX RESULT NULL"
            : $"INDEX FOUND RESULT {result.Id}");

        if (result == null)
        {
            result = new AssessmentResult
            {
                UserId = userId,
                CareerTestId = 1,
                CreatedAt = DateTime.Now,
                Status = 1
            };

            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();
        }

        // 2. truyền resultId xuống view
        ViewBag.ResultId = result.Id;

        var totalQuestions = await _service.GetCountAsync();
        var question = await _service.GetQuestionByOrderAsync(questionNumber);

        if (question == null)
            return NotFound();

        ViewBag.CurrentQuestion = questionNumber;
        ViewBag.TotalQuestions = totalQuestions;

        // 3. load câu đã chọn (nếu có)
        var selected = await _context.UserAnswers
            .FirstOrDefaultAsync(x =>
                x.AssessmentResultId == result.Id &&
                x.QuestionId == question.Id);

        ViewBag.SelectedOption = selected?.OptionId;

        return View("~/Views/Home/CareerTest.cshtml", question);
    }

    // =========================
    // SAVE ANSWER (DB)
    // =========================
    [HttpPost]
    public async Task<IActionResult> SaveAnswer(QuestionSubmitDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Console.WriteLine($"UserId = {userId}");

        var results = await _context.AssessmentResults.ToListAsync();

        Console.WriteLine($"Total Results = {results.Count}");

        foreach (var r in results)
        {
            Console.WriteLine(
                $"ResultId={r.Id}, UserId={r.UserId}, CareerTestId={r.CareerTestId}");
        }

        var result = await _context.AssessmentResults
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.CareerTestId == 1);
        Console.WriteLine(result == null
            ? "RESULT NULL"
            : $"FOUND RESULT {result.Id}");

        if (result == null)
        {
            result = new AssessmentResult
            {
                UserId = userId,
                CareerTestId = 1,
                CreatedAt = DateTime.Now,
                Status = 1
            };

            _context.AssessmentResults.Add(result);
            await _context.SaveChangesAsync();
        }

        var existing = await _context.UserAnswers
            .FirstOrDefaultAsync(x =>
                x.AssessmentResultId == result.Id &&
                x.QuestionId == dto.QuestionId);

        if (existing == null)
        {
            _context.UserAnswers.Add(new UserAnswer
            {
                AssessmentResultId = result.Id,
                QuestionId = dto.QuestionId,
                OptionId = dto.OptionId,
                CreatedAt = DateTime.Now,
                Status = 1
            });
        }
        else
        {
            existing.OptionId = dto.OptionId;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", new
        {
            questionNumber = dto.CurrentQuestion + 1
        });
    }

    // =========================
    // FINISH (TOP 3 CAREER)
    // =========================
    [HttpPost]
    public async Task<IActionResult> Finish(int resultId)
    {
        var result = await _context.AssessmentResults
            .Include(x => x.UserAnswers)
            .FirstOrDefaultAsync(x => x.Id == resultId);

        if (result == null)
            return NotFound();

        var answers = result.UserAnswers
            .Select(x => new UserAnswerDto
            {
                QuestionId = x.QuestionId,
                OptionId = x.OptionId
            })
            .ToList();

        var resultData = await _service.GetTop3CareerAsync(answers);

        return View("~/Views/Assessment/Result.cshtml", resultData);
    }
}