using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Authentication.Json.Responses
{
    public class RegisterResponse : BaseResponse
    {
        
        [SetsRequiredMembers]
        public RegisterResponse()
        {
            Success = false;
        }
    }
}
