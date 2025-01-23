using Authentication.Json.Enums;
using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    [JsonDerivedType(typeof(BaseResponse), typeDiscriminator: "base")]
    [JsonDerivedType(typeof(LoginResponse), typeDiscriminator: "loginResponse")]
    public class BaseResponse : ErrorEnums
    {
        [JsonPropertyName("success")]
        public required bool Success { get; set; } = false;
        [JsonPropertyName("authenticated")]
        public required bool Authenticated { get; set; } = false;
        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? error { get; set; }
                  
        
        public void SetError(Error newError)
        {
            error = newError.ToString();
            
        }

        public string? GetError()
        {
            return error;
        }
    }
}
