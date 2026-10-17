using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class UpdateProjectMemberRequest
    {
        [Required]
        [RegularExpression("^(Admin|Member)$")]
        public string Role { get; set; } = string.Empty;
    }
}
