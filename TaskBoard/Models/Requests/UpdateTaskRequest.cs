using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class UpdateTaskRequest
    {
        [MaxLength(200)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [RegularExpression(
            "^(Low|Medium|High|Critical)$",
            ErrorMessage = "Priority must be Low, Medium, High, or Critical.")]
        public string? Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public Guid? AssigneeId { get; set; }
    }
}
