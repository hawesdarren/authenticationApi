using Authentication.Application.Validations;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;

namespace Authentication.Application
{
    public class LoginUser : DatabaseConnector
    {
        private readonly AuthenticationOptions _authenticationOptions;

        public LoginUser(IOptions<AuthenticationOptions> authenticationOptions)
        {
            _authenticationOptions = authenticationOptions.Value;
        }

        private static bool IsMaxLoginAttempts(string email) { 
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            // Create the query
            var sqlString = $"SELECT loginAttempts FROM Authentication.users WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            // Run the query
            var numLoginAttempts = (long)cmd.ExecuteScalar();
            // Return result
            return result;
        
        }

        // Fix for CS7036 and IDE0090:
        // - Pass an AuthenticationOptions instance to the Token constructor.
        // - Use object initializer syntax for simplification if possible.

        public LoginResponse ValidatePassword(LoginRequest loginRequest) {
            // This is main process for validating login creds
            LoginResponse loginResponse = new() { Success = false, Authenticated = false};
            // Email format check
            var isEmailFormatValid = EmailValidation.ValidateFormat(loginRequest.email);
            if (!isEmailFormatValid) { 
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.INVALID_EMAIL);
                return loginResponse;
            }
            //Is Email registered
            var isEmailRegistered = EmailValidation.EmailRegisteredInDatabase(loginRequest.email);
            if (!isEmailRegistered) { 
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.INVALID);
                return loginResponse;
            }
            // Is Password in a Temporary block (not this is not a temp password but a temporary block due to max login attempts)
            var isTempBlock = PasswordValidation.IsTemporaryBlockedPassword(loginRequest.email);
            if (isTempBlock) {
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.PASSWORD_TEMP_BLOCK);
                return loginResponse;
            }
            // Temp password check 
            bool isTempPassword = PasswordValidation.IsTemporaryPassword(loginRequest.email);
            if (isTempPassword) {
                // Temporary password expired check
                bool isPasswordTempExpired = PasswordValidation.IsTemporaryPasswordExpired(loginRequest.email);
                if (isPasswordTempExpired)
                {
                    loginResponse.Success = false;
                    loginResponse.SetError(LoginResponse.Error.TEMP_PASSWORD_EXPIRED);
                    loginResponse.tempPassword = true;
                    return loginResponse;
                }
                else {
                    loginResponse.tempPassword = true;
                }
            }
            
            // Validate the password
            bool isPasswordValid = PasswordValidation.ValidatePasswordMatch(loginRequest.email, loginRequest.password);
            if (isPasswordValid)
            {
                // Check if tfa is enabled
                Tfa tfa = new Tfa(Options.Create(_authenticationOptions));
                var isTfaEnable = tfa.IsTfaEnabled(loginRequest.email);

                Token tokenGenerator = new Token(Options.Create(_authenticationOptions));

                if (isTfaEnable)
                {
                    loginResponse.Success = true;
                    loginResponse.token = tokenGenerator.GenerateJwtToken(loginRequest.email, false, 10);
                    loginResponse.tfaEnabled = true;
                    loginResponse.Authenticated = false;
                }
                else {
                    loginResponse.Success = true;
                    loginResponse.token = tokenGenerator.GenerateJwtToken(loginRequest.email, true, 10);
                    loginResponse.refreshToken = tokenGenerator.GenerateJwtToken(loginRequest.email, true, 600);
                    loginResponse.tfaEnabled = false;
                    loginResponse.Authenticated = true;
                }
                // Get expiry from token and add to response
                var expTime = tokenGenerator.GetExpiryFromToken(loginResponse.token);
                loginResponse.expiry = expTime;
                
            }
            else {
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.INVALID);
                loginResponse.Authenticated = false;
            }
            return loginResponse;
        }
    }
}
