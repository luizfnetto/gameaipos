using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoIA
{
    class Decoder
    {
        private readonly List<Aviao> _avioesPossiveis;
        private readonly List<BaseInimiga> _basesInimigas;

        private int NumeroDeAvioesPossiveis
        {
            get { return _avioesPossiveis.Count; }
        }

        public Decoder(List<Aviao> avioesPossiveis, List<BaseInimiga> basesInimigas)
        {
            _avioesPossiveis = avioesPossiveis;
            _basesInimigas = basesInimigas;
        }

        public List<bool[]> Codifica(List<Dictionary<BaseInimiga, List<Aviao>>> populacao)
        {
            var listOfGenes = new List<bool[]>();
            foreach (var individuo in populacao)
            {
                var genes = new bool[individuo.Keys.Count * NumeroDeAvioesPossiveis];
                var geneCount = 0;
                foreach (var baseinimiga in individuo.Keys)
                    for (var j = 0; j < NumeroDeAvioesPossiveis; j++)
                        if (individuo[baseinimiga].Any(x => x.Nome == _avioesPossiveis[j].Nome))
                            genes[geneCount++] = true;
                        else
                            genes[geneCount++] = false;
                listOfGenes.Add(genes);
            }
            return listOfGenes;
        }

        public List<Dictionary<BaseInimiga, List<Aviao>>> Decodifica(List<bool[]> listaDeIndividuos)
        {
            var populacaoNova = new List<Dictionary<BaseInimiga, List<Aviao>>>();
            foreach (var individuocodificado in listaDeIndividuos)
                populacaoNova.Add(DecodificaIndividuo(individuocodificado));
            return populacaoNova;
        }

        public Dictionary<BaseInimiga, List<Aviao>> DecodificaIndividuo(bool[] individuocodificado)
        {
            var individuoNovo = new Dictionary<BaseInimiga, List<Aviao>>();
            var avioes = PegarListaDeAvioesReferenteAoIndividuo(individuocodificado);
            for (var indexBase = 0; indexBase < _basesInimigas.Count; indexBase++)
            {
                var listaDeAvioes = PegarListaDeAvioesReferenteABaseInimiga(avioes, indexBase, individuocodificado);
                individuoNovo.Add(_basesInimigas[indexBase], listaDeAvioes);
            }
            return individuoNovo; ;
        }

       
        private List<Aviao> PegarListaDeAvioesReferenteAoIndividuo(bool[] individuocodificado)
        {
            var listaDeAvioes = new List<Aviao>();
            for (var j = 0; j < NumeroDeAvioesPossiveis; j++)
            {
                var vidasUtilizadasPorAviaoJ = 0;
                for (var i = j; i < individuocodificado.Length; i = i + NumeroDeAvioesPossiveis)
                    if (individuocodificado[i])
                        vidasUtilizadasPorAviaoJ++;
                listaDeAvioes.Add(new Aviao(_avioesPossiveis[j].Nome, _avioesPossiveis[j].PoderDeFogo, _avioesPossiveis[j].PontosDeEnergia - vidasUtilizadasPorAviaoJ));
            }
            return listaDeAvioes;
        }

        private List<Aviao> PegarListaDeAvioesReferenteABaseInimiga(List<Aviao> avioes, int indexBase, bool[] individuocodificado)
        {
            var listaDeAvioes = new List<Aviao>();
            var inicial = indexBase * NumeroDeAvioesPossiveis;
            var final = inicial + NumeroDeAvioesPossiveis;
            var contadorAviao = 0;
            for (var i = inicial; i < final; i++)
            {
                if (individuocodificado[i])
                    listaDeAvioes.Add(avioes[contadorAviao]);
                contadorAviao++;
            }
            return listaDeAvioes;
        }

    }
}
