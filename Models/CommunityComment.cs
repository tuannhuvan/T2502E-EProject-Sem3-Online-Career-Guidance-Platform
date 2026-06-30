using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("community_comments")]
    public class CommunityComment
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("post_id")]
        public int PostId { get; set; }

        [Column("author_id")]
        public int AuthorId { get; set; }

        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("PostId")]
        public CommunityPost? CommunityPost { get; set; }

        [ForeignKey("AuthorId")]
        public User? Author { get; set; }
    }
}
