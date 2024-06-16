using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MensagemFromKafka = Entities.Entities.Kafka.Mensagem;

namespace Writer.Services.Generics
{
    public abstract class ServiceRM
    {
        protected ILogger _logger;
        // public abstract bool ReadAndWriteMensage(MensagemFromKafka mensagem);

        protected abstract DbExecutions getDbExecution();


        public bool ReadAndWriteMensage(MensagemFromKafka mensagem)
        {
            var result = true;
            var execution = getDbExecution();
            var nomeTabela = mensagem.Conteudo.Nome;
            //Conteudo contendo tambem o modelo de dados.
            var dados = mensagem.Conteudo.DicionarioMetadados;
            var modelo = mensagem.Conteudo.Estrutura;
            //Existe Estrutura Válida e Existem Dados a Serem processados
            if (modelo != null && modelo.ListaColuna.Count > 0 && dados != null && dados.Count > 0)
            {
                //Compara modelo com o banco de dados.
                var modeloBanco = execution.GetModelo(nomeTabela);
                if (modeloBanco != null)
                {
                    //Retirando tratativa de modelo de destino diferente da origem temporariamente
                    //if (modeloBanco.ListaColuna.Count != modelo.ListaColuna.Count)
                    //{
                    //    _logger.LogCritical($"Tabela {nomeTabela} com modelo de dados no banco, diferente do recebido em mensagem");
                    //    result = false;
                    //}
                    //else
                    //{
                    //Realiza Operação

                    modelo.ListaColuna.RemoveAll(item => item.Nome.Contains("din_dadoprogramado"));

                    _logger.LogInformation("Executando comando no banco de dados.");
                    var success = true;

                    success = execution.TrataRecords(dados, modelo, modeloBanco);
                    if (!success)
                    {
                        _logger.LogInformation("Erro ao inserir dados no banco de dados.");
                        result = false;
                    }

                    //}
                }
                else
                {
                    _logger.LogCritical($"Tabela {nomeTabela} não encontrada no banco de dados");
                    result = false;
                }
            }
            return result;
        }


        public static ServiceRM GetServiceRead(TipoGravacaoEnum tipoGravacao, ILoggerFactory factory, IConfiguration configuration)
        {
            return new ReadSqlServerService(factory, configuration);
        }
    }
}
