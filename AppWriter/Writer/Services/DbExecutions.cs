using BD.Connection;
using CrossCutting.Utilitarios;
using Entities.Entities;
using Entities.Entities.CDC;
using Entities.Enums;
using Writer.Connection;
using Writer.Helper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using MensagemKafka = Entities.Entities.Kafka.MensagemKafka;

namespace Writer.Services
{
    public abstract class DbExecutions
    {
        protected SyncConnection _connection;
        protected readonly ILogger<DbExecutions> _logger;
        protected QueryHelperWriter _helper;

        public DbExecutions(SyncConnection connection, ILoggerFactory factory)
        {
            _connection = connection;
            var logger = factory.CreateLogger<DbExecutions>();
            _logger = logger;
            _helper = new QueryHelperWriter(connection, factory);
        }
        /// <summary>
        /// Cria Execution de acordo com o tipo de banco de dados a ser utilizado
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="config"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static DbExecutions createDbExecution(ILoggerFactory factory, IConfiguration config, EnumTypeDbExecution type)
        {
                return new DbExecutionSqlServer(new SqlServerDbConnection(config), factory);
        }

        /// <summary>
        /// Função que retorna o modelo de uma determinada tabela no banco de dados
        /// </summary>
        /// <param name="nometabela">Nome da tabela</param>
        /// <returns>Modelo da tabela no banco de dados</returns>
        public abstract IList<SchemaBD> ConsultarCamposSchema(string nometabela);
        /// <summary>
        /// Função que retorna as constraints de uma determinada tabela no banco de dados
        /// </summary>
        /// <param name="nometabela">Nome da tabela</param>
        /// <returns>Lista contendo o nome das colunas, e o indicativo se a mesma é constraint de banco</returns>
        public abstract IDictionary<string, bool> ConsultarConstraintCamposSchemaPorTabela(string nometabela);

        /// <summary>
        /// Função que deve ser implementada para cada banco, que monta o script de Deleção no banco de dados
        /// </summary>
        /// <param name="mensagemKafka">Dados enviados para montar o script</param>
        /// <param name="metaTabela">Metadado com modelo da tabela em questão</param>
        /// <returns>Retorna a strinf contendo o script de execução no banco de dados</returns>
        public abstract string DeleteRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela);

        /// <summary>
        /// Função que deve ser implementada para cada banco, que monta o script de Atualização no banco de dados
        /// </summary>
        /// <param name="mensagemKafka">Dados enviados para montar o script</param>
        /// <param name="metaTabela">Metadado com modelo da tabela em questão</param>
        /// <returns>Retorna a strinf contendo o script de execução no banco de dados</returns>
        public abstract string UpdateRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela, MetadadoTabela metaDadosBancoDestino);

        /// <summary>
        /// Função que deve ser implementada para cada banco, que monta o script de Inserção no banco de dados
        /// </summary>
        /// <param name="mensagemKafka">Dados enviados para montar o script</param>
        /// <param name="metaTabela">Metadado com modelo da tabela em questão</param>
        /// <returns>Retorna a strinf contendo o script de execução no banco de dados</returns>
        public abstract string InsertRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela, MetadadoTabela metaDadosBancoDestino);

        /// <summary>
        /// Função que trata os dados recebidos pelo Kafka, e os processa para persistir no banco de dados.
        /// </summary>
        /// <param name="dados">Dados a serem persistidos</param>
        /// <param name="metaTabela">Metadado da tabela em questão</param>
        /// <returns>Retorna TRUE caso tenha obtido sucesso</returns>
        public virtual bool TrataRecords(IDictionary<string, MensagemKafka> dados, MetadadoTabela metaTabela, MetadadoTabela modeloBanco)
        {
            var result = true;

            string sqlCommand = "";

            var connection = _connection.GetConnection();
            try
            {
                //Abre a conexao, caso a mesma tenha sido fechada.
                if (connection.State == System.Data.ConnectionState.Closed)
                {
                    connection.Open();
                }
                //using (var transaction = connection.BeginTransaction())
                //{
                    foreach (var item in dados)
                    {
                        var itemTabela = item.Value;
                        sqlCommand = "";
                        if (itemTabela.Operacao == EnumOperacao.Inclusao)
                        {
                            sqlCommand = InsertRecords(itemTabela, metaTabela, modeloBanco);
                            
                        }
                        else if (itemTabela.Operacao == EnumOperacao.Exclusao)
                        {
                            sqlCommand = DeleteRecords(itemTabela, metaTabela);
                        }
                        else if (itemTabela.Operacao == EnumOperacao.Alteracao)
                        {
                            sqlCommand = UpdateRecords(itemTabela, metaTabela, modeloBanco);
                        }
                        if (sqlCommand == null)
                        {
                            _logger.LogError($"Erro ao inserir dados no banco de dados. Operação inválida {itemTabela.Operacao}");
                        }

                        _logger.LogInformation($"Executando comando: {sqlCommand}");
                        var command = _helper.CreateCommand(connection, sqlCommand, null);
                        command.ExecuteNonQuery();

                    }
                    //transaction.Commit();

                //}

            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao executar comando no banco de dados. {ex.Message}");
                result = false;
            }
            finally
            {
                //Sempre Libera a conexão com banco de dados.
                _connection.Dispose();
            }

            return result;
        }
        /// <summary>
        /// Recupera o Metadado do Modelo de uma determinada tabela no banco de dados.
        /// Método utilizado para comparar com a informação recebida pelo Kafka
        /// </summary>
        /// <param name="tabela">Nome da tabela a ser verificada</param>
        /// <returns></returns>
        public MetadadoTabela GetModelo(string tabela)
        {
            var listaSchemaBD = ConsultarCamposSchema(tabela);
            var dicionarioTabelaConstraints = ConsultarConstraintCamposSchemaPorTabela(tabela);
            return Util.ConvertSchemaBDToMetadadoTabela(listaSchemaBD, tabela, dicionarioTabelaConstraints);
        }
    }

    public enum EnumTypeDbExecution
    {
        SQLSERVER
    }
}
