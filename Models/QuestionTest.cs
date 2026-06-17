

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("question_tests")]
public class QuestionTest
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("test_id")]
    public int TestId { get; set; }

    [Column("question_type_id")]
    public int QuestionTypeId { get; set; }

    [Required]
    [Column("content")]
    public string Content { get; set; } = string.Empty;

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

    [ForeignKey("TestId")]
    public Test? Test { get; set; }

    [ForeignKey("QuestionTypeId")]
    public QuestionType? QuestionType { get; set; }

    public ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
}