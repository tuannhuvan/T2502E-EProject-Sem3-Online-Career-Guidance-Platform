using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("career_path_skills")]
    public class CareerPathSkill
    {
        [Column("career_path_id")]
        public int CareerPathId { get; set; }

        [Column("skill_id")]
        public int SkillId { get; set; }

        [Required]
        [Column("importance_level")]
        [StringLength(50)]
        public string ImportanceLevel { get; set; } = "Required"; // Required, Preferred, Optional

        [ForeignKey("CareerPathId")]
        public CareerPath? CareerPath { get; set; }

        [ForeignKey("SkillId")]
        public Skill? Skill { get; set; }
    }
}
