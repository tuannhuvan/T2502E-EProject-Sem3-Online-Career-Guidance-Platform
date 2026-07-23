using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("event_registrations")]
    public class EventRegistration
    {
        [Column("event_id")]
        public int EventId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("registered_at")]
        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        [Column("is_vip")]
        public bool IsVip { get; set; } = false;

        [ForeignKey("EventId")]
        public CareerEvent? CareerEvent { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
