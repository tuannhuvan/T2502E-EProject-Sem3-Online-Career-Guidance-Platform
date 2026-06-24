using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("career_events")]
    public class CareerEvent
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("title")]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("event_date")]
        public DateTime EventDate { get; set; } = DateTime.Now;

        [Required]
        [Column("location")]
        [StringLength(255)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [Column("speaker")]
        [StringLength(100)]
        public string Speaker { get; set; } = string.Empty;

        [Column("registration_url")]
        [StringLength(255)]
        public string RegistrationUrl { get; set; } = string.Empty;
    }
}
