using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("employer_reviews")]
    public class EmployerReview
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Required]
        [Column("company_name")]
        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [Column("job_title")]
        [StringLength(100)]
        public string? JobTitle { get; set; }

        [Column("overall_rating")]
        public int OverallRating { get; set; } = 5;

        [Column("culture_rating")]
        public int CultureRating { get; set; } = 5;

        [Column("work_life_balance_rating")]
        public int WorkLifeBalanceRating { get; set; } = 5;

        [Column("career_growth_rating")]
        public int CareerGrowthRating { get; set; } = 5;

        [Required]
        [Column("review_content")]
        public string ReviewContent { get; set; } = string.Empty;

        [Column("pros")]
        public string? Pros { get; set; }

        [Column("cons")]
        public string? Cons { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
