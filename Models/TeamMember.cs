using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Career_Guidance_Platform.Models
{
    [Table("team_members")]
    public class TeamMember
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("role")]
        [StringLength(100)]
        public string Role { get; set; } = string.Empty;

        [Column("avatar_url")]
        [StringLength(255)]
        public string AvatarUrl { get; set; } = string.Empty;

        [Column("bio")]
        public string Bio { get; set; } = string.Empty;

        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
