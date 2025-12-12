using Authentication.Json.Responses;
using MySql.Data.MySqlClient;
using System.Data; // Needed for ConnectionState
using System.Net.Mail;

namespace Authentication.Application.Validations
{
    public class EmailValidation : DatabaseConnector
    {
        public static bool ValidateFormat(string email)
        {
            bool result = false;
            // Check email is valid
            try
            {
                MailAddress mailAddress = new MailAddress(email);
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

        public static bool EmailRegisteredInDatabase(string email)
        {
            bool result = true;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            
            // Run query
            var sqlString = $"SELECT COUNT(UPPER(email)) FROM Authentication.users WHERE email = '{email.ToUpper()}';";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
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
