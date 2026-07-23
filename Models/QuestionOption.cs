namespace Career_Guidance_Platform.Models;

public class QuestionOption
{
    public int Id { get; set; }

    public int QuestionId { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

    public int Status { get; set; } = 1;

    // NAVIGATION
    public Question Question { get; set; } = null!;

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; }
        = new List<OptionCareerPath>();

  
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;
[Table("question_options")]
public class QuestionOption
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }

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

    [ForeignKey("QuestionId")]
    public QuestionTest? QuestionTest { get; set; }

    public ICollection<OptionCareerPath> OptionCareerPaths { get; set; } = new List<OptionCareerPath>();
    public ICollection<TestAnswer> TestAnswers { get; set; } = new List<TestAnswer>();
}