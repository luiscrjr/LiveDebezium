using BD.Connection;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace Writer.Connection
{
    public class SqlServerDbConnection : SyncConnection
    {
        public SqlServerDbConnection(IConfiguration config) : base(config)
        {
        }

        protected override string GetConnectionString()
        {
            string paramString = configuration.GetValue<string>("ConString");

            return paramString;
        }

        public override DbConnection GetConnection() => new SqlConnection(GetConnectionString());
    }
}
