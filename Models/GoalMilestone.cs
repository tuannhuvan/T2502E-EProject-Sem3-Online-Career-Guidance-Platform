using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("goal_milestones")]
    public class GoalMilestone
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("goal_id")]
        public int GoalId { get; set; }

        [ForeignKey("GoalId")]
        public virtual Goal? Goal { get; set; }

        [Required]
        [StringLength(255)]
        [Column("title")] // Ví dụ: "Đạt chứng chỉ C#", "Hoàn thành Milestone Intern"
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Column("status")] // In Progress, Completed
        public string Status { get; set; } = "In Progress";

        [Column("sequence_order")]
        public int SequenceOrder { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("skill_id")]
        public int? SkillId { get; set; }

        [ForeignKey("SkillId")]
        public virtual Skill? Skill { get; set; }

        [Column("resource_id")]
        public int? ResourceId { get; set; }

        [ForeignKey("ResourceId")]
        public virtual Resource? Resource { get; set; }
    }
}
