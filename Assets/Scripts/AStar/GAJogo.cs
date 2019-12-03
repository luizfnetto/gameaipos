using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace TrabalhoIA
{
    public class GaJogo
    {
        #region Variaveis
        private readonly List<Terreno> _caminho;
        private readonly int _tamanhoDaPopulacao;
        private readonly int _numeroDeGeracoes;
        private readonly double _taxaDeMutacao;
        private readonly Random _random;
        private readonly List<Dictionary<BaseInimiga, List<Aviao>>> _populacaoInicial;
        private readonly Decoder _decoder;
        private readonly Evaluator _evaluator;
        #endregion

        #region Propriedades

        private int NumeroDeBasesInimigasUtilizadas
        {
            get { return BasesInimigasUtilizadas.Count; }
        }
        private int NumeroDeAvioesPossiveis
        {
            get { return AvioesPossiveis.Count; }
        }

        /// <summary>
        /// Gera uma lista com todas as bases inimigas as quais devemos passar durante a solucao
        /// </summary>
        private List<BaseInimiga> BasesInimigasUtilizadas
        {
            get
            {
                return _caminho.Where(terreno => terreno.tipo == TipoTerreno.BaseInimiga).Cast<BaseInimiga>().ToList();
            }
        }

        /// <summary>
        /// Gera uma lista com todos os avioes que serao utilizados no jogo
        /// </summary>
        private static List<Aviao> AvioesPossiveis
        {
            get
            {
                return new List<Aviao>()
                {
                    new Aviao("F-22 Raptor", 1.5, 5),
                    new Aviao("F-35 Lightning II", 1.4, 5),
                    new Aviao("T-50 PAK FA", 1.3, 5),
                    new Aviao("Su-46", 1.2, 5),
                    new Aviao("MiG-35", 1.1, 5)
                };
            }
        }
        #endregion

        public GaJogo(List<Terreno> caminho, int tamanhoDaPopulacao, int geracoes, double taxaDeMutacao)
        {
            _caminho = caminho;
            _tamanhoDaPopulacao = tamanhoDaPopulacao;
            _numeroDeGeracoes = geracoes;
            _taxaDeMutacao = taxaDeMutacao;
            _random = new Random();
            _populacaoInicial = GeraPopulacaoInicial();
            _decoder = new Decoder(AvioesPossiveis, BasesInimigasUtilizadas);
            _evaluator = new Evaluator();

        }

        #region Populacao Inicial
        /// <summary>
        /// Gera a população inicial que consiste num dicionario onde para cada base inimiga, tem uma lista dos avioes que participarão da luta
        /// </summary>
        /// <returns></returns>
        private List<Dictionary<BaseInimiga, List<Aviao>>> GeraPopulacaoInicial()
        {
            var pop = new List<Dictionary<BaseInimiga, List<Aviao>>>();
            for (var i = 0; i < _tamanhoDaPopulacao; i++)
                pop.Add(TentaAdicionarIndividuoAleatorio());
            return pop;
        }

        private Dictionary<BaseInimiga, List<Aviao>> TentaAdicionarIndividuoAleatorio()
        {
            var valido = false;
            Dictionary<BaseInimiga, List<Aviao>> individuo = null;
            while (!valido)
            {
                individuo = AdicionaIndividuoAleatorio();
                valido = VerificaSeIndividuoDecodificadoValido(individuo);
            }
            return individuo;
        }

        private Dictionary<BaseInimiga, List<Aviao>> AdicionaIndividuoAleatorio()
        {
            var individuo = new Dictionary<BaseInimiga, List<Aviao>>();
            var novaListaDeAvioesPossiveis = AvioesPossiveis;
            foreach (var baseinimiga in BasesInimigasUtilizadas)
                individuo.Add(baseinimiga, GeraListadeAvioesAleatoria(novaListaDeAvioesPossiveis));
            return individuo;
        }

        private List<Aviao> GeraListadeAvioesAleatoria(List<Aviao> novaListaDeAvioesPossiveis)
        {
            var avioes = new List<Aviao>();
            for (var i = 0; i < novaListaDeAvioesPossiveis.Count; i++)
            {
                var newrandom = _random.NextDouble();
                if (newrandom >= 0.5)
                    if (novaListaDeAvioesPossiveis[i].PontosDeEnergia > 0)
                        if (!UltimoAviaoVivo(novaListaDeAvioesPossiveis))
                        {
                            avioes.Add(novaListaDeAvioesPossiveis[i]);
                            novaListaDeAvioesPossiveis[i].DiminuirPontoDeEnergia();
                        }
            }
            return avioes;
        }

        private bool UltimoAviaoVivo( List<Aviao> novaListaDeAvioesPossiveis)
        {
            return novaListaDeAvioesPossiveis.Sum(x => x.PontosDeEnergia) == 1;
        }
        #endregion

        #region GA
        public Dictionary<BaseInimiga, List<Aviao>> Rodadas(ref double pontuacao)
        {
            var populacao = _populacaoInicial;
            for (var geracao = 0; geracao < _numeroDeGeracoes; geracao++)
            {
                var populacaoCodificada = _decoder.Codifica(populacao);
                var populacaoCodificadaRecombinada = AplicaRecombinacao(populacaoCodificada);
                var populacaoCodificadaMutada = AplicaMutacao(populacaoCodificadaRecombinada);
                var populacaoFinal = _decoder.Decodifica(populacaoCodificadaMutada);
                populacao = SelecionaMelhoresIndividuos(populacao, populacaoFinal);
            }
            var avaliacaoFinal = _evaluator.Avaliar(populacao);
            pontuacao = pontuacao + avaliacaoFinal.Min();
            var indexMelhorIndividuo = avaliacaoFinal.IndexOf(avaliacaoFinal.Min());
            return populacao[indexMelhorIndividuo];
        }

        private List<bool[]> AplicaRecombinacao(List<bool[]> populacaoCodificada)
        {
            var populacaoRecombinada= new List<bool[]>();
            foreach (var individuo in populacaoCodificada)
            {
                bool valido;
                var tries = 30;
                var individuoRecombinado = new bool[individuo.Length];
                do
                {
                    valido = RecombinaIndividuo(populacaoCodificada, individuo, individuoRecombinado);
                } while (!valido && tries-- > 0);
                populacaoRecombinada.Add(valido ? individuoRecombinado : individuo);
            }
            return populacaoRecombinada;
        }

        private bool RecombinaIndividuo(List<bool[]> populacaoCodificada, bool[] individuo, bool[] individuoRecombinado)
        {
            var individuoParaRecombinacao =
                populacaoCodificada[_random.Next(0, _tamanhoDaPopulacao)];
            var indicePontodeCruzamento = _random.Next(0, individuo.Length);
            for (var i = 0; i < indicePontodeCruzamento; i++)
                individuoRecombinado[i] = individuo[i];
            for (var i = indicePontodeCruzamento; i < individuo.Length; i++)
                individuoRecombinado[i] = individuoParaRecombinacao[i];
            var valido = VerificaSeIndividuoValido(individuoRecombinado);
            return valido;
        }

        private List<bool[]> AplicaMutacao(List<bool[]> populacaoCodificada)
        {
            var populacaoMutada= new List<bool[]>();
            foreach (var individuo in populacaoCodificada)
            {
                bool valido;
                var tries = 5;
                var individuoMutado = new bool[individuo.Length];
                do
                {
                    valido = MutaIndividuo(individuo, individuoMutado);
                } while (!valido && tries-- > 0);
                populacaoMutada.Add(valido ? individuoMutado : individuo);
            }
            return populacaoMutada;
        }

        private bool MutaIndividuo(bool[] individuo, bool[] individuoMutado)
        {
            for (var i = 0; i < individuo.Length; i++)
                individuoMutado[i] = individuo[i];
            
            var muta = _random.NextDouble() < _taxaDeMutacao;
            if (muta)
            {
                var geneASerMutado = _random.Next(0, individuo.Length);
                if (individuoMutado[geneASerMutado])
                    individuoMutado[geneASerMutado] = false;
                else
                    individuoMutado[geneASerMutado] = true;
            }
            var individuoMutadoDecodificado = _decoder.DecodificaIndividuo(individuoMutado);
            var valido = VerificaSeIndividuoDecodificadoValido(individuoMutadoDecodificado);
            return valido;
        }

        private List<Dictionary<BaseInimiga, List<Aviao>>> SelecionaMelhoresIndividuos(List<Dictionary<BaseInimiga, List<Aviao>>> populacaoAntiga, List<Dictionary<BaseInimiga, List<Aviao>>> populacaoNova)
        {
            var populacaoFinal = new List<Dictionary<BaseInimiga, List<Aviao>>>();
            var avaliacaoAntiga = _evaluator.Avaliar(populacaoAntiga);
            var avaliacaoNova = _evaluator.Avaliar(populacaoNova);
            for (var i = 0; i < _tamanhoDaPopulacao; i++)
                populacaoFinal.Add(avaliacaoAntiga[i] < avaliacaoNova[i] ? populacaoAntiga[i] : populacaoNova[i]);
            return populacaoFinal;
        }

        #endregion

        #region Checa validade individuo
        private bool VerificaSeIndividuoValido(bool[] individuo)
        {
            if (!VerificaSeAviaoNaoEstaSendoUsadoMaisDoQuePode(individuo)) return false;
            if (!VerificaSeBaseTemPeloMenosUmAviaoLutando(individuo)) return false;
            return true;
        }

        private bool VerificaSeBaseTemPeloMenosUmAviaoLutando(bool[] individuo)
        {
            for (var i = 0; i < NumeroDeBasesInimigasUtilizadas; i++)
            {
                var sum = false;
                for (var j = i * NumeroDeAvioesPossiveis; j < i * NumeroDeAvioesPossiveis + NumeroDeAvioesPossiveis; j++)
                    sum = sum || individuo[j];
                if (!sum)
                    return false;
            }
            return true;
        }

        private bool VerificaSeAviaoNaoEstaSendoUsadoMaisDoQuePode(bool[] individuo)
        {
            var vidasUtilizadasPorAviaoJ = new int[NumeroDeAvioesPossiveis];
            for (var numeroAviao = 0; numeroAviao < NumeroDeAvioesPossiveis; numeroAviao++)
            {
                for (var i = numeroAviao; i < individuo.Length; i = i + NumeroDeAvioesPossiveis)
                    if (individuo[i])
                        vidasUtilizadasPorAviaoJ[numeroAviao]++;
                if (vidasUtilizadasPorAviaoJ[numeroAviao] > AvioesPossiveis[numeroAviao].PontosDeEnergia)
                    return false;
            }
            return vidasUtilizadasPorAviaoJ.Sum() < AvioesPossiveis.Select(x => x.PontosDeEnergia).Sum(); ;
        }

        private bool VerificaSeIndividuoDecodificadoValido(Dictionary<BaseInimiga, List<Aviao>> individuo)
        {
            //Verifica Se Aviao Nao Esta Sendo Usado Mais Do Que Pode
            foreach (var par in individuo)
            {
                var avioes = par.Value;
                if (avioes.Any(aviao => aviao.PontosDeEnergia < 0))
                    return false;
            }
            //verifica se sobrou pelo menos 1 aviao
            var sum = 0;
            foreach (var baseInimiga in individuo.Keys)
            {
                var aviaoCount = 0;
                for (var i = 0; i < NumeroDeAvioesPossiveis; i++)
                    if (individuo[baseInimiga].Any(x => x.Nome == AvioesPossiveis[i].Nome))
                        sum += individuo[baseInimiga][aviaoCount++].PontosDeEnergia;
            }
            //verifica se base tem pelo menos um aviao lutando
            return sum != 0 && individuo.Keys.All(baseInimiga => individuo[baseInimiga].Count != 0);
        }

        #endregion
    }
}