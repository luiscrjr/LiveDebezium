using Entities.Enums;
using System;
using System.Collections.Generic;

namespace Entities.Entities.CDC
{
    [Serializable]
    public class MetadadoTabela : BaseEntities
    {
        public MetadadoTabela()
        {
            ListaColuna = new List<MetadadoColuna>();
            MudancaEstrutura = false;
        }
        public List<MetadadoColuna> ListaColuna { get; set; }
        public string Nome { get; set; }

        public List<string> TabelasAssociadas { get; set; }

        public EnumOperacao Operacao { get; set; }

        public DateTime DataRegistro { get; set; }

        public int Flag { get; set; }

        public bool MudancaEstrutura { get; set; }

        public object ToJson(bool v)
        {
            return "{\"MetadadoTabela\":\"\"}";
        }
    }
}