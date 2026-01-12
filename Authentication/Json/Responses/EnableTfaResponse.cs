using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class EnableTfaResponse : BaseResponse
    {
        [JsonPropertyName("token")]
        public string? token { get; set; }

        [JsonPropertyName("refreshToken")]
        public string? refreshToken { get; set; }

        public EnableTfaResponse()
        {
            Success = false;
        }
    }
}
