using MySql.Data.MySqlClient;
using System.Data;

namespace Authentication.Application
{
    public class DatabaseConnector
    {
        public static MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection();

            var builder = new ConfigurationBuilder().AddUserSecrets<Program>();
            var projectDir = Directory.GetParent("Authentication");
            var secretFiles = Directory.EnumerateFiles(".", "secrets.json", SearchOption.AllDirectories);
            foreach (var path in secretFiles)
            {
                builder.AddJsonFile(path, optional: true);
                //config.Configuration.AddJsonFile(path);
            }
            var config = builder.Build();
            //const string connectionString = "server=192.168.153.1;port=32010;uid=root;pwd=test01;database=Authentication";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = config["authentication:databaseConnectionString"];
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
