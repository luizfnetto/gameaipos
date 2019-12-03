using System;
using System.Collections.Generic;
using System.Linq;

namespace TrabalhoIA
{
    public class Evaluator
    {

        public List<double> Avaliar(List<Dictionary<BaseInimiga, List<Aviao>>> populacao)
        {
            var avaliacoes = new List<double>();
            foreach (var individuo in populacao)
            {
                var avaliacao = 0.0;
                foreach (var baseInimiga in individuo.Keys)
                {
                    var somaDoPoderDeFogo = individuo[baseInimiga].Sum(aviao => aviao.PoderDeFogo);
                    if (somaDoPoderDeFogo == 0.0)
                    {
                        avaliacao = 0;
                        throw new Exception("Nao existe aviao atacando base");
                    }
                    
                    avaliacao += baseInimiga.dificuldade/somaDoPoderDeFogo;
                }
                avaliacoes.Add(avaliacao);
            }
            return avaliacoes;
        }
    }
}