using Authentication.Application.Validations;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Identity.Data;
using MySql.Data.MySqlClient;
using RegisterRequest = Authentication.Json.Requests.RegisterRequest;

namespace Authentication.Application
{
    public class RegisterUser : DatabaseConnector
    {
        public static RegisterResponse Register(RegisterRequest registerRequest)
        {
            RegisterResponse registerResponse = new ();
            // Email format check
            var isFormat = EmailValidation.ValidateFormat(registerRequest.email);
            if (!isFormat)
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.INVALID_EMAIL);
                return registerResponse;
            }
            // Password matches Re-enter password
            if (registerRequest.password != registerRequest.renteredPassword)
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.PASSWORDS_DONT_MATCH);
                return registerResponse;
            }
            // Email already registered
            var isEmailAlreadyRegistered = EmailValidation.EmailRegisteredInDatabase(registerRequest.email);
            if (isEmailAlreadyRegistered)
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.EMAIL_ALREADY_REGISTERED);
                return registerResponse;
            }
            // Password complexity check
            var isPassowordComplex = PasswordValidation.PasswordComplexityCheck(registerRequest.password);
            if (!isPassowordComplex)
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.PASSWORD_COMPLEXITY);
                return registerResponse;
            }
            // Is password in blocked password list
            var isPasswordBlocked = PasswordValidation.CommonlyUsedPasswordsCheck(registerRequest.password);
            if (isPasswordBlocked)
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.COMMON_PASSWORD);
                return registerResponse;
            }
            //Register the user
            registerResponse = CreateSaltAndHashAndRegisterUser(registerRequest);
            return registerResponse;
        }

        private static RegisterResponse CreateSaltAndHashAndRegisterUser(RegisterRequest registerRequest) {
            RegisterResponse registerResponse = new ();
            // Create Salt and Hash for Password
            byte[] salt = Argon.CreateSalt();
            string saltString = Convert.ToHexString(salt);
            byte[] passwordBytes = Argon.CreateHashPassword(registerRequest.password, salt);
            string hashedPassword = Convert.ToHexString(passwordBytes);
            // Register user in Database
            var isRegistered = RegisterUserInDatabase(registerRequest.email, saltString, hashedPassword);
            if (isRegistered)
            {
                registerResponse.Success = true;
                registerResponse.Authenticated = false;
                return registerResponse;
            }
            else
            {
                registerResponse.Success = false;
                registerResponse.SetError(RegisterResponse.Error.EMAIL_ALREADY_REGISTERED);
                return registerResponse;
            }
        }

        private static bool RegisterUserInDatabase(string email, string salt, string hashedPassword) {
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"INSERT INTO users (email, salt, hashedPassword, loginAttempts, tempBlockExpiry, expiryDate)" +
                            $"VALUES ('{email}', '{salt}', '{hashedPassword}', 0, null, null);";
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
