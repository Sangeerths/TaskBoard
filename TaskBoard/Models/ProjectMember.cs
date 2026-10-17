using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class ProjectMember
    {
        [Required]
        public Guid ProjectId { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Project Project { get; set; } = null!;

        public User User { get; set; } = null!;
    }
}
