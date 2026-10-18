using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class MoveTaskRequest
    {
        [Required]
        public Guid ColumnId { get; set; }

        [Range(0, int.MaxValue)]
        public int Position { get; set; }
    }
}
