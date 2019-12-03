using System;
using System.Collections.Generic;
using System.Linq;

namespace TrabalhoIA
{
   
    public class Grafo<VT, ET> where VT : class
    {
        protected Dictionary<VT, List<Tuple<ET, VT>>> VerticesAdjecentes = new Dictionary<VT, List<Tuple<ET, VT>>>();

        public IEnumerable<VT> PegarVertices()
        {
            return VerticesAdjecentes.Keys;
        }

        protected void AdicionarVertice(VT vertice)
        {
            VerticesAdjecentes.Add(vertice, new List<Tuple<ET, VT>>());
        }

        public void DFS(VT root, Action<VT> preAction, Action<VT> postAction)
        {
            DFS_Visit(root, preAction, postAction, new HashSet<VT>());
        }

        void DFS_Visit(VT vertice, Action<VT> preAction, Action<VT> postAction, HashSet<VT> verticesVisitados)
        {
            verticesVisitados.Add(vertice);

            if (preAction != null)
                preAction(vertice);

            foreach (var verticeAdjacente in VerticesAdjacentes(vertice))
                if (!verticesVisitados.Contains(verticeAdjacente))
                    DFS_Visit(verticeAdjacente, preAction, postAction, verticesVisitados);

            if (postAction != null)
                postAction(vertice);
        }


        public void AdicionarVetice(VT vertice)
        {
            AdicionarVertice(vertice);
        }

        public void AdicionarAresta(VT verticeOrigem, ET aresta, VT verticeDestino)
        {
            VerticesAdjecentes[verticeOrigem].Add(Tuple.Create(aresta, verticeDestino));
        }

        public IEnumerable<VT> VerticesAdjacentes(VT vertice)
        {
            return VerticesAdjecentes[vertice].Select(verticeEAresta => verticeEAresta.Item2);
        }

        public ET PegarAresta(VT origem, VT destino)
        {
            return VerticesAdjecentes[origem].Single(aresta => aresta.Item2 == destino).Item1;
        }
    }
}