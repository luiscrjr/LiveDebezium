using Entities;
using Entities.Entities.CDC;
using System;
using System.Collections.Generic;
using Entities.Enums;

namespace Entities.Entities.Kafka
{
    [Serializable]
    public class AgrupamentoTabela : BaseEntities
    {
        public AgrupamentoTabela(string nome)
        {
            Nome = nome;
        }

        public AgrupamentoTabela(string nome, IDictionary<string, MensagemKafka> dicionarioMetadados, MetadadoTabela estrutura)
        {
            Nome = nome;
            DicionarioMetadados = dicionarioMetadados;
            Estrutura = estrutura;
        }

        public AgrupamentoTabela(string nome, IDictionary<string, MensagemKafka> dicionarioMetadados, MetadadoTabela estrutura, string dataArquivo, long quantidadeRegistros)
        {
            Nome = nome;
            DicionarioMetadados = dicionarioMetadados;
            Estrutura = estrutura;
            DataArquivo = dataArquivo;
            QuantidadeRegistros = quantidadeRegistros;
        }

        public MetadadoTabela Estrutura { get; set; }
        public IDictionary<string, MensagemKafka> DicionarioMetadados { get; set; }
        public string Nome { get; set; }

        public string DataArquivo { get; set; }

        public long QuantidadeRegistros { get; set; }
    }
}