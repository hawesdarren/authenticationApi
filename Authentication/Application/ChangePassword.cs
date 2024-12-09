using Authentication.Application.Validations;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Authentication.Models;
using Microsoft.AspNetCore.Identity.Data;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;

namespace Authentication.Application
{
    public class ChangePassword : DatabaseConnector
    {

        public static ChangePasswordResponse Change(ChangePasswordRequest request, string email) { 
            ChangePasswordResponse response = new ChangePasswordResponse();
            // Password and Confirm Password match
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
            //Update the password in the database
            response.Success = UpdatePassword(request, email); ;
            return response;

        }

        private static bool UpdatePassword(ChangePasswordRequest request, string email ) {
            
            bool result = false;
            // Create Salt and Hash for Password
            byte[] salt = Argon.CreateSalt();
            string saltString = Convert.ToHexString(salt);
            byte[] passwordBytes = Argon.CreateHashPassword(request.password, salt);
            string hashedPassword = Convert.ToHexString(passwordBytes);

            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"UPDATE Authentication.users " + 
                            $"SET salt = '{saltString}', " + 
                            $"hashedPassword = '{hashedPassword}', " + 
                            $"loginAttempts = 0, " + 
                            $"tempBlockExpiry = null, " +
                            $"expiryDate = null " + 
                            $"WHERE email = '{email}'; ";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            var queryResult = (long)cmd.ExecuteNonQuery();
            if (queryResult != 0)
            {
                result = true;
            }
            conn.Close();
            return result;

        }
       
    }
}
