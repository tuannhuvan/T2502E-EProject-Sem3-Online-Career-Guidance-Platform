using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;
[Table("test_answers")]
public class TestAnswer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("test_result_id")]
    public int TestResultId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("question_option_id")]
    public int QuestionOptionId { get; set; }

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

    [ForeignKey("TestResultId")]
    public TestResult? TestResult { get; set; }

    [ForeignKey("QuestionId")]
    public QuestionTest? QuestionTest { get; set; }

    [ForeignKey("QuestionOptionId")]
    public QuestionOption? QuestionOption { get; set; }
}