using Authentication.Json.Responses;
using Microsoft.Extensions.Options; // Add this for IOptions<T>
using MySql.Data.MySqlClient;
using System.Data; // Needed for ConnectionState
using System.Net.Mail;

namespace Authentication.Application.Validations
{
    public class EmailValidation
    {
        public static bool ValidateFormat(string email)
        {
            bool result = false;
            // Check email is valid
            try
            {
                MailAddress mailAddress = new(email);
                result = true;
            }
            catch (FormatException)
            {
                result = false;
            }
            catch (Exception)
            {
                result = false;

            }
            return result;
        }

        public static bool EmailRegisteredInDatabase(string email, IOptions<AuthenticationOptions> authenticationOptions)
        {
            bool result = true;
            // Connect to database
            DatabaseConnector databaseConnector = new(authenticationOptions);
            MySqlConnection conn = databaseConnector.OpenConnection();

            // Run query
            var sqlString = $"SELECT COUNT(UPPER(email)) FROM Authentication.users WHERE email = '{email.ToUpper()}';";
            MySqlCommand cmd = new(sqlString, conn);
            var queryResult = (long)cmd.ExecuteScalar();
            if (queryResult == 0)
            {
                result = false;
            }
            conn.Close();
            return result;
        }
    }
}
