using System;
using System.Collections.Generic;
using System.Linq;

namespace TrabalhoIA
{
    internal static class AlgoritimoAEstrela
    {
        public static List<T> AEstrela<T>(this Grafo<T, double> grafo, T inicio, T fim, Func<T, double> heuristicCostEstimate = null, Action<List<T>, double> onChangePathCallback = null) where T : class
        {
            HashSet<T> closedSet = new HashSet<T>(), openSet = new HashSet<T> { inicio };

            var cameFrom = new Dictionary<T, T>();

            var gScore = new Dictionary<T, double>();

            foreach (var vertex in grafo.PegarVertices())
                gScore[vertex] = double.PositiveInfinity;

            gScore[inicio] = 0;

            var fScore = new Dictionary<T, double>();

            foreach (var vertex in grafo.PegarVertices())
                fScore[vertex] = double.PositiveInfinity;

            if (heuristicCostEstimate == null)
                heuristicCostEstimate = t => 0.0;

            fScore[inicio] = gScore[inicio] + heuristicCostEstimate(inicio);

            while (openSet.Count > 0)
            {
                var current = openSet.Aggregate((a, b) => fScore[a] < fScore[b] ? a : b);

                if (current == fim)
                {
                    var finalPath = CaminhoDeReconstrução(cameFrom, fim);

                    if (onChangePathCallback != null)
                        onChangePathCallback(finalPath, gScore[fim]);

                    return finalPath;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var neighbor in grafo.VerticesAdjacentes(current))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGScore = gScore[current] + grafo.PegarAresta(current, neighbor);

                    if (!openSet.Contains(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + heuristicCostEstimate(neighbor);
                        if (!openSet.Contains(neighbor))
                            openSet.Add(neighbor);

                        if (onChangePathCallback != null)
                            onChangePathCallback(CaminhoDeReconstrução(cameFrom, neighbor), gScore[neighbor]);
                    }
                }
            }

            throw new Exception();
        }

        static List<T> CaminhoDeReconstrução<T>(Dictionary<T, T> cameFrom, T current)
        {
            var totalPath = new List<T> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                totalPath.Insert(0, current);
            }

            return totalPath;
        }
    }
}