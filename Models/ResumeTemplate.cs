using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("resume_templates")]
    public class ResumeTemplate
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column("thumbnail_url")]
        [StringLength(255)]
        public string? ThumbnailUrl { get; set; }

        [Required]
        [Column("template_code")]
        [StringLength(100)]
        public string TemplateCode { get; set; } = "Default";

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("is_premium")]
        public bool IsPremium { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    }
}
