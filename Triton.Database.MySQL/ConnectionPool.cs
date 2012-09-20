
using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Triton.Database.MySQL
{
    public static class DatabaseMysqlConnectionSettings
    {
        private static string _host;
        private static string _username;
        private static string _password;
        private static string _database;
        private static bool _configured = false;

        public static void Setup(string username, string password, string host, string database)
        {
            _host = host;
            _username = username;
            _password = password;
            _database = database;

            _configured = true;
            DatabaseMysqlConnectionPool.Boot(CreateConnection);
        }
        
        private static IDbConnection CreateConnection()
        {
            if (!_configured)
                throw new Exception("Please run the Setup() function properly first.");
            MySqlConnection con = new MySqlConnection("Server="+_host+"; Database="+_database+"; User="+_username+"; Password="+_password+";");
            return con;
        }

    }

    public class DatabaseMysqlConnectionPool : DatabaseConnectionPool
    {
        public static MySqlConnection GetMysqlConnection()
        {
            return (MySqlConnection)GetConnection();
        }
        public static MySqlConnection GetMysqlConnection(int Identifier)
        {
            return (MySqlConnection)GetConnection(Identifier);
        }
    }
}
