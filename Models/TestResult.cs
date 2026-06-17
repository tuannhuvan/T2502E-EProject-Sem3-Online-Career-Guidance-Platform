
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("test_results")]
public class TestResult
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("test_id")]
    public int TestId { get; set; }

    [Column("recommended_career_path_id")]
    public int? RecommendedCareerPathId { get; set; }

    [Column("compatibility_score", TypeName = "decimal(5,2)")]
    public decimal? CompatibilityScore { get; set; }

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

    [ForeignKey("TestId")]
    public Test? Test { get; set; }

    [ForeignKey("RecommendedCareerPathId")]
    public CareerPath? RecommendedCareerPath { get; set; }

    public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
}