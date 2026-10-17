using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class AddProjectMemberRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Admin|Member)$")]
        public string Role { get; set; } = "Member";
    }
}
