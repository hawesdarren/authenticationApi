using Authentication.Application.Validations;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using MySql.Data.MySqlClient;

namespace Authentication.Application
{
    public class ChangePassword : DatabaseConnector
    {

        public static ChangePasswordResponse Change(ChangePasswordRequest request) { 
            ChangePasswordResponse response = new ChangePasswordResponse();
            // Password and Conform Passowrd match
            if (!request.password.Equals(request.confirmPassword)) {
                response.Success = false;
                response.SetError(Json.Enums.ErrorEnums.Error.PASSWORDS_DONT_MATCH);
                return response;
            }
            // Complexity check
            if (!PasswordValidation.PasswordComplexityCheck(request.password)) {
                response.Success = false;
                response.SetError(Json.Enums.ErrorEnums.Error.PASSWORD_COMPLEXITY);
                return response;
            }
            // Check for common password in block list
            if (PasswordValidation.CommonlyUsedPasswordsCheck(request.password)) {
                response.Success = false;
                response.SetError(Json.Enums.ErrorEnums.Error.COMMON_PASSWORD);
                return response;
            }
            response.Success = true;
            return response;

        }

        private static void UpdatePassword(ChangePasswordRequest request) {
            // Connect to database
           
        }
       
    }
}
