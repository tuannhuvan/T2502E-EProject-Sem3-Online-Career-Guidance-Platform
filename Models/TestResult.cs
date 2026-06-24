using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("test_results")]
public class TestResult
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int? UserId { get; set; }

    [Column("test_id")]
    public int TestId { get; set; }

    [Column("recommended_career_path_id")]
    public int? RecommendedCareerPathId { get; set; }

    [Column("compatibility_score", TypeName = "decimal(5,2)")]
    public decimal? CompatibilityScore { get; set; }

    [Column("attempt_number")]
    public int AttemptNumber { get; set; } = 1;

    [Column("date_taken")]
    public DateTime DateTaken { get; set; } = DateTime.Now;

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

    [ForeignKey("UserId")]
    public User? User { get; set; }

    [ForeignKey("TestId")]
    public Test? Test { get; set; }

    [ForeignKey("RecommendedCareerPathId")]
    public CareerPath? RecommendedCareerPath { get; set; }

    public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
}