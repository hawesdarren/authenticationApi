namespace Authentication.Json.Requests
{
    public class ChangePasswordRequest
    {
        public required string password { get; set; }
        public required string confirmPassword { get; set; }
    }
}
