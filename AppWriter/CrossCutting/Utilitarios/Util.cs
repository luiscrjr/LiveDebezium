using Entities.Entities;
using Entities.Entities.CDC;
using Entities.Entities.Debezium;
using Entities.Entities.Kafka;
using Entities.Enums;
using Confluent.Kafka;
using java.math;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity.Interception.Utilities;
using MensagemProtoUsing = Infra.Shared.Proto.Mensagem;
using Timestamp = java.sql.Timestamp;

namespace CrossCutting.Utilitarios
{
    public static class Util

        
    {
        public static readonly String DATETIME_PATTERN = "{0:yyyy-MM-dd HH:mm:ss}";
        public static MetadadoTabela ConvertSchemaBDToMetadadoTabela(IList<SchemaBD> listaSchemaBD, string nometabela, IDictionary<string, bool> dicionarioTabelaConstraints)
        {
            var metadadoTabela = new MetadadoTabela();
            if (listaSchemaBD.Count > 0)
            {
                metadadoTabela.Nome = nometabela;

                foreach (var item in listaSchemaBD)
                {
                    var chavePrimaria = dicionarioTabelaConstraints.ContainsKey(item.ColName) ? dicionarioTabelaConstraints[item.ColName] : false;
                    var metadadoColuna = new MetadadoColuna()
                    {
                        ChavePrimaria = chavePrimaria,
                        Nome = item.ColName,
                        Tamanho = item.ColLength,
                        Tipo = item.ColNameType,
                        Ordem = item.Colno
                    };
                    metadadoColuna.GerarNovoCodigo();
                    metadadoTabela.ListaColuna.Add(metadadoColuna);
                }
            }

            return metadadoTabela;
        }

        public static MensagemProtoUsing MapearKafkaMensagemParaProtoMensagem(Mensagem mensagem)
        {
            var mensagemEnviar = new MensagemProtoUsing()
            {
                Aplicacao = (Infra.Shared.Proto.EnumIdentificadorAplicacao)(int)mensagem.Aplicacao,
                Conteudo = new Infra.Shared.Proto.AgrupamentoTabela()
                {
                    Nome = mensagem.Conteudo.Nome ?? string.Empty,
                },
                Data = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(mensagem.Data.ToUniversalTime()),
                IdentificadorAgrupamento = mensagem.IdentificadorAgrupamento ?? string.Empty,
                IdentificadorTabela = mensagem.IdentificadorTabela ?? string.Empty
            };

            var metadadoTabela = new Infra.Shared.Proto.MetadadoTabela()
            {
                Flag = mensagem.Conteudo.Estrutura.Flag,
                Nome = mensagem.Conteudo.Estrutura.Nome ?? string.Empty,
                Operacao = (Infra.Shared.Proto.EnumOperacao)(int)mensagem.Conteudo.Estrutura.Operacao,
                MudancaEstrutura = mensagem.Conteudo.Estrutura.MudancaEstrutura
            };

            foreach (var item in mensagem.Conteudo.Estrutura.ListaColuna)
            {
                var metadadoColuna = new Infra.Shared.Proto.MetadadoColuna()
                {
                    Id = item.Id,
                    ChaveEstrangeira = item.ChaveEstrangeira,
                    ChavePrimaria = item.ChavePrimaria,
                    Data = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(item.Data.ToUniversalTime()),
                    Nome = item.Nome ?? string.Empty,
                    Tamanho = item.Tamanho,
                    Tipo = item.Tipo ?? string.Empty,
                    Valor = item.Valor ?? string.Empty

                };

                metadadoTabela.ListaColuna.Add(metadadoColuna);
            }

            if (mensagem.Conteudo.Estrutura.TabelasAssociadas != null)
                metadadoTabela.TabelasAssociadas.AddRange(mensagem.Conteudo.Estrutura.TabelasAssociadas);

            mensagemEnviar.Conteudo.Estrutura = metadadoTabela;

            var dicionarioMetadados = new Dictionary<string, Infra.Shared.Proto.MensagemKafka>();
            mensagem.Conteudo.DicionarioMetadados.ForEach(action =>
            {
                var mensagemKafka = action.Value;
                var mensagemKafkaProto = new Infra.Shared.Proto.MensagemKafka();
                mensagemKafkaProto.Operacao = (Infra.Shared.Proto.EnumOperacao)(int)mensagemKafka.Operacao;
                mensagemKafkaProto.DataRegistro = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(mensagemKafka.DataRegistro.ToUniversalTime());

                foreach (var metadadoKafka in mensagemKafka.Metadados)
                {

                    var metadadoKafkaProto = new Infra.Shared.Proto.MetadadoKafka()
                    {
                        Id = metadadoKafka.Id ?? string.Empty,
                        NomeColuna = metadadoKafka.NomeColuna ?? string.Empty,
                        Valor = metadadoKafka.Valor ?? string.Empty
                    };

                    mensagemKafkaProto.Metadados.Add(metadadoKafkaProto);
                }

                dicionarioMetadados.Add(action.Key, mensagemKafkaProto);
            });

            mensagemEnviar.Conteudo.DicionarioMetadados.Add(dicionarioMetadados);

            return mensagemEnviar;
        }

        public static Mensagem MapearProtoMensagemParaKafkaMensagem(MensagemProtoUsing mensagemProto)
        {
            if (mensagemProto.Conteudo == null) { return null; }
            var metadadoTabela = new MetadadoTabela()
            {
                Flag = mensagemProto.Conteudo.Estrutura.Flag,
                Nome = mensagemProto.Conteudo.Estrutura.Nome ?? string.Empty,
                Operacao = (EnumOperacao)(int)mensagemProto.Conteudo.Estrutura.Operacao,
                MudancaEstrutura = mensagemProto.Conteudo.Estrutura.MudancaEstrutura
            };

            foreach (var item in mensagemProto.Conteudo.Estrutura.ListaColuna)
            {
                var metadadoColuna = new MetadadoColuna()
                {
                    Id = item.Id,
                    ChaveEstrangeira = item.ChaveEstrangeira,
                    ChavePrimaria = item.ChavePrimaria,
                    Data = item.Data.ToDateTime(),
                    Nome = item.Nome ?? string.Empty,
                    Tamanho = item.Tamanho,
                    Tipo = item.Tipo ?? string.Empty,
                    Valor = item.Valor ?? string.Empty
                };

                metadadoTabela.ListaColuna.Add(metadadoColuna);
            }

            if (mensagemProto.Conteudo.Estrutura.TabelasAssociadas != null)
            {
                metadadoTabela.TabelasAssociadas = new List<string>();
                mensagemProto.Conteudo.Estrutura.TabelasAssociadas.ForEach(action =>
                {
                    metadadoTabela.TabelasAssociadas.Add(action);
                });
            }

            var dicionarioMetadados = new Dictionary<string, MensagemKafka>();


            mensagemProto.Conteudo.DicionarioMetadados.ForEach(action =>
            {
                var mensagemKafkaProto = action.Value;
                var mensagemKafka = new MensagemKafka
                {
                    Metadados = new List<MetadadoKafka>(),
                    DataRegistro = mensagemKafkaProto.DataRegistro.ToDateTime(),
                    Operacao = (EnumOperacao)(int)mensagemKafkaProto.Operacao
                };

                foreach (var metadadoProto in mensagemKafkaProto.Metadados)
                {
                    mensagemKafka.Metadados.Add(new MetadadoKafka(metadadoProto.Id, metadadoProto.NomeColuna, ObterValorFormatado(metadadoProto.Valor)));
                }

                dicionarioMetadados.Add(action.Key, mensagemKafka);
            });

            var agrupamentoTabela = new AgrupamentoTabela(
                mensagemProto.Conteudo.Nome,
                dicionarioMetadados,
                metadadoTabela,
                mensagemProto.Conteudo.DataArquivo,
                mensagemProto.Conteudo.QuantidadeRegistros
                );

            var mensagemRecebida = new Mensagem(
                (EnumIdentificadorAplicacao)(int)mensagemProto.Aplicacao,
                mensagemProto.IdentificadorTabela,
                agrupamentoTabela,
                mensagemProto.IdentificadorAgrupamento
                );

            return mensagemRecebida;
        }

        public static Mensagem MapearDebeziumMensagemParaKafkaMensagem(ConsumeResult<string, string> mensagemDebezium)
        {
            if (mensagemDebezium.Message.Value != null)
            {
                var messageJson = JsonConvert.DeserializeObject<DebeziumJsonModel>(mensagemDebezium.Message.Value);
                var messageKey = mensagemDebezium.Message.Key!=null?JsonConvert.DeserializeObject<DebeziumKey>(mensagemDebezium.Message.Key):new DebeziumKey();

                //var mensagemProto = ConverterDtoDadosNotificacao(messageJson, messageKey);
                var NomeTabela = messageJson.payload.source.table; //NomeTabela;
                var Operacao = GetTipoOperacaoDebezium(messageJson.payload.op);
                var MudancaEstrutura = false;
                var metadadoTabela = new MetadadoTabela()
                {
                    Flag = 1, //Pesquisar qual deve ser o valor default
                    Nome = NomeTabela ?? string.Empty,
                    Operacao = Operacao,
                    DataRegistro = DateTime.Now,
                    MudancaEstrutura = MudancaEstrutura //Por padrão, deixo requisições do debezium sem mudança de estrutura.
                };
                metadadoTabela.ListaColuna.AddRange(ConvertJsonToDataModelKafka(messageJson, messageKey, Operacao));

                var dicionarioMetadados = new Dictionary<string, MensagemKafka>();
                var mensagemKafka = new MensagemKafka
                {
                    Metadados = new List<MetadadoKafka>(),
                    DataRegistro = DateTime.Now,
                    Operacao = Operacao
                };
                mensagemKafka.Metadados.AddRange(ConvertJsonToDataKafka(messageJson, Operacao));
                dicionarioMetadados.Add(NomeTabela, mensagemKafka);

                var agrupamentoTabela = new AgrupamentoTabela(
                   NomeTabela,
                   dicionarioMetadados,
                   metadadoTabela

                   );

                var mensagemRecebida = new Mensagem(
                    EnumIdentificadorAplicacao.SqlServerDebezium,
                    NomeTabela,
                    agrupamentoTabela,
                    NomeTabela
                    );

                return mensagemRecebida;
            }
            return null;

        }

        private static List<MetadadoColuna> ConvertJsonToDataModelKafka(DebeziumJsonModel messageJson, DebeziumKey debeziumKey, EnumOperacao tipoOperacao)
        {
            List<MetadadoColuna> metadado = new List<MetadadoColuna>();
            var schema = GetSchemaDataBase(messageJson.schema, tipoOperacao);
            List<string> keyField = new List<string>();
            if (debeziumKey != null && debeziumKey.payload != null && schema != null)
            {
                foreach (JProperty child in debeziumKey.payload.Children())
                {
                    if (child.HasValues)
                    {
                        var fieldName = child.Name;
                        keyField.Add(fieldName);
                    }
                }
            }
            int ordem = 0;
            foreach (var field in schema.fields)
            {
                var fieldName = field.field;
                var fieldType = field.type;
                var metadadoColuna = new MetadadoColuna()
                {
                    ChavePrimaria = keyField.Contains(fieldName),
                    Data = DateTime.Now,
                    Nome = fieldName,
                    Tamanho = GetSizeByDebeziun(field),
                    Tipo = fieldType,
                    Valor = string.Empty,
                    Ordem = ordem++,


                };
                metadado.Add(metadadoColuna);

            }
            return metadado;
        }

        private static List<MetadadoKafka> ConvertJsonToDataKafka(DebeziumJsonModel messageJson, EnumOperacao tipoOperacao)
        {
            var Metadados = new List<MetadadoKafka>();
            JObject? resultMessage = messageJson.payload.after != null ? messageJson.payload.after : messageJson.payload.before;
            var schema = GetSchemaDataBase(messageJson.schema, tipoOperacao);
            if (resultMessage != null && schema != null)
            {
                foreach (JProperty child in resultMessage.Children())
                {
                    if (child.HasValues)
                    {
                        var fieldName = child.Name;


                        {
                            var fieldSchema = GetFieldBySchema(schema, fieldName);
                            var fieldSchemaName = fieldSchema.name;
                            var value = child.Value.Value<string>();
                            value = value == null ? "" : value;
                            if (!string.IsNullOrEmpty(fieldSchemaName))
                            {
                                value = ConvertValue(value, fieldSchema);

                            }
                            var metadado = new MetadadoKafka("", fieldName, ObterValorFormatado(value));
                            metadado.GerarNovoCodigo();
                            Metadados.Add(metadado);
                        }

                    }
                }
            }
            return Metadados;
        }


        private static DebeziumSchemaFieldFields GetFieldBySchema(DebeziumSchemaFields schema, string fieldName)
        {
            return schema.fields.FirstOrDefault(f => f.field.Equals(fieldName));
        }

        private static DebeziumSchemaFields? GetSchemaDataBase(DebeziumSchema schema, EnumOperacao operacao)
        {
            var fieldName = "after";
            if (EnumOperacao.Inclusao.Equals(operacao))
            {
                fieldName = "after";
            }
            else
            {
                fieldName = "before";
            }
            var field = schema.fields.FirstOrDefault(f => f.field.Equals(fieldName));
            return field;
        }
        public static EnumOperacao GetTipoOperacaoDebezium(string debeziumOp)
        {
            if ("c".Equals(debeziumOp))
            {
                return EnumOperacao.Inclusao;
            }
            else if ("u".Equals(debeziumOp))
            {
                return EnumOperacao.Alteracao;
            }
            else if ("d".Equals(debeziumOp))
            {
                return EnumOperacao.Exclusao;
            }
            return EnumOperacao.Nenhum;
        }

        private static int GetSizeByDebeziun(DebeziumSchemaFieldFields fieldSchema)
        {
            var fieldSchemaName = fieldSchema.name;
            if (!string.IsNullOrEmpty(fieldSchemaName) && fieldSchemaName.Contains(".Decimal") && fieldSchema.parameters != null)
            {
                var shcemaFields = fieldSchema.parameters.Children();

                var precision = shcemaFields.FirstOrDefault(w => w.Value<JProperty>().Name.Contains("precision"));
                if (precision != null)
                {
                    return precision.Value<JProperty>().Value.Value<int>();

                }


            }
            return 0;
        }
        private static string ConvertValue(string value, DebeziumSchemaFieldFields fieldSchema)
        {
            var newValue = value;
            var fieldSchemaName = fieldSchema.name;
            if (!string.IsNullOrEmpty(value))
            {
                if (fieldSchemaName.Contains(".Decimal") && fieldSchema.parameters != null && fieldSchema.parameters["scale"] != null)

                {
                    var scale = fieldSchema.parameters["scale"].Value<int>();
                    string encoded = value;
                    var bigInteger = new java.math.BigInteger(Convert.FromBase64String(encoded));
                    var bigDecimal = new BigDecimal(bigInteger, scale);
                    newValue = bigDecimal.toString();
                }
                else if ("io.debezium.time.Timestamp".Equals(fieldSchemaName))
                {

                    long timestamp = long.Parse(value);
                    DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    DateTime dt = epoch.AddMilliseconds(timestamp);

                    newValue = string.Format(DATETIME_PATTERN, dt);
                }
                else if ("io.debezium.time.Date".Equals(fieldSchemaName))
                {
                    DateTime dt = new DateTime(1970, 1, 1);
                    dt = dt.AddDays(Convert.ToInt32(value));
                    newValue = string.Format("{0:yyyy-MM-dd}", dt);
                }
            }

            return newValue;
        }

        private static string ObterValorFormatado(string valor)
        {
            string[] formats = { "MM/dd/yyyy HH:mm:ss", "dd/MM/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt", "dd/MM/yyyy hh:mm:ss tt" };

            DateTime data;
            if (DateTime.TryParseExact(valor, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out data))
            {
                valor = data.ToString("dd/MM/yyyy HH:mm:ss");
            }

            return valor;
        }

    }
}