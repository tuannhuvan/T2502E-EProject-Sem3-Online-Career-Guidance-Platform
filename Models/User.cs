using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace Career_Guidance_Platform.Models;

[Table("users")]
public class User : IdentityUser<int>
{
    [Required]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Column("role")]
    public string Role { get; set; } = "Student";

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    public string CreatedBy { get; set; } = "System";

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    public string? UpdatedBy { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;

    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}