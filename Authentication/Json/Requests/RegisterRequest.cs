namespace Authentication.Json.Requests
{
    public class RegisterRequest
    {
        public required string email { get; set; }
        public required string password { get; set; }
        public required string renteredPassword { get; set; }

    }
}
