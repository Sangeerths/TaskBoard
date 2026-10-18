using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class ReorderColumnsRequest
    {
        [Required]
        [MinLength(1)]
        public List<Guid> ColumnIds { get; set; } = new();
    }
}
