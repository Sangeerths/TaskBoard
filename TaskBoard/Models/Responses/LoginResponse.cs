namespace TaskBoard.Models.Responses
{
    public class LoginResponse: ResponseBase
    {
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
