using Entities.Enums;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Entities.Entities.Kafka
{
    [Serializable]
    public class Mensagem : BaseEntities
    {
        public Mensagem(EnumIdentificadorAplicacao aplicacao, string identificadorTabela, AgrupamentoTabela conteudo, string identificadorAgrupamento)
        {
            Data = DateTime.Now;
            IdentificadorTabela = identificadorTabela;
            Conteudo = conteudo;
            Aplicacao = aplicacao;
            IdentificadorAgrupamento = identificadorAgrupamento;
        }

        public DateTime Data { get; private set; }

        public AgrupamentoTabela Conteudo { get; private set; }

        public string IdentificadorAgrupamento { get; private set; }

        public string IdentificadorTabela { get; private set; }

        [JsonConverter(typeof(StringEnumConverter))]

        public EnumIdentificadorAplicacao Aplicacao { get; private set; }
    }
}