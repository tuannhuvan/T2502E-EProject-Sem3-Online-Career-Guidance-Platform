using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("career_stages")]
    public class CareerStage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("career_path_id")]
        public int CareerPathId { get; set; }

        [ForeignKey("CareerPathId")]
        public virtual CareerPath? CareerPath { get; set; }

        [Required]
        [StringLength(100)]
        [Column("title")] // Intern, Junior, Mid-Senior, Senior
        public string Title { get; set; } = string.Empty;

        [Column("description", TypeName = "text")]
        public string? Description { get; set; }

        [Column("sequence_order")] // Thứ tự giai đoạn (1, 2, 3, 4)
        public int SequenceOrder { get; set; }

        public virtual ICollection<CareerStageSkill> CareerStageSkills { get; set; } = new List<CareerStageSkill>();
    }
}
