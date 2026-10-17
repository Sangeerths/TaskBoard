using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class UpdateProjectRequest
    {
        
        [MaxLength(150)]
        public string? Name { get; set; } 

        [MaxLength(1000)]
        public string? Description { get; set; }
    }
}
