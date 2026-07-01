using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("mentorship_meetings")]
    public class MentorshipMeeting
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("mentee_id")]
        public int MenteeId { get; set; }

        [Column("mentor_id")]
        public int MentorId { get; set; }

        [Required]
        [Column("title")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("scheduled_time")]
        public DateTime ScheduledTime { get; set; }

        [Column("meeting_url")]
        [StringLength(255)]
        public string? MeetingUrl { get; set; }

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Scheduled"; // Scheduled, Completed, Cancelled

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MenteeId")]
        public User? Mentee { get; set; }

        [ForeignKey("MentorId")]
        public User? Mentor { get; set; }
    }
}
