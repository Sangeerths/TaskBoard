using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class RenameColumnRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
    }
}
