using MySql.Data.MySqlClient;

namespace Authentication.Application
{
    public class DatabaseConnector
    {
        public static MySqlConnection OpenConnection()
        {
            MySqlConnection conn = new MySqlConnection();
            const string connectionString = "server=192.168.153.1;port=32010;uid=root;pwd=test01;database=Authentication";

            try
            {
                conn = new MySqlConnection();
                conn.ConnectionString = connectionString;
                conn.Open();

            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return conn;
        }
                
    }
}
