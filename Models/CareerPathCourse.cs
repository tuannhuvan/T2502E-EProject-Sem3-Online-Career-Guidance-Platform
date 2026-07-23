using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("career_path_courses")]
    public class CareerPathCourse
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("career_path_id")]
        public int CareerPathId { get; set; }

        [Required]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("estimated_days")]
        public int EstimatedDays { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("status")]
        public int Status { get; set; } = 1;

        [ForeignKey(nameof(CareerPathId))]
        public CareerPath? CareerPath { get; set; }
    }
}