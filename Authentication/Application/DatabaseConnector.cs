using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Data;

namespace Authentication.Application
{
    public class DatabaseConnector
    {
        readonly AuthenticationOptions _authenticationOptions;

        public DatabaseConnector(IOptions<AuthenticationOptions> authenticationOptions)
        {
            _authenticationOptions = authenticationOptions.Value;
        }

        public MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection();

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = _authenticationOptions.DatabaseConnectionString;
                conn.Open();
                if (conn == null || conn.State != ConnectionState.Open)
                {
                    // Handle error: connection is not open
                    throw new InvalidOperationException("Database connection is not open.");
                }

            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return conn;
        }
                
    }
}
