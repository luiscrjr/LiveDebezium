using System;

namespace Entities.Entities.Kafka
{
    [Serializable]
    public class MetadadoKafka : BaseEntities
    {
        public MetadadoKafka(string valor)
        {
            Valor = valor;
        }

        public MetadadoKafka(string id, string nomeColuna, string valor)
        {
            Id = id;
            Valor = valor;
            NomeColuna = nomeColuna;
        }

        public string NomeColuna { get; set; }

        public string Valor { get; set; }


    }
}