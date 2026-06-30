using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("group_mentoring_registrations")]
    public class GroupMentoringRegistration
    {
        [Column("session_id")]
        public int SessionId { get; set; }

        [Column("student_id")]
        public int StudentId { get; set; }

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [ForeignKey("SessionId")]
        public GroupMentoringSession? Session { get; set; }

        [ForeignKey("StudentId")]
        public User? Student { get; set; }
    }
}
