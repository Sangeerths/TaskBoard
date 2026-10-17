using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class CreateProjectRequest
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
