using Authentication.Json.Responses;
using Authentication.Models;
using MySql.Data.MySqlClient;
using OtpNet;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace Authentication.Application
{
    public class Tfa : DatabaseConnector
    {
        public static TfaSetupResponse CreateNewTotp(string email, string issuer) {

            var key = KeyGeneration.GenerateRandomKey(20);
            var base32String = Base32Encoding.ToString(key);
            // Store in database
            bool result = RegisterTotpInDatabase(email, base32String);
            var uriTotp = new OtpUri(OtpType.Totp, base32String, email, issuer).ToString();
            // Response 
            TfaSetupResponse tfaSetupResponse = new() { Success = result, Authenticated = false };
            tfaSetupResponse.keyUri = uriTotp;
            tfaSetupResponse.Success = result;
            return tfaSetupResponse;
        }

        public static TfaValidateResponse Validate(string email, string input) {
            // Get key from database
            string key = GetTfaKey(email);
            var base32Bytes = Base32Encoding.ToBytes(key);
            var totp = new Totp(base32Bytes);
            long timeStepMatched;
            // Verify totp
            bool verify = totp.VerifyTotp(input, out timeStepMatched, window: null);
            TfaValidateResponse tfaValidateResponse = new() { Success = false, Authenticated = false };
            if (verify) { 
                tfaValidateResponse.Success = true;
                tfaValidateResponse.Authenticated = true;
                tfaValidateResponse.token = Token.GenerateJwtToken(email, true, 10);
                
            }
            else
            {
                tfaValidateResponse.Success = false;
                tfaValidateResponse.Authenticated = false;
                tfaValidateResponse.SetError(Json.Enums.ErrorEnums.Error.TFA_CODE_INVALID);
            }
            return tfaValidateResponse;
        }

        private static string GetTfaKey(string email) {
            // Query
            StringBuilder query = new StringBuilder();
            query.Append("SELECT tfa.key  ");
            query.Append("FROM Authentication.tfa AS tfa ");
            query.Append("INNER JOIN Authentication.users AS users ");
            query.Append("ON tfa.idUsers=users.id ");
            query.Append($"WHERE users.email = '{email}';");
            // Database connection
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query
            MySqlCommand cmd = new MySqlCommand(query.ToString(), conn);
            MySqlDataReader queryResult = cmd.ExecuteReader();
            queryResult.Read();
            string key = (string)queryResult["key"];
            queryResult.Close();
            return key;
        }

        private static bool RegisterTotpInDatabase(string email, string key) {
            // Register in database, but do not enable
            // Get Get user info
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query to get id
            StringBuilder sqlStringUsers = new StringBuilder();
            sqlStringUsers.Append("SELECT ");
            sqlStringUsers.Append("users.id, users.email ");
            sqlStringUsers.Append("FROM Authentication.users ");
            sqlStringUsers.Append($"WHERE users.email = '{email}';");
            MySqlCommand cmd = new MySqlCommand(sqlStringUsers.ToString(), conn);
            MySqlDataReader queryResult = cmd.ExecuteReader();
            if (queryResult.HasRows == false) {
                return result;
            }
            queryResult.Read();
            int userId = (int)queryResult["id"];
            queryResult.Close ();
            // Run query to get check if customer has any entries in Authentication.tfa
            StringBuilder sqlTfaQuery = new StringBuilder();
            sqlTfaQuery.Append("SELECT COUNT(tfa.idTfa) ");
            sqlTfaQuery.Append("FROM Authentication.tfa ");
            sqlTfaQuery.Append($"WHERE tfa.idUsers = {userId};");
            cmd = new MySqlCommand(sqlTfaQuery.ToString(), conn);
            long isTfa = (long)cmd.ExecuteScalar();
                        
            queryResult.Close();
            if (isTfa == 1)
            {
                // user has current entry, update record
                StringBuilder updateTfaSql = new StringBuilder();
                updateTfaSql.Append("UPDATE Authentication.tfa ");
                updateTfaSql.Append($"SET tfa.key = '{key}', ");
                updateTfaSql.Append("tfa.enabled = 0 ");
                updateTfaSql.Append($"WHERE tfa.idUsers = {userId};");

                cmd = new MySqlCommand(updateTfaSql.ToString(), conn);
                var updateResult = cmd.ExecuteNonQuery();
                if (updateResult == 1)
                {
                    result = true;
                }
            }
            else if (isTfa == 0) 
            {
                // currently no tfa entry - insert new record
                StringBuilder insertTfaSql = new StringBuilder();
                insertTfaSql.Append("INSERT INTO Authentication.tfa ");
                insertTfaSql.Append("(tfa.idUsers, tfa.key, tfa.enabled) ");
                insertTfaSql.Append($"VALUES ({userId}, '{key}', 1);");
                cmd = new MySqlCommand(insertTfaSql.ToString(), conn);
                var insertResult = (long)cmd.ExecuteNonQuery();
                if (insertResult == 1)
                {
                    result = true;
                }
            }
            else{ 
                // todo user should not have multiple tfa records
                result  = false;
            }

            return result;

        }

        public static bool IsTfaEnabled(string email) {
            bool tfaEnabled = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query to get tfa.enabled
            StringBuilder userQuery = new StringBuilder();
            userQuery.Append("SELECT users.id, users.email, tfa.idTfa, tfa.enabled ");
            userQuery.Append("FROM Authentication.users AS users ");
            userQuery.Append("INNER JOIN Authentication.tfa AS tfa ");
            userQuery.Append("ON users.id=tfa.idUsers ");
            userQuery.Append($"WHERE users.email = '{email}' ");
            userQuery.Append($"AND tfa.enabled = 1;");

            MySqlCommand cmd = new MySqlCommand(userQuery.ToString(), conn);
            using (MySqlDataReader queryResult = cmd.ExecuteReader()) {
                if (queryResult.HasRows) {
                    queryResult.Read();
                    tfaEnabled = (bool)queryResult["enabled"];
                    queryResult.Close();
                }
                
            }

            return tfaEnabled;
        }

        public static bool IsTfaRegistered(string email) {
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query to get tfa.enabled
            StringBuilder userQuery = new StringBuilder();
            userQuery.Append("SELECT users.id, users.email, tfa.idTfa, tfa.enabled ");
            userQuery.Append("FROM Authentication.users AS users ");
            userQuery.Append("INNER JOIN Authentication.tfa AS tfa ");
            userQuery.Append("ON users.id=tfa.idUsers ");
            userQuery.Append($"WHERE users.email = '{email}' ;");

            MySqlCommand cmd = new MySqlCommand(userQuery.ToString(), conn);
            using (MySqlDataReader queryResult = cmd.ExecuteReader())
            {
                if (queryResult.HasRows)
                {
                    result = true;
                    queryResult.Close();
                }

            }

            return result;
        }

        public static EnableTfaResponse EnableTfa(string email, bool register, string? totp) {
            bool result = false;
            // To enable tfa valid totp must be supplied 
            // To disable totp not required

            EnableTfaResponse enableTfaResponse = new()
            {
                Authenticated = true,
                Success = result
            };

            // Check for valid totp 
            TfaValidateResponse tfaValidateResponse = Validate(email, totp);
            if (tfaValidateResponse.Success == false) {
                enableTfaResponse.Success = false;
                enableTfaResponse.SetError(Json.Enums.ErrorEnums.Error.TFA_CODE_INVALID);
                return enableTfaResponse;
            }

            // Convert bool to int for SQL query
            int enable = Convert.ToInt32(register);

            // Check if users is registered
            bool isTfaRegistered = IsTfaRegistered(email);
            if (isTfaRegistered)
            {
                // Connect to database
                MySqlConnection conn = OpenConnection();
                conn = OpenConnection();
                // Run query to get tfa.enabled
                StringBuilder userQuery = new StringBuilder();
                userQuery.Append("UPDATE Authentication.tfa AS tfa ");
                userQuery.Append("LEFT JOIN Authentication.users AS users ");
                userQuery.Append("ON tfa.idUsers=users.id ");
                userQuery.Append($"SET tfa.enabled = {enable} ");
                userQuery.Append($"WHERE users.email = '{email}';");
                // Execute query
                MySqlCommand cmd = new MySqlCommand(userQuery.ToString(), conn);
                int isSuccessful = cmd.ExecuteNonQuery();
                if (isSuccessful != 0)
                {
                    result = true;
                    enableTfaResponse.Success = result;
                }
                else {
                    enableTfaResponse.SetError(Json.Enums.ErrorEnums.Error.TFA_ERROR);
                }
            }
            else {
                enableTfaResponse.SetError(Json.Enums.ErrorEnums.Error.EMAIL_NOT_REGISTERED_FOR_TFA);
            }
            

            return enableTfaResponse;
        }

    }
}
