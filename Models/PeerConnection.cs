using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("peer_connections")]
    public class PeerConnection
    {
        [Column("requester_id")]
        public int RequesterId { get; set; }

        [Column("receiver_id")]
        public int ReceiverId { get; set; }

        [Required]
        [Column("status")]
        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Declined, Blocked

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [Column("connected_at")]
        public DateTime? ConnectedAt { get; set; }

        [ForeignKey("RequesterId")]
        public User? Requester { get; set; }

        [ForeignKey("ReceiverId")]
        public User? Receiver { get; set; }
    }
}
