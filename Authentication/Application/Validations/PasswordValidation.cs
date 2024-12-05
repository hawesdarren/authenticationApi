using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using System.Collections;
using System.Text.RegularExpressions;

namespace Authentication.Application.Validations
{
    public class PasswordValidation : DatabaseConnector
    {
        public static bool PasswordComplexityCheck(string password)
        {
            bool result = false;
            // todo - shif regex t database
            string pattern = "^(?:(?=.*\\d)(?=.*[\\w\\W])(?=.*[A-Z]).{7,})";
            Regex rg = new Regex(pattern);
            Match match = rg.Match(password);
            if (match.Success)
            {
                result = true;
            }
            return result;
        }

        public static bool ValidatePasswordMatch(string email, string password)
        {

            // todo
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"SELECT salt, hashedPassword FROM Authentication.users WHERE email = '{email}';";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            MySqlDataReader queryResult = cmd.ExecuteReader();
            queryResult.Read();
            // Validate password
            string hashedPassword = queryResult["hashedPassword"].ToString();
            string hashedSalt = queryResult["salt"].ToString();
            var isPasswordMatch = Argon.MatchPassword(password, hashedPassword, hashedSalt);
            // If passwords don't match increment password attempts in database else reset to 0 -todo
            Hashtable results;
            if (!isPasswordMatch)
            {
                results = IncrementPasswordAttempts(email);
            }
            else
            {
                results = SetPasswordAttempts(email, 0);
            }

            // Close database connection
            conn.Close();
            // Return result
            return isPasswordMatch;
        }

        public static bool CommonlyUsedPasswordsCheck(string password)
        {
            // todo
            // Should reyurn true id password is in the blockedPassword table in the Authentication database
            // Compare should be done in upper case, convert password to uppercase and sql query to uppercase
            bool result = true;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"SELECT COUNT(UPPER(password)) FROM Authentication.blockedPasswords WHERE password = '{password.ToUpper()}'";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            var queryResult = (long)cmd.ExecuteScalar();
            if (queryResult == 0)
            {
                result = false;
            }
            //Close database connection
            conn.Close();
            //Return query result

            return result;
        }

        public static bool IsTemporaryPasswordExpired(string email)
        {
            // Return true if password is temporary password
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"SELECT expiryDate FROM Authentication.users WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            object queryResult = cmd.ExecuteScalar();
            //Close database connection
            conn.Close();
            // Logic for expired password
            if (queryResult != DBNull.Value)
            {
                var currentDate = DateTime.UtcNow.Date;
                DateTime tempExpiry = (DateTime)queryResult;
                if (tempExpiry < currentDate)
                {
                    result = true;
                }
            }

            //Return query result
            return result;
        }

        public static bool IsTemporaryPassword(string email)
        {
            // Return true if password is temporary password
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"SELECT expiryDate FROM Authentication.users WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            object queryResponse = cmd.ExecuteScalar();
            //Close database connection
            conn.Close();
            // Logic for isTempPassword
            if (queryResponse != DBNull.Value)
            {
                result = true;
            }

            //Return query result
            return result;
        }

        public static bool IsTemporaryBlockedPassword(string email)
        {
            // Return true if password is temporary password
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            var sqlString = $"SELECT tempBlockExpiry FROM Authentication.users WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlString, conn);
            object queryResponse = cmd.ExecuteScalar();
            //Close database connection
            conn.Close();
            // Logic for expired password
            if (queryResponse != DBNull.Value)
            {
                var currentDateTime = DateTime.UtcNow;
                DateTime tempBlockExpiry = (DateTime)queryResponse;
                if (tempBlockExpiry > currentDateTime)
                {
                    result = true;
                }
            }

            //Return query result
            return result;
        }


        private static Hashtable IncrementPasswordAttempts(string email)
        {

            // Run query to get current number of login attempts
            int numLoginAttempts = GetNumberOfPasswordAttempts(email);

            // Run query to update number of login attempts
            numLoginAttempts++;
            Hashtable result = SetPasswordAttempts(email, numLoginAttempts);

            return result;
        }

        private static Hashtable SetPasswordAttempts(string email, int attempts)
        {
            Hashtable result = new Hashtable();
            // Run query to update number of login attempts
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            string sqlUpdateAttemptsString = $"UPDATE Authentication.users SET loginAttempts = {attempts} WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlUpdateAttemptsString, conn);
            var attemptsQueryResult = cmd.ExecuteNonQuery();

            // Check for max login attempts and temp lock password
            if (attempts >= 6)
            {
                // Put a temp block on the user and set login attempts to 0
                DateTime tempBlock = DateTime.UtcNow.AddMinutes(10);
                string sqlTempBlockString = $"UPDATE Authentication.users SET tempBlockExpiry = '{tempBlock.ToString("yyyy-MM-dd hh:mm:ss")}' WHERE email = '{email}'";
                MySqlCommand cmd2 = new MySqlCommand(sqlTempBlockString, conn);
                var blockQueryResult = cmd2.ExecuteNonQuery();
                result.Add("blocked", true);
                // Set login attempts back to 0
                SetPasswordAttempts(email, 0);
            }
            //Close database connection
            conn.Close();
            //Return query result
            result.Add("attempts", attempts);
            return result;
        }

        private static int GetNumberOfPasswordAttempts(string email)
        {

            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query to get current number of login attempts
            var sqlSelectString = $"SELECT loginAttempts FROM Authentication.users WHERE email = '{email}'";
            MySqlCommand cmd = new MySqlCommand(sqlSelectString, conn);
            int numLoginAttempts = (int)cmd.ExecuteScalar();
            //Close database connection
            conn.Close();

            return numLoginAttempts;
        }



    }
}
