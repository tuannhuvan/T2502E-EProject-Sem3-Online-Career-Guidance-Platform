using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Models;

public class User : IdentityUser
{
    // Thêm các trường tùy chỉnh
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public ICollection<AssessmentResult> AssessmentResults { get; set; }
}