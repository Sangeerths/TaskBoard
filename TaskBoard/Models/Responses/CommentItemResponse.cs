namespace TaskBoard.Models.Responses
{
    public class CommentItemResponse
    {
        public Guid CommentId { get; set; }
        public Guid TaskId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CommentsListResponse : ResponseBase
    {
        public List<CommentItemResponse> Comments { get; set; } = new();
    }

    public class CommentResponse : ResponseBase
    {
        public CommentItemResponse? Comment { get; set; }
    }
}
