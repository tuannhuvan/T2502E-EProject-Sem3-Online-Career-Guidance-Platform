using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("career_stage_skills")]
    public class CareerStageSkill
    {
        [Column("career_stage_id")]
        public int CareerStageId { get; set; }

        [ForeignKey("CareerStageId")]
        public virtual CareerStage? CareerStage { get; set; }

        [Column("skill_id")]
        public int SkillId { get; set; }

        [ForeignKey("SkillId")]
        public virtual Skill? Skill { get; set; }

        [StringLength(50)]
        [Column("proficiency_required")] // Basic, Intermediate, Advanced
        public string? ProficiencyRequired { get; set; }
    }
}
