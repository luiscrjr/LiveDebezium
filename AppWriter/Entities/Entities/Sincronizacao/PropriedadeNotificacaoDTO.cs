using System.Text;

namespace Entities.Entities.Sincronizacao
{
    public class PropriedadeNotificacaoDTO
    {

        public PropriedadeNotificacaoDTO(string nome, string valor)
        {
            Nome = nome;
            Valor = valor;
        }

        public string Nome { get; set; }
        public string Valor { get; set; }

        public override string ToString()
        {
            StringBuilder dados = new StringBuilder();
            dados.AppendFormat("Propriedade: {0}", Nome).AppendLine();
            dados.AppendFormat("Valor: {0}", Valor).AppendLine();
            return dados.ToString();
        }
    }
}
