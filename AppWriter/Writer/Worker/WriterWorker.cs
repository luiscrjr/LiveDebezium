using CrossCutting.Utilitarios;
using Writer.Services.Generics;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MensagemConverterKafka = Entities.Entities.Kafka.Mensagem;
using System.Net.Mail;

namespace Writer.Worker
{
    public class WriterWorker : BackgroundService
    {
        private readonly ILogger<WriterWorker> _logger;
        private readonly IConfiguration _configuration;
        private readonly ServiceRM service;
        private readonly ILoggerFactory iloggerFactory;

        private String[] TOPICOS;
        private TipoGravacaoEnum tipoBancoDestino;
        private TipoPayloadEnum tipoPayload;

        public WriterWorker(ILoggerFactory factory, ILogger<WriterWorker> logger, IConfiguration configuration)
        {
            GC.Collect();

            _logger = logger;
            iloggerFactory = factory;
            _configuration = configuration;

            TOPICOS = _configuration.GetValue<string>("TopicosTabelaKafka").Split(",");

            var tipoGravacao = _configuration.GetValue<string>("TipoBancoDestino");
            tipoBancoDestino = TipoGravacaoEnum.SqlServer;

            var tipoPayloadString = _configuration.GetValue<string>("TipoPayload");
            tipoPayload = TipoPayloadEnum.Debezium;

            service = ServiceRM.GetServiceRead(tipoBancoDestino, factory, configuration);

            Console.WriteLine($"Reading topics: {_configuration.GetValue<string>("TopicosTabelaKafka")}");

            string paramString = configuration.GetValue<string>("ConString");
            var constring = configuration.GetValue<string>(paramString);

            Console.WriteLine($"");
        }


        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(() =>
            {
                Task.Run(() => MonitoraTopicosTabelas(), stoppingToken);
            }, stoppingToken);
        }

        private Task MonitoraTopicosTabelas()
        {
            var consumerGroup = $"AppWriter_{_configuration.GetValue<string>("PodGuid")}";

            using (var c = new ConsumerBuilder<String, String>(ObtemConsumerConfig(consumerGroup)).Build())
            {

                SubscribeAndReadTopic(c);
            }

            return Task.CompletedTask;
        }



        private void SubscribeAndReadTopic<T, K>(IConsumer<T, K> consumer)
        {
            var cts = new CancellationTokenSource();
            consumer.Subscribe(TOPICOS);

            try
            {
                while (true)
                {

                    var message = consumer.Consume(cts.Token);
                    MensagemConverterKafka mensagemResultado = null;

                    ConsumeResult<String, string> messageConverter = message as ConsumeResult<String, String>;
                    mensagemResultado = Util.MapearDebeziumMensagemParaKafkaMensagem(messageConverter);

                    _logger.LogInformation($"Mensagem: {message.Message.Value} recebida de {message.TopicPartitionOffset}");

                    _logger.LogInformation($"Lendo Mensagem: Recebida de {message.TopicPartitionOffset}");

                    try
                    {
                        if (mensagemResultado != null)
                        {
                            var result = service.ReadAndWriteMensage(mensagemResultado);
                            if (!result)
                            {
                                consumer.StoreOffset(message);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        consumer.StoreOffset(message);
                        _logger.LogError(ex.ToString());
                    }


                }
            }
            catch (OperationCanceledException)
            {
                consumer.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, String.Concat("Erro:", e.Message));
                consumer.Close();
                consumer.Unassign();
                consumer.Unsubscribe();

            }
        }

        private ConsumerConfig ObtemConsumerConfig(string grupo = "")
        {
            ConsumerConfig _consumerConfig = null;

            Console.WriteLine("Using Kafka Server: " + _configuration.GetValue<string>("Kafka_BootstrapServers"));
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration.GetValue<string>("Kafka_BootstrapServers"),
                GroupId = grupo,
                AllowAutoCreateTopics = true,
                EnableAutoOffsetStore = false,
                EnableAutoCommit = false,
                MaxPollIntervalMs = 86400000, //24h máximo permitido
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            return _consumerConfig;
        }
    }
}
