using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;

namespace Triton.Database   
{

    public class DatabaseOleDbConnectionPool : DatabaseConnectionPool
    {
        public static OleDbConnection GetOleDbConnection()
        {
            return (OleDbConnection)GetConnection();
        }
        public static OleDbConnection GetOleDbConnection(int Identifier)
        {
            return (OleDbConnection)GetConnection(Identifier);
        }
    }
}
