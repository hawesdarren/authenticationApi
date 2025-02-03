using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class LoginResponse : BaseResponse
    {
        
        [JsonPropertyName("tempPassword")]
        public bool? tempPassword { get; set; } = false;
        [JsonPropertyName("token")]
        public string? token { get; set; }
        [JsonPropertyName("tfaEnabled")]
        public bool? tfaEnabled { get; set; }
        public LoginResponse() { 
            Success = false;
        }      
                       

    }
}
