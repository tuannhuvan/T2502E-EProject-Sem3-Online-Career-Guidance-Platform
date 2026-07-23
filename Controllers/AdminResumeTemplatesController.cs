using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminResumeTemplatesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminResumeTemplatesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /AdminResumeTemplates
        public async Task<IActionResult> Index()
        {
            var templates = await _context.ResumeTemplates.OrderByDescending(t => t.Id).ToListAsync();
            return View("~/Views/Admin/ResumeTemplates/Index.cshtml", templates);
        }

        // GET: /AdminResumeTemplates/Create
        public IActionResult Create()
        {
            return View("~/Views/Admin/ResumeTemplates/Create.cshtml");
        }

        // POST: /AdminResumeTemplates/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResumeTemplate template, IFormFile? thumbnailFile)
        {
            if (ModelState.IsValid)
            {
                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "templates");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(fileStream);
                    }
                    template.ThumbnailUrl = "/uploads/templates/" + fileName;
                }

                template.CreatedAt = DateTime.Now;
                _context.ResumeTemplates.Add(template);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ResumeTemplates/Create.cshtml", template);
        }

        // GET: /AdminResumeTemplates/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var template = await _context.ResumeTemplates.FindAsync(id);
            if (template == null)
            {
                return NotFound();
            }
            return View("~/Views/Admin/ResumeTemplates/Edit.cshtml", template);
        }

        // POST: /AdminResumeTemplates/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ResumeTemplate template, IFormFile? thumbnailFile)
        {
            if (id != template.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var dbTemplate = await _context.ResumeTemplates.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
                    if (dbTemplate == null) return NotFound();

                    template.ThumbnailUrl = dbTemplate.ThumbnailUrl; // keep old
                    template.CreatedAt = dbTemplate.CreatedAt;

                    if (thumbnailFile != null && thumbnailFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "templates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await thumbnailFile.CopyToAsync(fileStream);
                        }
                        template.ThumbnailUrl = "/uploads/templates/" + fileName;
                    }

                    _context.Update(template);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.ResumeTemplates.Any(e => e.Id == template.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Admin/ResumeTemplates/Edit.cshtml", template);
        }

        // POST: /AdminResumeTemplates/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var template = await _context.ResumeTemplates.FindAsync(id);
            if (template != null)
            {
                _context.ResumeTemplates.Remove(template);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminResumeTemplates/ToggleActive/5
        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var template = await _context.ResumeTemplates.FindAsync(id);
            if (template != null)
            {
                template.IsActive = !template.IsActive;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
