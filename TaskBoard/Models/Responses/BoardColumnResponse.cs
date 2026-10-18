namespace TaskBoard.Models.Responses
{
    public class BoardColumnResponse 
    {
        public Guid ColumnId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }

        public List<BoardTaskResponse> Tasks { get; set; }
            = new();
    }
}
