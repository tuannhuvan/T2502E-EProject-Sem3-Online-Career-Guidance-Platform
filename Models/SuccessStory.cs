using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("success_stories")]
    public class SuccessStory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("career_path_id")]
        public int CareerPathId { get; set; }

        [Required]
        [Column("professional_name")]
        [StringLength(100)]
        public string ProfessionalName { get; set; } = string.Empty;

        [Required]
        [Column("job_title")]
        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Column("company_name")]
        [StringLength(100)]
        public string? CompanyName { get; set; }

        [Required]
        [Column("story_content", TypeName = "longtext")]
        public string StoryContent { get; set; } = string.Empty;

        [Column("image_url")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        [Column("linkedin_url")]
        [StringLength(255)]
        public string? LinkedInUrl { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CareerPathId")]
        public CareerPath? CareerPath { get; set; }
    }
}
