namespace Authentication.Json.Requests
{
    public class EnableTfa
    {
        public bool enableTfa {  get; set; }
        public string totp { get; set; }
    }
}
