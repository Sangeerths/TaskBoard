using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class TaskFilterRequest
    {
        public Guid? AssigneeId { get; set; }

        [RegularExpression("^(Low|Medium|High|Critical)$")]
        public string? Priority { get; set; }

        public DateTime? DueDateFrom { get; set; }

        public DateTime? DueDateTo { get; set; }

        public string? SearchText { get; set; }

        [Range(1, int.MaxValue)]
        public int PageNumber { get; set; } = 1;

        [Range(1, 100)]
        public int PageSize { get; set; } = 10;
    }
}
