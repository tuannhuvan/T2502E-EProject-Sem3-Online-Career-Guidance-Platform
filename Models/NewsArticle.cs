using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("news_articles")]
    public class NewsArticle
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("summary")]
        public string Summary { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("author")]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Column("published_date")]
        public DateTime PublishedDate { get; set; } = DateTime.Now;

        [Required]
        [Column("category")]
        [StringLength(100)]
        public string Category { get; set; } = string.Empty;

        [Column("image_url")]
        [StringLength(255)]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
