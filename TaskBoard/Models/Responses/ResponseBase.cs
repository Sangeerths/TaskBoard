namespace TaskBoard.Models.Responses
{
    public class ResponseBase
    {
        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
