using Career_Guidance_Platform.Data;
using Career_Guidance_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Career_Guidance_Platform.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ResourceController : Controller
    {
        private readonly AppDbContext _context;

        public ResourceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Resource
        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.Resources
                .Include(r => r.Category)
                .Include(r => r.ParentResource)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(r => r.CategoryId == categoryId);
            }

            var resources = await query
                .Where(r => r.Status == 1)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories
                .Where(c => c.Status == 1)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.SelectedCategoryId = categoryId;
            return View(resources);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdownData();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Resource resource)
        {
            ValidateResource(resource);

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    resource.CreatedBy = userId ?? "System";
                    resource.CreatedAt = DateTime.Now;
                    resource.Status = 1;
                    resource.PathId = 0;

                    _context.Add(resource);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Lưu resource thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi lưu resource: {ex.Message}");
                }
            }

            await LoadDropdownData();
            return View(resource);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var resource = await _context.Resources.FindAsync(id);
            if (resource == null) return NotFound();

            await LoadDropdownData(resource.Id);
            return View(resource);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Resource resource)
        {
            if (id != resource.Id) return NotFound();

            ValidateResource(resource);

            if (ModelState.IsValid)
            {
                try
                {
                    var existingResource = await _context.Resources.FindAsync(id);
                    if (existingResource == null) return NotFound();

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    
                    existingResource.Title = resource.Title;
                    existingResource.Description = resource.Description;
                    existingResource.ResourceType = resource.ResourceType;
                    existingResource.Url = resource.Url;
                    existingResource.CategoryId = resource.CategoryId;
                    existingResource.ParentResourceId = resource.ParentResourceId;
                    existingResource.UpdatedAt = DateTime.Now;
                    existingResource.UpdatedBy = userId ?? "System";

                    _context.Update(existingResource);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Cập nhật resource thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ResourceExists(resource.Id)) return NotFound();
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi cập nhật resource: {ex.Message}");
                }
            }

            await LoadDropdownData(resource.Id);
            return View(resource);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var resource = await _context.Resources
                .Include(r => r.Category)
                .Include(r => r.ParentResource)
                .Include(r => r.ChildResources)
                .FirstOrDefaultAsync(m => m.Id == id);

            return resource == null ? NotFound() : View(resource);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = await _context.Resources
                .Include(r => r.ChildResources)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (resource != null)
            {
                if (resource.ChildResources.Any())
                {
                    TempData["ErrorMessage"] = "Không thể xóa resource này vì nó có các resource con liên kết. Vui lòng xóa các resource con trước.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa resource thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ResourceExists(int id) => _context.Resources.Any(e => e.Id == id);

        private void ValidateResource(Resource resource)
        {
            if (string.IsNullOrWhiteSpace(resource.Title))
            {
                ModelState.AddModelError(nameof(resource.Title), "Tên resource là bắt buộc");
            }

            if (string.IsNullOrWhiteSpace(resource.ResourceType))
            {
                ModelState.AddModelError(nameof(resource.ResourceType), "Loại resource là bắt buộc");
            }
        }

        private async Task LoadDropdownData(int? currentResourceId = null)
        {
            ViewBag.Categories = new SelectList(
                await _context.Categories
                    .Where(c => c.Status == 1)
                    .OrderBy(c => c.Name)
                    .ToListAsync(),
                "Id",
                "Name"
            );

            var parentResources = await _context.Resources
                .Where(r => r.Status == 1 && (currentResourceId == null || r.Id != currentResourceId))
                .OrderBy(r => r.Title)
                .ToListAsync();

            ViewBag.ParentResources = new SelectList(parentResources, "Id", "Title");
        }
    }
}