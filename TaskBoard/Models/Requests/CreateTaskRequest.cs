using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class CreateTaskRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Required]
        [RegularExpression(
            "^(Low|Medium|High|Critical)$",
            ErrorMessage = "Priority must be Low, Medium, High, or Critical.")]
        public string Priority { get; set; } = "Medium";

        [RegularExpression("^(Started|In Progress|On Hold|Completed)$", ErrorMessage = "Status must be Started, In Progress, On Hold, or Completed.")]
        public string Status { get; set; } = "Started";

        public DateTime? DueDate { get; set; }

        public Guid? AssigneeId { get; set; }
    }
}
