using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("skills")]
    public class Skill
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Required]
        [StringLength(50)]
        [Column("skill_type")]
        public string SkillType { get; set; } = "Hard Skill";

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

        public ICollection<CareerPathSkill> CareerPathSkills { get; set; } = new List<CareerPathSkill>();
        public ICollection<UserSkill> UserSkills { get; set; } = new List<UserSkill>();
        public ICollection<Resource> Resources { get; set; } = new List<Resource>();
        public ICollection<CareerStageSkill> CareerStageSkills { get; set; } = new List<CareerStageSkill>();
    }
}
