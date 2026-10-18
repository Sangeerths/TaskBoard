using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models
{
    public class BoardColumn
    {
        [Key]
        public Guid ColumnId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int Position { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project Project { get; set; } = null!;

        public ICollection<TaskItem> Tasks { get; set; }
            = new List<TaskItem>();
    }
}
