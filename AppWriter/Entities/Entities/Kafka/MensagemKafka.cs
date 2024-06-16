using Entities.Entities.CDC;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using EnumOperacao = Entities.Enums.EnumOperacao;

namespace Entities.Entities.Kafka
{
    [Serializable]
    public class MensagemKafka
    {
        public MensagemKafka(EnumOperacao operacao, List<MetadadoColuna> metadados, DateTime? dataRegistro)
        {
            if (Metadados == null) Metadados = new List<MetadadoKafka>();
            Operacao = operacao;
            metadados.ForEach(x => Metadados.Add(new MetadadoKafka(x.Id, x.Nome, x.Valor)));
            DataRegistro = dataRegistro ?? DateTime.Now;
        }

        public MensagemKafka()
        {
        }

        public List<MetadadoKafka> Metadados { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EnumOperacao Operacao { get; set; }

        public DateTime DataRegistro { get; set; }
    }
}