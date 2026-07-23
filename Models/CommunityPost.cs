using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("community_posts")]
    public class CommunityPost
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Required]
        [Column("author_name")]
        [StringLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [Column("author_id")]
        public int? AuthorId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("likes_count")]
        public int LikesCount { get; set; } = 0;

        [Column("replies_count")]
        public int RepliesCount { get; set; } = 0;

        [Required]
        [Column("category")]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }

        public ICollection<CommunityComment> Comments { get; set; } = new List<CommunityComment>();
    }
}
