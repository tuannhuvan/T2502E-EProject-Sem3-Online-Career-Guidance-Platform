using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("mentorship_requests")]
    public class MentorshipRequest
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("mentee_id")]
        public int MenteeId { get; set; }

        [Column("mentor_id")]
        public int MentorId { get; set; }

        [Required]
        [Column("message")]
        public string Message { get; set; } = string.Empty;

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected, Terminated

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MenteeId")]
        public User? Mentee { get; set; }

        [ForeignKey("MentorId")]
        public User? Mentor { get; set; }
    }
}
