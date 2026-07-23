namespace Career_Guidance_Platform.Models;

public class CareerPath
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Status { get; set; } = 1;

    // NAVIGATION
    public Category Category { get; set; } = null!;

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; }
        = new List<OptionCareerPath>();

    public ICollection<AssessmentResult> AssessmentResults { get; set; }
        = new List<AssessmentResult>();

    
   


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

    [Column("parent_path_id")]
    public int? ParentPathId { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("content")]
    public string? Content { get; set; }

    [Column("salary_min", TypeName = "decimal(18,2)")]
    public decimal SalaryMin { get; set; } = 0;

    [Column("salary_max", TypeName = "decimal(18,2)")]
    public decimal SalaryMax { get; set; } = 0;

    [Column("job_outlook")]
    public double JobOutlook { get; set; } = 0;

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

    [ForeignKey("ParentPathId")]
    public CareerPath? ParentPath { get; set; }

    public ICollection<CareerPath> ChildPaths { get; set; } = new List<CareerPath>();
    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; } = new List<OptionCareerPath>();
    public ICollection<TestResult> TestResults { get; set; } = new List<TestResult>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
    public ICollection<JobPosting> JobPostings { get; set; } = new List<JobPosting>();
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();
    public ICollection<CareerPathSkill> CareerPathSkills { get; set; } = new List<CareerPathSkill>();
    public ICollection<SuccessStory> SuccessStories { get; set; } = new List<SuccessStory>();
    public ICollection<TestResultScore> TestResultScores { get; set; } = new List<TestResultScore>();
}