using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("mentor_reviews")]
    public class MentorReview
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("mentor_id")]
        public int MentorId { get; set; }

        [Column("mentee_id")]
        public int MenteeId { get; set; }

        [Column("meeting_id")]
        public int MeetingId { get; set; }

        [Column("rating")]
        public int Rating { get; set; } // Thang điểm 1-5 sao

        [Column("comment")]
        public string? Comment { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("MentorId")]
        public User? Mentor { get; set; }

        [ForeignKey("MenteeId")]
        public User? Mentee { get; set; }

        [ForeignKey("MeetingId")]
        public MentorshipMeeting? Meeting { get; set; }
    }
}
