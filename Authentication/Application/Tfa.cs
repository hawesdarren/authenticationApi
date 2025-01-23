using Authentication.Json.Responses;
using MySql.Data.MySqlClient;
using OtpNet;

namespace Authentication.Application
{
    public class Tfa : DatabaseConnector
    {
        public static TfaRegisterResponse CreateNewTotp(string email) {

            var key = KeyGeneration.GenerateRandomKey(20);
            var base32String = Base32Encoding.ToString(key); //Store in database
            var uriTotp = new OtpUri(OtpType.Totp, base32String, email, "hawes.co.nz").ToString();
            // Response 
            TfaRegisterResponse tfaRegisterResponse = new() { Success = false, Authenticated = false };
            tfaRegisterResponse.keyUri = uriTotp;
            tfaRegisterResponse.Success = true;
            return tfaRegisterResponse;
        }

        public static void Validate(string key, string input) {
            // todo
            var base32Bytes = Base32Encoding.ToBytes(key);
            var totp = new Totp(base32Bytes);
            long timeStepMatched;
            bool verify = totp.VerifyTotp(input, out timeStepMatched, window: null);
            TfaValidateResponse tfaValidateResponse = new() { Success = false, Authenticated = false };
            if (verify) { 
                tfaValidateResponse.Success = true;
                tfaValidateResponse.Authenticated = true;
                
            }
            else
            {
                tfaValidateResponse.Success = true;
                tfaValidateResponse.Authenticated = false;

            }
        }

        private static void RegisterTotpInDatabase(string email, string key) {
            //Get Get user info
            bool result = false;
            // Connect to database
            MySqlConnection conn = OpenConnection();
            conn = OpenConnection();
            // Run query to get idUser
            var sqlStringUsers = $"SELECT idUsers FROM Authentication.users WHERE email = '{email}';";
            MySqlCommand cmd = new MySqlCommand(sqlStringUsers, conn);
            MySqlDataReader queryResult = cmd.ExecuteReader();
            int userId = (int)queryResult["idUser"];
            // Run a query to check if user has entry in tfa table
            var sqlStringTfa = $"SELECT COUNT(idTfa) FROM Authentication.tfa WHERE tfa.idUsers = {userId};";
            cmd = new MySqlCommand(sqlStringTfa, conn);
            var queryCountResult = (int)cmd.ExecuteScalar();
            if (queryCountResult == 0)
            {
                // currently no tfa entry - insert new record
                var insertTfaSql = $"INSERT INTO Authentication.tfa (tfa.idUsers, tfa.key, tfa.enabled) VALUES (1, '{key}', 1);";
                cmd = new MySqlCommand(insertTfaSql, conn);
                var insertResult = (long)cmd.ExecuteNonQuery();
            }
            else {
                // user has current entry, update record
                var updateTfaSql =  $"UPDATE Authentication.tfa " +
                                    $"SET tfa.key = '{key}', " +
                                    $"enabled = 1 " +
                                    $"WHERE `idTfa` = {userId};";
            }
           
            

        }
    }
}
