namespace TaskBoard.Models.Responses
{
    public class BoardResponse: ResponseBase
    {
        public Guid ProjectId { get; set; }

        public List<BoardColumnResponse> Columns { get; set; }
            = new();
    }
}
