using System;
using Entities.Entities.Kafka;
using Entities.Entities.CDC;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Entities.Enums;
using System.Text.RegularExpressions;
using log4net;
using System.Text;

namespace CrossCutting.Utilitarios
{
    public class UtilLeitorArquivoCDC
    {
        private static readonly int MAX_NUM_TRIES = 20;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(UtilLeitorArquivoCDC));
        private static readonly Encoding utf8Encoding;
        private static readonly Encoding codepage1252Encoding;

        static UtilLeitorArquivoCDC()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            utf8Encoding = Encoding.GetEncoding("UTF-8", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            codepage1252Encoding = Encoding.GetEncoding("Windows-1252", EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
        }

        public static List<string[]> Ler(string nomeArquivo)
        {
            List<string[]> listaArraysNomes = new List<string[]>();

            if (string.IsNullOrWhiteSpace(nomeArquivo)) return listaArraysNomes;

            int numTries = 0;
            while (true)
            {
                ++numTries;
                try
                {
                    try
                    {
                        LerLinhas(nomeArquivo, utf8Encoding, listaArraysNomes);
                    }
                    catch (DecoderFallbackException ex)
                    {
                        _logger.Debug($"UtilLeitorArquivoCDC|Ler|Erro ao ler {nomeArquivo} como UTF8, tentando como Windows1252\n{ex}");
                        LerLinhas(nomeArquivo, codepage1252Encoding, listaArraysNomes);
                    }
                    _logger.Debug($"UtilLeitorArquivoCDC|Ler|Finalizou leitura do arquivo {nomeArquivo} com {listaArraysNomes.Count} registros");
                    return listaArraysNomes;
                }
                catch (Exception ex)
                {
                    _logger.Debug($"UtilLeitorArquivoCDC|Ler|Falha ao tentar obter lock exclusivo do arquivo {nomeArquivo}", ex);
                    if (numTries > MAX_NUM_TRIES)
                    {
                        _logger.Error($"UtilLeitorArquivoCDC|Ler|Desistindo de ler arquivo {nomeArquivo} depois de {numTries} tentativas", ex);
                        return listaArraysNomes;
                    }
                    System.Threading.Thread.Sleep(500);
                }
                finally
                {
                    _logger.Info($"UtilLeitorArquivoCDC|Ler|Finalizou a tentativa de leitura do arquivo {nomeArquivo} após {numTries} tentativas");
                }
            }
        }

        private static void LerLinhas(string nomeArquivo, Encoding encoding, List<string[]> listaArraysNomes)
        {
            _logger.Debug($"UtilLeitorArquivoCDC|LerLinhas|Inicio");
            string linha = null;
            int numTries = 0;
            // Tenta abrir o arquivo de forma exclusiva
            using FileStream fs = new FileStream(nomeArquivo, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            _logger.Debug($"UtilLeitorArquivoCDC|LerLinhas|FileStream length {fs.Length}");
            using var arquivo = new StreamReader(fs, encoding);
            {
                arquivo.Peek();
                while (numTries < 5 && linha == null)
                {
                    ++numTries;
                    while ((linha = arquivo.ReadLine()) != null)
                    {
                        var arrayValores = Regex.Split(linha, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)").Select(x => x.Trim('"')).ToArray();
                        listaArraysNomes.Add(arrayValores);
                    }
                    System.Threading.Thread.Sleep(100);
                }
                _logger.Debug($"UtilLeitorArquivoCDC|LerLinhas|Finalizou leitura de linhas após {numTries} tentativas");
            }
            _logger.Debug($"UtilLeitorArquivoCDC|LerLinhas|Fim");
        }

        public static MensagemKafka ObtenhaListaMetadadoKafka(List<MetadadoColuna> lista, EnumOperacao operacao, DateTime? dataRegistro)
        {
            return new MensagemKafka(operacao, lista, dataRegistro);
        }
    }
}