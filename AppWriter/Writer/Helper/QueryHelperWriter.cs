using BD.Connection;
using Domain.Repositories.Impl.Helpers.Extensions;
using Writer.Connection;
using log4net;
using log4net.Config;
using log4net.Core;
using Microsoft.Extensions.Logging;
using System.Data.Common;
using System.Reflection;

namespace Writer.Helper
{
/// <summary>
/// Classe que executa de fato os comandos de banco de dados
/// </summary>
    public class QueryHelperWriter
    {
        private SyncConnection _connection;
        //private readonly ILogger _logger;
        private readonly ILogger<SyncConnection> _logger;


        public QueryHelperWriter(SyncConnection connection, ILoggerFactory loggerFactory)
        {
            _connection = connection;
            //LogManager.RegisterLogger<Logger>();
            _logger = loggerFactory.CreateLogger<SyncConnection>();
        }
        private DbConnection GetConnection()
        {
            var con = _connection.GetConnection();
            if (con.State == System.Data.ConnectionState.Closed)
            {
                con.Open();
            }
            return con;
        }


        public DbCommand CreateCommand(string sql, DbParameter[] parameters)
        {

            var con = GetConnection();

            return CreateCommand(con, sql, parameters);
        }

        public DbCommand CreateCommand(DbConnection con, string sql, DbParameter[] parameters)
        {
            return CreateCommand(null, con, sql, parameters);
        }

        public DbCommand CreateCommand(DbTransaction transaction, DbConnection con, string sql, DbParameter[] parameters)
        {
            _logger.LogInformation($"[{this.GetType().Name}] Create Command.");
            var command = con.CreateCommand();
            if (transaction != null)
            {
                command.Transaction = transaction;
            }

            command.CommandText = sql;
            if (parameters != null && parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
            _logger.LogInformation($"[{this.GetType().Name}] {sql}");

            return command;
        }



        private DbDataReader ExcuteReader(string sql, params DbParameter[] parameters)
        {
            _logger.LogInformation($"[{this.GetType().Name}] ExecuteReaderAsync<T> - {String.Join(",", parameters.Select(p => p.ParameterName + ':' + p.Value).ToArray())}");
            return CreateCommand(sql, parameters).ExecuteReader();
        }


        public List<T> QueryToList<T>(string sql, params DbParameter[] parameters) where T : new()
        {
            _logger.LogInformation($"[{this.GetType().Name}] QueryToListAsync<T> - {String.Join(",", parameters.Select(p => p.ParameterName + ':' + p.Value).ToArray())}");
            var result = (ExcuteReader(sql, parameters));
            return result.MapToList<T>();
        }

        public IList<string[]> QueryToListArrayString(string sql, params DbParameter[] parameters)
        {
            var listaDados = new List<string[]>();

            var dbReader = ExcuteReader(sql.ToString(), parameters);

            if (dbReader != null && dbReader.HasRows)
            {
                while (dbReader.Read())
                {
                    var arrayString = new string[dbReader.FieldCount];

                    for (var Index = 0; Index < dbReader.FieldCount; Index++)
                    {
                        var value = dbReader.GetValue(Index).ToString();
                        arrayString[Index] = value;
                    }

                    listaDados.Add(arrayString);
                }
                dbReader.Close();
            }
            return listaDados;
        }
    }
}
