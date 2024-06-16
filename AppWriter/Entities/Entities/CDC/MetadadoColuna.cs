using System;

namespace Entities.Entities.CDC
{
    [Serializable]
    public class MetadadoColuna : BaseEntities
    {
        public bool ChaveEstrangeira { get; set; }
        public bool ChavePrimaria { get; set; }
        public DateTime Data { get; set; }

        public string Nome { get; set; }
        public long Tamanho { get; set; }

        public string Tipo { get; set; }

        public string Valor { get; set; }

        /// <summary>
        /// Ordem das colunas 
        /// </summary>
        public int Ordem { get; set; }
    }
}