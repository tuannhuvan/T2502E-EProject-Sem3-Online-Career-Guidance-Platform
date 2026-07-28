using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Dtos;
using Career_Guidance_Platform.Dtos.Question;
using Career_Guidance_Platform.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Career_Guidance_Platform.Controllers;

public class QuestionController : Controller
{
    private readonly IQuestionService _service;
    private readonly AppDbContext _context;

    public QuestionController(IQuestionService service, AppDbContext context)
    {
        _service = service;
        _context = context;
    }
   
    
    
    // ================= INDEX =================
    public async Task<IActionResult> Index()
    {
        var data = await _service.GetAllAsync();
        return View(data);
    }

    // ================= CREATE =================
    public IActionResult Create()
    {
        ViewBag.Tests = new SelectList(
            _context.CareerTests,
            "Id",
            "Title"
        );
        ViewBag.QuestionTypes = new SelectList(
            _context.QuestionTypes,
            "Id",
            "Title"
        );

        ViewBag.CareerPaths = new SelectList(
            _context.CareerPaths,
            "Id",
            "Title"
        );
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(QuestionFullCreateDto dto)
    {
        await _service.CreateAsync(dto);
        return RedirectToAction("Index");
    }

    // ================= EDIT =================
    public async Task<IActionResult> Edit(int id)
    {
        var data = await _service.GetByIdAsync(id);

        if (data == null)
            return NotFound();

        ViewBag.Tests = new SelectList(
            _context.CareerTests,
            "Id",
            "Title",
            data.CareerTestId
        );

        ViewBag.QuestionTypes = new SelectList(
            _context.QuestionTypes,
            "Id",
            "Title",
            data.QuestionTypeId
        );

        ViewBag.CareerPaths = new SelectList(
            _context.CareerPaths,
            "Id",
            "Title"
        );

        return View(data);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(QuestionFullDto dto)
    {
        await _service.UpdateAsync(dto);
        return RedirectToAction("Index");
    }

    // ================= DELETE =================
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return RedirectToAction("Index");
    }

    // ================= DETAILS =================
    public async Task<IActionResult> Details(int id)
    {
        var data = await _service.GetByIdAsync(id);
        return View(data);
    }
}