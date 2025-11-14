using System.Text.Json.Serialization

namespace Authentication.Json.Responses
{
    public class RefreshTokenResponse : BaseResponse
    {
        [JsonPropertyName("token")]
        public string token { get; set; }
    }
}
