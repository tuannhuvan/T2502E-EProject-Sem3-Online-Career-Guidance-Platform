
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("option_career_paths")]
public class OptionCareerPath
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("option_id")]
    public int OptionId { get; set; }

    [Column("career_path_id")]
    public int CareerPathId { get; set; }

    [Column("weight")]
    public int Weight { get; set; } = 1;

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

    [ForeignKey("OptionId")]
    public QuestionOption? QuestionOption { get; set; }

    [ForeignKey("CareerPathId")]
    public CareerPath? CareerPath { get; set; }
}