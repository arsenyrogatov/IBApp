using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace IBApp
{
    internal class DBConnection: IDisposable
    {
        static readonly string Server = "DESKTOP-OUCRF2M";
        static readonly string Database = "IBBase";
        internal static System.Data.SqlClient.SqlConnection connection = new System.Data.SqlClient.SqlConnection($"Data Source={Server};Initial Catalog=\"{Database}\";Integrated Security=True");

        public DBConnection()
        {
            DBConnection.connection.Open();
        }

        public void Dispose ()
        {
            DBConnection.connection.Close();
        }
    }
}
