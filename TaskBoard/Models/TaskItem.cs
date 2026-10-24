using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class TaskItem
    {
        [Key]
        public Guid TaskId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ColumnId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required, MaxLength(20)]
        public string Priority { get; set; } = "Medium";

        [Required, MaxLength(20)]
        public string Status { get; set; } = "Started";

        public DateTime? DueDate { get; set; }

        public Guid? AssigneeId { get; set; }

        public int Position { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public BoardColumn Column { get; set; } = null!;

        public User? Assignee { get; set; }
    }
}
