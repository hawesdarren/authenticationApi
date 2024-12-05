using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class RegisterResponse : BaseResponse
    {
        [JsonPropertyName("token")]
        public string? token { get; set; }

        [SetsRequiredMembers]
        public RegisterResponse()
        {
            Success = false;
        }
    }
}
