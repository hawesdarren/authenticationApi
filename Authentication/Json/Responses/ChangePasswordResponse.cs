using System.Diagnostics.CodeAnalysis;

namespace Authentication.Json.Responses
{
    public class ChangePasswordResponse : BaseResponse
    {
        

        [SetsRequiredMembers]
        public ChangePasswordResponse()
        {
            Success = false;
        }
    }
}
