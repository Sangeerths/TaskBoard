using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class ActivityLog
    {
        [Key]
        public Guid ActivityId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectId { get; set; }

        // Nullable so history can survive task deletion.
        public Guid? TaskId { get; set; }

        // Nullable so history can survive account deletion.
        public Guid? ActorId { get; set; }

        [Required, MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;

        public TaskItem? Task { get; set; }

        public User? Actor { get; set; }
    }
}
