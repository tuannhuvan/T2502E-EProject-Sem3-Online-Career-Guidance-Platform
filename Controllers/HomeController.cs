using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Career_Guidance_Platform.Models;
using Career_Guidance_Platform.Service.Interfaces;

namespace Career_Guidance_Platform.Controllers;

public class HomeController : Controller
{ private readonly IQuestionUserService _questionUserService;

    public HomeController(IQuestionUserService questionUserService)
    {
        _questionUserService = questionUserService;
    }
    private readonly ILogger<HomeController> _logger;
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