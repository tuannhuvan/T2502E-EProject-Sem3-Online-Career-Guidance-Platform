
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Career_Guidance_Platform.Models;

[Table("goals")]
public class Goal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("career_path_id")]
    public int? CareerPathId { get; set; }

    [Required]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("progress")]
    public int Progress { get; set; } = 0;

    [Column("target_date")]
    public DateTime? TargetDate { get; set; }

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

    [ForeignKey("StudentId")]
    public User? Student { get; set; }

    [ForeignKey("CareerPathId")]
    public CareerPath? CareerPath { get; set; }
}