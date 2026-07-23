using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models;

[Table("goals")]
public class Goal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("student_id")]
    public int StudentId { get; set; }

    [Column("career_path_id")]
    public int? CareerPathId { get; set; }

    [Required(ErrorMessage = "Tên mục tiêu không được để trống")]
    [StringLength(255, ErrorMessage = "Tên mục tiêu không được vượt quá 255 ký tự")]
    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại mục tiêu không được để trống")]
    [StringLength(50)]
    [Column("goal_type")]
    public string GoalType { get; set; } = "ShortTerm";

    [Range(0, 100, ErrorMessage = "Tiến độ phải từ 0 đến 100")]
    [Column("progress")]
    public int Progress { get; set; } = 0;

    [DataType(DataType.Date)]
    [Column("target_date")]
    public DateTime? TargetDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("created_by")]
    [StringLength(255)]
    public string CreatedBy { get; set; } = "System";

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("updated_by")]
    [StringLength(255)]
    public string? UpdatedBy { get; set; }

    [Column("status")]
    public int Status { get; set; } = 1;

    [ForeignKey(nameof(StudentId))]
    public User? Student { get; set; }

    [ForeignKey(nameof(CareerPathId))]
    public CareerPath? CareerPath { get; set; }

    public ICollection<GoalMilestone> GoalMilestones { get; set; } = new List<GoalMilestone>();
}