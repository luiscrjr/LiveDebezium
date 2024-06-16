using Writer.Connection;
using Writer.Services.Generics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MensagemFromKafka = Entities.Entities.Kafka.Mensagem;

namespace Writer.Services
{
    public class ReadSqlServerService : ServiceRM
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _factory;
        public ReadSqlServerService(ILoggerFactory factory, IConfiguration configuration)
        {
            var logger = factory.CreateLogger<ReadSqlServerService>();
            _logger = logger;
            _configuration = configuration;
            _factory = factory;
        }


        protected override DbExecutions getDbExecution()
        {
            return DbExecutions.createDbExecution(_factory, _configuration, EnumTypeDbExecution.SQLSERVER);
        }


    }
}
