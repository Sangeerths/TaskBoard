namespace TaskBoard.Models.Responses
{
    public class ProjectResponse: ResponseBase
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
