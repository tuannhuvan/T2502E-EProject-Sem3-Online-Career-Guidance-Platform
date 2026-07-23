
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("job_postings")]
public class JobPosting
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("career_path_id")]
    public int? CareerPathId { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("company_name")]
    public string? CompanyName { get; set; }

    [Column("job_type")]
    public string JobType { get; set; } = "FullTime";

    [Column("location")]
    public string? Location { get; set; }

    [Column("experience_level")]
    public string? ExperienceLevel { get; set; }

    [Column("application_url")]
    public string? ApplicationUrl { get; set; }

    [Column("salary", TypeName = "decimal(18,2)")]
    public decimal Salary { get; set; } = 0;

    [Column("description")]
    public string? Description { get; set; }

    [Column("expired_at")]
    public DateTime? ExpiredAt { get; set; }

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

    [ForeignKey("CareerPathId")]
    public CareerPath? CareerPath { get; set; }

    public ICollection<SavedJob> SavedJobs { get; set; } = new List<SavedJob>();
    public ICollection<JobApplication> JobApplications { get; set; } = new List<JobApplication>();
}