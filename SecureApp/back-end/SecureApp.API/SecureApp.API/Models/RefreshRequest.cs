namespace SecureApp.API.Models
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
    }
}
