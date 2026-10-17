namespace TaskBoard.Models.Responses
{
    public class ProjectMemberResponse:ResponseBase
    {
        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
