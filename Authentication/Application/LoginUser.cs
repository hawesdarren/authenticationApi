﻿using Authentication.Application.Validations;
using Authentication.Json.Requests;
using Authentication.Json.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using MySql.Data.MySqlClient;

namespace Authentication.Application
{
    public class LoginUser : DatabaseConnector
    {
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

        public static LoginResponse ValidatePassword(LoginRequest loginRequest) {
            // This is main process for validating login creds
            LoginResponse loginResponse = new() { Success = false, Authenticated = false};
            // Email format check
            var isEmailFormatValid = EmailValidation.ValidateFormat(loginRequest.email);
            if (!isEmailFormatValid) { 
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.INVALID_EMAIL);
                //loginResponse.error = "Email is invalid format";
                return loginResponse;
            }
            //Is Email registered
            var isEmailRegistered = EmailValidation.EmailRegisteredInDatabase(loginRequest.email);
            if (!isEmailRegistered) { 
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.INVALID);
                //loginResponse.error = "Invalid email or password";
                return loginResponse;
            }
            // Is Password in a Temporary block (not this is not a temp password but a temporary block due to max login attempts)
             var isTempBlock = PasswordValidation.IsTemporaryBlockedPassword(loginRequest.email);
            if (isTempBlock) {
                loginResponse.Success = false;
                loginResponse.SetError(LoginResponse.Error.PASSWORD_TEMP_BLOCK);
                //loginResponse.error = "Password is temporarily blocked";
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
                    //loginResponse.error = "Tempoarary password has expired";
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
                var isTfaEnable = Tfa.IsTfaEnabled(loginRequest.email);
                if (isTfaEnable)
                {
                    loginResponse.Success = true;
                    loginResponse.token = Token.GenerateJwtToken(loginRequest.email, false);
                    loginResponse.tfaEnabled = true;
                    loginResponse.Authenticated = false;
                }
                else {
                    loginResponse.Success = true;
                    loginResponse.token = Token.GenerateJwtToken(loginRequest.email, true);
                    loginResponse.tfaEnabled = false;
                    loginResponse.Authenticated = true;
                }
                
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
