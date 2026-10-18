namespace TaskBoard.Models.Responses
{
    public class ActivityItemResponse
    {
        public Guid ActivityId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid? TaskId { get; set; }
        public Guid? ActorId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ActivityListResponse : ResponseBase
    {
        public List<ActivityItemResponse> Activities { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
