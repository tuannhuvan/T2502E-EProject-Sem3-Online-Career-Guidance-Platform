using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Career_Guidance_Platform.Models;

namespace Career_Guidance_Platform.Controllers;

public class AdminController : Controller
{
    private readonly IConfiguration _config;
    private readonly UploadSettings _uploadSettings;

    public AdminController(IConfiguration config)
    {
        _config = config;
        _uploadSettings = new UploadSettings();
        _config.GetSection("UploadSettings").Bind(_uploadSettings);

        // Default folders if not configured
        if (string.IsNullOrWhiteSpace(_uploadSettings.ImageFolder))
            _uploadSettings.ImageFolder = "uploads/images";
        if (string.IsNullOrWhiteSpace(_uploadSettings.AttachmentFolder))
            _uploadSettings.AttachmentFolder = "uploads/attachments";

        // Parse allowed extensions to arrays
        _uploadSettings.AllowedImageExtensionsArray = (_uploadSettings.AllowedImageExtensions ?? "")
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant()).ToArray();

        _uploadSettings.AllowedAttachmentExtensionsArray = (_uploadSettings.AllowedAttachmentExtensions ?? "")
            .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim().ToLowerInvariant()).ToArray();

        // Defaults for sizes
        if (_uploadSettings.MaxImageSize <= 0) _uploadSettings.MaxImageSize = 10 * 1024 * 1024; // 10MB
        if (_uploadSettings.MaxAttachmentSize <= 0) _uploadSettings.MaxAttachmentSize = 50 * 1024 * 1024; // 50MB
    }

    // Existing admin pages (kept)
    public IActionResult Dashboard() => View();
    public IActionResult Users() => View();
    public IActionResult Mentors() => View();
    public IActionResult CareerTests() => View();
    public IActionResult Jobs() => View();
    public IActionResult Resources() => View();
    public IActionResult Reports() => View();
    public IActionResult Settings() => View();
    public IActionResult Terms() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // --- Sample CRUD actions ---

    // GET: Admin/SampleCrud
    public IActionResult SampleCrud()
    {
        var items = new List<SampleCrudModel>
        {
            new()
            {
                Id = 1,
                Name = "Mẫu 1",
                Description = "Mô tả mẫu 1",
                Category = "Category1",
                IsActive = true,
                CreatedDate = DateTime.Now.AddDays(-5)
            }
        };
        return View(items);
    }

    // GET: Admin/SampleCrud/Create
    public IActionResult SampleCrudCreate()
    {
        return View(new SampleCrudModel());
    }

    // POST: Admin/SampleCrud/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SampleCrudCreate(SampleCrudModel model, IFormFile? imageFile, IFormFile? attachmentFile)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            if (imageFile != null)
            {
                model.ImagePath = await SaveUploadedFile(imageFile, _uploadSettings.ImageFolder, _uploadSettings.AllowedImageExtensionsArray, _uploadSettings.MaxImageSize);
            }

            if (attachmentFile != null)
            {
                model.AttachmentPath = await SaveUploadedFile(attachmentFile, _uploadSettings.AttachmentFolder, _uploadSettings.AllowedAttachmentExtensionsArray, _uploadSettings.MaxAttachmentSize);
            }

            model.CreatedDate = DateTime.Now;
            TempData["SuccessMessage"] = "Tạo mới thành công!";
            return RedirectToAction(nameof(SampleCrud));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // GET: Admin/SampleCrud/Edit/1
    public IActionResult SampleCrudEdit(int id)
    {
        var model = new SampleCrudModel
        {
            Id = id,
            Name = "Mẫu 1",
            Description = "Mô tả mẫu 1",
            Category = "Category1",
            IsActive = true,
            CreatedDate = DateTime.Now.AddDays(-5),
            ImagePath = "/uploads/images/sample.jpg"
        };
        return View(model);
    }

    // POST: Admin/SampleCrud/Edit/1
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SampleCrudEdit(int id, SampleCrudModel model, IFormFile? imageFile, IFormFile? attachmentFile)
    {
        if (id != model.Id)
            return NotFound();

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            if (imageFile != null)
            {
                model.ImagePath = await SaveUploadedFile(imageFile, _uploadSettings.ImageFolder, _uploadSettings.AllowedImageExtensionsArray, _uploadSettings.MaxImageSize);
            }

            if (attachmentFile != null)
            {
                model.AttachmentPath = await SaveUploadedFile(attachmentFile, _uploadSettings.AttachmentFolder, _uploadSettings.AllowedAttachmentExtensionsArray, _uploadSettings.MaxAttachmentSize);
            }

            model.UpdatedDate = DateTime.Now;
            TempData["SuccessMessage"] = "Cập nhật thành công!";
            return RedirectToAction(nameof(SampleCrud));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    // Helper: Lưu file vào wwwroot/uploads/...
    private async Task<string> SaveUploadedFile(IFormFile file, string folderRelativePath, string[] allowedExtensions, long maxSize)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File không hợp lệ");

        if (file.Length > maxSize)
            throw new ArgumentException($"Kích thước file vượt quá giới hạn ({maxSize} bytes)");

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (allowedExtensions != null && allowedExtensions.Length > 0 && !allowedExtensions.Contains(fileExtension))
            throw new ArgumentException("Loại file không được hỗ trợ");

        var fileName = $"{Guid.NewGuid()}{fileExtension}";

        // Tạo đường dẫn vật lý: <project-root>/wwwroot/<folderRelativePath>
        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadPath = Path.Combine(uploadsRoot, folderRelativePath.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar));

        if (!Directory.Exists(uploadPath))
            Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về đường dẫn public tương đối để dùng trong <img src=""> hoặc link
        var publicPath = "/" + folderRelativePath.Trim('/').Replace('\\', '/').Replace("wwwroot/", "");
        return $"{publicPath}/{fileName}";
    }

    // Internal class để bind UploadSettings
    private class UploadSettings
    {
        public string? ImageFolder { get; set; }
        public string? AttachmentFolder { get; set; }
        public long MaxImageSize { get; set; }
        public long MaxAttachmentSize { get; set; }
        public string? AllowedImageExtensions { get; set; }
        public string? AllowedAttachmentExtensions { get; set; }

        // computed arrays
        public string[]? AllowedImageExtensionsArray { get; set; }
        public string[]? AllowedAttachmentExtensionsArray { get; set; }
    }
}