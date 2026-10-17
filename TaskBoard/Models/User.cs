using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class User
    {
     
            [Key]
            public Guid UserId { get; set; }

            [Required]
            [MaxLength(255)]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string PasswordHash { get; set; } = string.Empty;

            public bool IsActive { get; set; } = true;

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

            // One User → One Profile
            public UserProfile? UserProfile { get; set; }

        public ICollection<ProjectMember> ProjectMemberships { get; set; }
    = new List<ProjectMember>();

    }
}
