using Entities.Entities.CDC;
using Entities.Entities.Kafka;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Entities.Entities.Sincronizacao
{
    public class DadosNotificacaoDTO
    {
        public DadosNotificacaoDTO()
        {
            PropriedadesChave = new List<PropriedadeNotificacaoDTO>();
            Propriedades = new List<PropriedadeNotificacaoDTO>();
        }

        public string Acao { get; set; }
        public string TipoObjeto { get; set; }
        public bool MudancaEstrutura { get; set; }

        public List<PropriedadeNotificacaoDTO> Propriedades { get; set; }
        public List<PropriedadeNotificacaoDTO> PropriedadesChave { get; set; }

        public static List<DadosNotificacaoDTO> ConverterDtoDadosNotificacao(List<Mensagem> mensagens)
        {
            var lista = new List<DadosNotificacaoDTO>();

            foreach (var mensagem in mensagens)
            {

                foreach (var dto in mensagem.Conteudo.DicionarioMetadados)
                {
                   
                    var dados = new DadosNotificacaoDTO();
                    dados.TipoObjeto = mensagem.Conteudo.Estrutura.Nome;
                    dados.MudancaEstrutura = mensagem.Conteudo.Estrutura.MudancaEstrutura;
                    if (string.IsNullOrWhiteSpace(dados.Acao)) dados.Acao = dto.Value.Operacao.ToString();

                    foreach (var metadadoKafka in dto.Value.Metadados)
                    {
                        
                        var coluna =
                        mensagem.Conteudo.Estrutura.ListaColuna?.FirstOrDefault(x =>
                            x.Nome.Equals(metadadoKafka.NomeColuna));

                        if (coluna == null) continue;

                        if (coluna.ChavePrimaria || coluna.ChaveEstrangeira)
                            dados.PropriedadesChave.Add(new PropriedadeNotificacaoDTO(coluna.Nome,
                                metadadoKafka.Valor));
                        else
                            dados.Propriedades.Add(new PropriedadeNotificacaoDTO(coluna.Nome, metadadoKafka.Valor));
                    }

                    lista.Add(dados);
                }
            }

            return lista;
        }

        public static List<DadosNotificacaoDTO> ConverterDtoDadosNotificacao ( List<Dictionary<string, MensagemKafka>> mensagens
            , MetadadoTabela metadadoTabela )
        {
            var lista = new List<DadosNotificacaoDTO>();

            foreach ( var mensagem in mensagens )
            {

                foreach ( MensagemKafka dto in mensagem.Values )
                {
                    var dados = new DadosNotificacaoDTO();
                    dados.TipoObjeto = metadadoTabela.Nome;
                    dados.MudancaEstrutura = metadadoTabela.MudancaEstrutura;

                    if ( string.IsNullOrWhiteSpace( dados.Acao ) ) dados.Acao = dto.Operacao.ToString();

                    foreach ( MetadadoKafka metadadoKafka in dto.Metadados )
                    {
                        var coluna = metadadoTabela.ListaColuna?.FirstOrDefault( x =>
                             x.Nome.Equals( metadadoKafka.NomeColuna ) );

                        if ( coluna == null ) continue;

                        if ( coluna.ChavePrimaria || coluna.ChaveEstrangeira )
                            dados.PropriedadesChave.Add( new PropriedadeNotificacaoDTO( coluna.Nome,
                                metadadoKafka.Valor ) );
                        else
                            dados.Propriedades.Add( new PropriedadeNotificacaoDTO( coluna.Nome, metadadoKafka.Valor ) );
                    }

                    lista.Add( dados );
                }
            }

            return lista;
        }



        public override string ToString()
        {
            StringBuilder dados = new StringBuilder();
            dados.AppendFormat("TipoObjeto: {0}", TipoObjeto).AppendLine();
            dados.AppendFormat("Acao: {0}", Acao).AppendLine();
            return dados.ToString();
        }

    }
}