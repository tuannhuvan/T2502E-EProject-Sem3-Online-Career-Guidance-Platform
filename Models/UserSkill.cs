using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("user_skills")]
    public class UserSkill
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("skill_id")]
        public int SkillId { get; set; }

        [Required]
        [Column("proficiency_level")]
        [StringLength(50)]
        public string ProficiencyLevel { get; set; } = "Beginner"; // Beginner, Intermediate, Advanced

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Acquired"; // Acquired, Learning, Gap, In-Goals, Completed

        [Column("start_timestamp")]
        public DateTime? StartTimestamp { get; set; }

        [Column("cooldown_until")]
        public DateTime? CooldownUntil { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("SkillId")]
        public Skill? Skill { get; set; }
    }
}
