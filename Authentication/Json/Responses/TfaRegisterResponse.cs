using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class TfaRegisterResponse : BaseResponse
    {
        [JsonPropertyName("keyUri")]
        public string keyUri {  get; set; }
    }
}
