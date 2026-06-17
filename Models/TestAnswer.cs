
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;
[Table("test_answers")]
public class TestAnswer
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("result_id")]
    public int ResultId { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

    [Column("option_id")]
    public int OptionId { get; set; }

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

    [ForeignKey("ResultId")]
    public TestResult? TestResult { get; set; }

    [ForeignKey("QuestionId")]
    public QuestionTest? QuestionTest { get; set; }

    [ForeignKey("OptionId")]
    public QuestionOption? QuestionOption { get; set; }
}