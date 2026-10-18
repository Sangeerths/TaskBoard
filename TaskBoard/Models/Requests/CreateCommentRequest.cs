using System.ComponentModel.DataAnnotations;

namespace TaskBoard.Models.Requests
{
    public class CreateCommentRequest
    {
        [Required, MaxLength(3000)]
        public string Content { get; set; } = string.Empty;
    }
}
