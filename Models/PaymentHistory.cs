using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("payment_histories")]
    public class PaymentHistory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Column("paypal_order_id")]
        public string PaypalOrderId { get; set; } = string.Empty;

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("currency")]
        public string Currency { get; set; } = "USD";

        [Column("payment_status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
