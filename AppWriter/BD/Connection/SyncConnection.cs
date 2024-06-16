using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace BD.Connection
{
    public abstract class SyncConnection
    {
        protected DbConnection? _connection;
        private readonly IConfiguration _configuration;
        public IConfiguration configuration { get => _configuration; }
        protected SyncConnection(IConfiguration config)
        {
            this._configuration = config;
        }

        public abstract DbConnection GetConnection();


        protected abstract string GetConnectionString();

        public void Dispose()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
