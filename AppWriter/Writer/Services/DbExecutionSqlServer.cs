using Entities.Entities;
using Writer.Connection;
using Microsoft.Extensions.Logging;
using System.Data.SqlClient;
using System.Text;
using MensagemKafka = Entities.Entities.Kafka.MensagemKafka;
using Entities.Entities.CDC;

namespace Writer.Services
{
    public class DbExecutionSqlServer : DbExecutions
    {
        public DbExecutionSqlServer(SqlServerDbConnection connection, ILoggerFactory factory) : base(connection, factory)
        {
        }

        public override IList<SchemaBD> ConsultarCamposSchema(string nometabela)
        {
            var sql = new StringBuilder();

            sql.Append(@"SELECT                             
                            COLUMN_NAME as ColName,
                            COL_LENGTH(TABLE_NAME,COLUMN_NAME) as ColLength,
                            0 as ColType,                            
                            DATA_TYPE as ColNameType,
                            ORDINAL_POSITION as Colno 
                        FROM INFORMATION_SCHEMA.COLUMNS");

            sql.Append(" where lower(TABLE_NAME) = @NomeTabela order by colno");

            var parametros = new[] { new SqlParameter("NomeTabela", nometabela.ToLower()) };

            return _helper.QueryToList<SchemaBD>(sql.ToString(), parametros);
        }



        public override IDictionary<string, bool> ConsultarConstraintCamposSchemaPorTabela(string nometabela)
        {
            var sql = new StringBuilder();

            sql.Append(@"SELECT                             
                            col.name AS column_name                            
                        FROM 
                         sys.key_constraints AS k
                            INNER JOIN 
                            sys.tables AS t ON k.parent_object_id = t.object_id
                        INNER JOIN 
                            sys.index_columns AS ic ON ic.object_id = t.object_id AND ic.index_id = k.unique_index_id
                        INNER JOIN 
                            sys.columns AS col ON col.object_id = t.object_id AND col.column_id = ic.column_id
                        INNER JOIN 
                            sys.objects AS c ON c.object_id = k.object_id                                                 
                            WHERE lower(t.name) = @NomeTabela and c.type = 'PK'");

            var parametros = new[] { new SqlParameter("NomeTabela", nometabela.ToLower()) };

            var listaConstraints = _helper.QueryToListArrayString(sql.ToString(), parametros);
            var constraints = listaConstraints.Count > 0 ? listaConstraints[0] : new string[0];

            var dicionarioConstraint = new Dictionary<string, bool>();

            constraints = constraints.Where(nomeColuna => !string.IsNullOrWhiteSpace(nomeColuna)).ToArray();

            foreach (var item in constraints)
            {
                dicionarioConstraint.Add(item, true);
            }

            return dicionarioConstraint;
        }

        private String convertDataType(String dataType, String value)
        {
            if (value == null)
            {
                return null;
            }
            else if(string.IsNullOrEmpty(value))
            {
                return "NULL";
            }
            else if (dataType.Equals("datetime") || dataType.Equals("date"))
            {
                return $"CONVERT(DATETIME,'{value}',103)";
            }
            else
            {
                return $"'{value}'";
            }
        }

        public override string InsertRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela, MetadadoTabela metaDadosBancoDestino)
        {

            var metaItens = metaTabela.ListaColuna;
            var sqlString = new StringBuilder();
            sqlString.Append("INSERT INTO ").Append(metaTabela.Nome).Append(" (");
            for (var i = 0; i < metaItens.Count; i++)
            {
                var metadados = mensagemKafka.Metadados[i];
                sqlString.Append(metadados.NomeColuna);
                //Adiciona Virgula
                if ((i + 1) < metaItens.Count)
                {
                    sqlString.Append(",");
                }
            }
            sqlString.Append(") ").Append("Values (");
            for (var i = 0; i < metaItens.Count; i++)
            {
                var metadados = mensagemKafka.Metadados[i];
                var metaEstrutura = metaItens[i];

                //Verifica tipo de dado
                var value = convertDataType(metaEstrutura.Tipo, metadados.Valor);
                sqlString.Append(value);
                //Adiciona Virgula
                if ((i + 1) < metaItens.Count)
                {
                    sqlString.Append(",");
                }


            }
            sqlString.Append(")");
            return sqlString.ToString();
        }

        public override string UpdateRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela, MetadadoTabela metaDadosBancoDestino)
        {

            var metaItens = metaTabela.ListaColuna;
            var sqlString = new StringBuilder();
            sqlString.Append("Update ").Append(metaTabela.Nome).Append(" SET ");
            var sqlWhere = new StringBuilder();
            sqlWhere.Append(" Where ");
            var contaisPk = false;
            for (var i = 0; i < metaItens.Count; i++)
            {
                var metadados = mensagemKafka.Metadados[i];
                var metaEstrutura = metaItens[i];
                //Verifica tipo de dado
                var value = convertDataType(metaEstrutura.Tipo, metadados.Valor);
                if (metaEstrutura.ChavePrimaria)
                {
                    //Se já contiver uma PK, acrescenta o AND
                    if (contaisPk)
                    {
                        sqlWhere.Append(" AND ");
                    }
                    sqlWhere.Append($" {metadados.NomeColuna} = {value}");

                    contaisPk = true;
                }
                else
                {
                    sqlString.Append($" {metadados.NomeColuna} = {value}");
                    //Adiciona Virgula
                    if ((i + 1) < metaItens.Count)
                    {
                        sqlString.Append(",");
                    }
                }



            }
            sqlString.Append(sqlWhere);
            if (!contaisPk)
            {
                return null;
            }
            return sqlString.ToString();
        }

        public override string DeleteRecords(MensagemKafka mensagemKafka, MetadadoTabela metaTabela)
        {

            var metaItens = metaTabela.ListaColuna;
            var sqlString = new StringBuilder();
            sqlString.Append("delete from ").Append(metaTabela.Nome);
            sqlString.Append(" Where ");
            var contaisPk = false;
            for (var i = 0; i < metaItens.Count; i++)
            {
                var metadados = mensagemKafka.Metadados[i];
                var metaEstrutura = metaItens[i];
                //Verifica tipo de dado
                var value = convertDataType(metaEstrutura.Tipo, metadados.Valor);
                if (metaEstrutura.ChavePrimaria)
                {
                    //Se já contiver uma PK, acrescenta o AND
                    if (contaisPk)
                    {
                        sqlString.Append(" AND ");
                    }
                    sqlString.Append($" {metadados.NomeColuna} = {value}");

                    contaisPk = true;
                }

            }

            if (!contaisPk) {
                return null;
            }
            return sqlString.ToString();
        }




    }
}
