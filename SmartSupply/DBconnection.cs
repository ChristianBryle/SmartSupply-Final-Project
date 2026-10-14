using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSupply
{
    public class DBconnection
    {
        private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=SmartSupplyDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection Open()
        {
            var connection = new SqlConnection(ConnectionString);
            connection.Open();
            return connection;
        }
    }
}
