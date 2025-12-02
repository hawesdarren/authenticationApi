using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class TfaSetupResponse : BaseResponse
    {
        [JsonPropertyName("keyUri")]
        public string keyUri {  get; set; }
    }
}
