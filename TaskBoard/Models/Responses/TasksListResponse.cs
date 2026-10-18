namespace TaskBoard.Models.Responses
{
    public class TasksListResponse : ResponseBase
    {
        public List<TaskResponse> Tasks { get; set; } = new();

        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
    }
}
