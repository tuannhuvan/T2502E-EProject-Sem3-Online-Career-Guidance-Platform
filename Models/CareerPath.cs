

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("career_paths")]
public class CareerPath
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string? Content { get; set; }

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

    [ForeignKey("CategoryId")]
    public Category? Category { get; set; }

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; } = new List<OptionCareerPath>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
}