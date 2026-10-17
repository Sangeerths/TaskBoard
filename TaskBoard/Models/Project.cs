using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{

        public class Project
        {
            [Key]
            public Guid ProjectId { get; set; } = Guid.NewGuid();

            [Required]
            [MaxLength(150)]
            public string Name { get; set; } = string.Empty;

            [MaxLength(1000)]
            public string? Description { get; set; }

            [Required]
            public Guid OwnerId { get; set; }

            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

            // Navigation properties
            public User Owner { get; set; } = null!;

            public ICollection<ProjectMember> Members { get; set; }
                = new List<ProjectMember>();
        }
    }


