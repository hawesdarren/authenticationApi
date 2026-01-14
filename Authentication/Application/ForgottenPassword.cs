using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using Authentication.Json.Requests;
using Authentication.Json.Responses;

namespace Authentication.Application
{
    public class ForgottenPassword
    {
        private readonly SmtpOptions _smtpOptions;

        public ForgottenPassword(IOptions<SmtpOptions> smtpOptions)
        {
            _smtpOptions = smtpOptions.Value;
        }

        private static string GenerateTempPassword()
        {
            // List of colors with first letter capitalized
            string[] colors = new[]
            {
                "Red", "Blue", "Green", "Yellow", "Purple", "Orange", "Pink", "Brown", "Black", "White",
                "Gray", "Violet", "Indigo", "Cyan", "Magenta", "Teal", "Lime", "Olive", "Maroon", "Navy"
            };

            // List of special characters
            char[] specialChars = new[] { '!', '@', '$', '?', '_', '-' };

            Random random = new Random();

            // Pick a random color
            string color = colors[random.Next(colors.Length)];

            // Pick a random special character
            char specialChar = specialChars[random.Next(specialChars.Length)];

            // Generate a random 4-digit number (1000-9999)
            int number = random.Next(1000, 10000);

            // Combine to form the password
            return $"{color}{specialChar}{number}";
        }

        private bool CheckEmailExists(string email)
        {
            bool result = false;
            // Connect to database
            MySqlConnection conn = DatabaseConnector.OpenConnection();
            // Create the query
            var sqlString = $"SELECT COUNT(*) FROM Authentication.users AS users " +
                            $"WHERE users.email = '{email}';";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            // Run the query
            var count = (long)cmd.ExecuteScalar();
            if (count > 0)
            {
                result = true;
            }
            // Close connection
            conn.Close();
            // Return result
            return result;
        }


        private bool StoreTempPasswordInDatabase(string email, string tempPassword)
        {
            bool result = false;
            // Connect to database
            MySqlConnection conn = DatabaseConnector.OpenConnection();
            // Hash the password
            // Create Salt and Hash for Password
            byte[] salt = Argon.CreateSalt();
            string saltString = Convert.ToHexString(salt);
            byte[] passwordBytes = Argon.CreateHashPassword(tempPassword, salt);
            string hashedTempPassword = Convert.ToHexString(passwordBytes);
            // Set expiry date to 24 hours from now
            DateTime expiryDate = DateTime.UtcNow.AddHours(24);
            // Create the query
            var sqlString = $"UPDATE Authentication.users AS users " +
                            $"SET users.hashedPassword = '{hashedTempPassword}', " +
                            $"users.salt = '{saltString}', " + 
                            $"users.expiryDate = '{expiryDate.ToString("yyyy-MM-dd HH:mm:ss")}' " +
                            $"WHERE users.email = '{email}'; ";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            // Run the query
            int rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                result = true;
            }
            // Close connection
            conn.Close();
            // Return result
            return result;
        }

        private async Task<bool> SendTempPasswordEmailAsync(string email, string tempPassword)
        {
            // Todo: Implement email sending functionality
            bool result = false;
            var subject = "Your Temporary Password";
            var body = $"<p>Your temporary password is: <strong>{tempPassword}</strong></p>" +
                       "<p>Please use this password to log in and remember to change it after logging in.</p>" +
                       "<p>This temporary password will expire in 24 hours.</p>";
            // Send the email using ZeptoMail
            var smtp2go = new SMTP2GO(Options.Create(_smtpOptions));
            result = await smtp2go.SendEmailAsync(email, subject, body);
            return result;
        }

        public async Task<ForgottenPasswordResponse> ProcessForgottenPasswordAsync(ForgottenPasswordRequest forgottenPasswordRequest)
        {
            ForgottenPasswordResponse forgottenPasswordResponse = new ForgottenPasswordResponse
            {
                Authenticated = false,
                Success = false
            };
            // Check if email exists
            bool emailExists = CheckEmailExists(forgottenPasswordRequest.email);
            if (!emailExists)
            {
                forgottenPasswordResponse.Success = false;
                forgottenPasswordResponse.SetError(Json.Responses.ForgottenPasswordResponse.Error.EMAIL_NOT_FOUND);
                return forgottenPasswordResponse;
            }
            // Generate temporary password
            string tempPassword = GenerateTempPassword();
            // Store temporary password in database
            bool isStored = StoreTempPasswordInDatabase(forgottenPasswordRequest.email, tempPassword);
            if (!isStored)
            {
                forgottenPasswordResponse.Success = false;
                forgottenPasswordResponse.SetError(Json.Responses.ForgottenPasswordResponse.Error.DATABASE_ERROR);
                return forgottenPasswordResponse;
            }
            // Send temporary password via email
            bool emailSent = await SendTempPasswordEmailAsync(forgottenPasswordRequest.email, tempPassword);
            if (!emailSent)
            {
                forgottenPasswordResponse.Success = false;
                forgottenPasswordResponse.SetError(Json.Responses.ForgottenPasswordResponse.Error.EMAIL_SENDING_FAILED);
                return forgottenPasswordResponse;
            }
            // If all steps are successful
            forgottenPasswordResponse.Success = true;
            forgottenPasswordResponse.Authenticated = false;
            return forgottenPasswordResponse;
        }
    }
}
