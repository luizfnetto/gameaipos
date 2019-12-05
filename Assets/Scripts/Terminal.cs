using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TrabalhoIA;

public class Terminal : MonoBehaviour
{
    #region variaveis

    private string[] _arquivo;
    private double _pontuacaoAEstrela;
    private double _pontuacaoGa;
    private List<Terreno> _caminho;
    //private bool _pronto = false;
    private readonly Dictionary<TipoTerreno, double> _tempoGastoPorTerreno = new Dictionary<TipoTerreno, double>
        {
            {TipoTerreno.Montanhoso, 200},
            {TipoTerreno.Plano, 1},
            {TipoTerreno.Rochoso, 5},
            {TipoTerreno.Inicial, 0},
            {TipoTerreno.Final, 0},
            {TipoTerreno.BaseAntiAerea, 50},
            {TipoTerreno.BaseInimiga, 0}
        };

    private readonly int _tamanhoDoMapa = 42; /*Obs: mapa atual é 41 x 42*/
    //private readonly int _tamanhoDoQuadrado = 17; // [netto] variavel nao usada
    private readonly int _tamPopulacao = 20;
    private readonly int _numGeracoes = 2000;
    private readonly double _taxaDeMutacao = 0.4;
    private Terreno _inicio;
    private Stopwatch _stopWatch;

    #endregion variaveis

    // Start is called before the first frame update
    void Start()
    {
        RodaPrograma();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void RodaPrograma()
    {
        var grafo = CriaGrafoDoMapa();
        _inicio = grafo.PegarVertices().Single(node => node.tipo == TipoTerreno.Inicial);
        var fim = grafo.PegarVertices().Single(node => node.tipo == TipoTerreno.Final);

        //CheckForIllegalCrossThreadCalls = false;
        Task.Run(() =>
        {
            _stopWatch = new Stopwatch();
            _stopWatch.Start();
            grafo.AEstrela(_inicio, fim, Heuristica, (caminho, pontuacao) => {
                _caminho = caminho;
                _pontuacaoAEstrela = pontuacao;
                //Refresh();
                //Thread.Sleep(8);
            });
            RodaGa();
        });
    }

    private double Heuristica(Terreno terreno)
    {
        var heuristica = Math.Abs(_inicio.x - terreno.x) + Math.Abs(_inicio.y - terreno.y);
        if (terreno.tipo == TipoTerreno.BaseInimiga)
        {
            var baseInimiga = (BaseInimiga)terreno;
            return heuristica + baseInimiga.dificuldade / 1.3;
        }
        return heuristica;
    }


    private void RodaGa()
    {
        var ga = new GaJogo(_caminho, _tamPopulacao, _numGeracoes, _taxaDeMutacao);
        var melhorIndividuo = ga.Rodadas(ref _pontuacaoGa);
        _stopWatch.Stop();
        //_pronto = true;
        //Refresh();
        EscreveSaida(melhorIndividuo);
    }


    #region Saida
    private void EscreveSaida(Dictionary<BaseInimiga, List<Aviao>> melhorIndividuo)
    {
        var saida = string.Format("Tempo Final: {0} + {1} = {2}\r\n", _pontuacaoAEstrela, _pontuacaoGa, _pontuacaoGa + _pontuacaoAEstrela);
        saida += string.Format("Tamanho populacao: {0} Numero de Geracoes: {1} Taxa de Mutacao: {2}\r\n", _tamPopulacao, _numGeracoes, _taxaDeMutacao);
        foreach (var baseInimiga in melhorIndividuo.Keys)
        {
            saida += string.Format("Base Inimiga dificuldade: {0} \r\n", baseInimiga.dificuldade);
            foreach (var aviao in melhorIndividuo[baseInimiga])
                saida += string.Format("\t\t nome: {0} poder de fogo: {1} vidas restantes: {2}\r\n", aviao.Nome,
                    aviao.PoderDeFogo, aviao.PontosDeEnergia);
        }
        saida += string.Format("Tempo decorrido: {0}", _stopWatch.Elapsed);
        print(saida);
    }
    #endregion Saida

    #region Grafo
    private Grafo<Terreno, double> CriaGrafoDoMapa()
    {
        CarregaMapaEscolhido();
        var dificuldadesBases = new List<int>
            {
                 120, 110, 100,  95, 85, 90, 80, 75, 65, 70, 60
            };
        var tiposDeTerrenoPorLetra = new Dictionary<char, TipoTerreno>
            {
                {'M', TipoTerreno.Montanhoso},
                {'.', TipoTerreno.Plano},
                {'R', TipoTerreno.Rochoso},
                {'I', TipoTerreno.Inicial},
                {'F', TipoTerreno.Final},
                {'C', TipoTerreno.BaseAntiAerea},
                {'B', TipoTerreno.BaseInimiga}
            };

        var grafo = CriaGrafo(dificuldadesBases, tiposDeTerrenoPorLetra);

        return grafo;
    }

    //Carregamento de arquivo
    private void CarregaMapaEscolhido()
    {
        //var caminhoArquivo = "";
        //using (FileDialog fileDialog = new OpenFileDialog())
        //{
        //    if (fileDialog.ShowDialog(this) == DialogResult.OK)
        //        caminhoArquivo = fileDialog.InitialDirectory + fileDialog.FileName;
        //}
        _arquivo = new string[] {
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                "MMM..........M....RR.........R....R...MMM",
                "MMM.RRR.R.R..B..R....RRR.R.R.R..R...F.MMM",
                "MMM.....R.R..M..R.RR.....R.R.R..R.R...MMM",
                "MMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
                "MMM.R.MMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
                "MMM.R..R......M.....RR..R.....M....R..MMM",
                "MMM.R..R.RRR..B..R..RR..R.RRR.B..R.RR.MMM",
                "MMM........R..M..R..........R.M..R....MMM",
                "MMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.R.MMM",
                "MMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMM",
                "MMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMBMMMM",
                "MMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMM",
                "MMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMM",
                "MMM....R.M..R.....R.....R.M..R....R.R.MMM",
                "MMM....R.B.RR..R..R.R...R.B.RR.R..R.R.MMM",
                "MMMRR....M.....R.....R....M....R......MMM",
                "MMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMM",
                "MMMRR.MMMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMM",
                "MMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMM",
                "MMM.R.MMMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMM",
                "MMM.R....M.....R.....R....M....R......MMM",
                "MMM.RR.R.B..RR.RRR.RRRR.R.B..R.RRR....MMM",
                "MMM....R.M.......R.R....R.M......R.RR.MMM",
                "MMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMM...MMM",
                "MMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMMRR.MMM",
                "MMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMM...MMM",
                "MMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMMRR.MMM",
                "MMM....R....R....M......R....R...M..R.MMM",
                "MMMRRR.R.R..R.RR.B..RRR.R.R..RRR.B..R.MMM",
                "MMM......R.......M..R.....R......M....MMM",
                "MMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
                "MMM...MMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
                "MMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
                "MMM..R......R...R..M..R......R..R.....MMM",
                "MMM..RRR.R..R..RRR.B..RRR.R..R.RRR..I.MMM",
                "MMM......R.........M......R...........MMM",
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
                "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"};
    }

    //Criação de grafo
    private Grafo<Terreno, double> CriaGrafo(IReadOnlyList<int> dificuldadesBases, Dictionary<char, TipoTerreno> tiposDeTerrenoPorLetra)
    {

        var grafo = new Grafo<Terreno, double>();

        AdicionarVertices(dificuldadesBases, tiposDeTerrenoPorLetra, grafo);
        AdicionarArestas(grafo);
        return grafo;
    }

    private void AdicionarVertices(IReadOnlyList<int> dificuldadesBases, Dictionary<char, TipoTerreno> tiposDeTerrenoPorLetra,
        Grafo<Terreno, double> grafo)
    {

        var indexBaseInimiga = 0;
        for (var y = 0; y < _tamanhoDoMapa; y++)
            for (var x = 0; x < _tamanhoDoMapa - 1; x++)
            {
                var corrente = _arquivo[y][x];
                grafo.AdicionarVetice(corrente != 'B'
                    ? new Terreno(x, y, tiposDeTerrenoPorLetra[corrente])
                    : new BaseInimiga(x, y, dificuldadesBases[indexBaseInimiga++]));
            }
    }

    private void AdicionarArestas(Grafo<Terreno, double> grafo)
    {
        for (var y = 0; y < _tamanhoDoMapa; y++)
            for (var x = 0; x < _tamanhoDoMapa - 1; x++)
            {
                var corrente = PegarTerrenoDeGrafoEmCoordenada(grafo, x, y);

                foreach (var coordinates in new[]
                {
                        new[] {1, 0},
                        new[] {-1, 0},
                        new[] {0, 1},
                        new[] {0, -1}
                    })
                {
                    var noAdjacente = PegarTerrenoDeGrafoEmCoordenada(grafo, x + coordinates[0], y + coordinates[1]);

                    if (noAdjacente != null)
                        grafo.AdicionarAresta(corrente, _tempoGastoPorTerreno[corrente.tipo], noAdjacente);
                }
            }
    }

    private Terreno PegarTerrenoDeGrafoEmCoordenada(Grafo<Terreno, double> graph, int x, int y)
    {
        if (x < 0 || y < 0 || x > _tamanhoDoMapa - 1 - 1 || y > _tamanhoDoMapa - 1) return null;
        return graph.PegarVertices().Single(vertex => vertex.x == x && vertex.y == y);
    }
    #endregion

    #region Interface
    //private void PrintaInterface(object sender, PaintEventArgs e)
    //{
    //    // [netto] Desenha somente após terminar
    //    if (!_pronto)
    //        return;
    //    DesenhaMapa(e);
    //    DesenhaCaminho(e);
    //    MostraCustoDoCaminho(e);
    //}

    //private void DesenhaMapa(PaintEventArgs e)
    //{
    //    var coresVsCasa = new Dictionary<char, Brush>
    //        {
    //            {'M', Brushes.Gray},
    //            {'R', Brushes.DarkGray},
    //            {'.', Brushes.LightGray},
    //            {'C', Brushes.Red},
    //            {'F', Brushes.LimeGreen},
    //            {'I', Brushes.Orange},
    //            {'B', Brushes.Gold}
    //        };

    //    for (var j = 0; j < _tamanhoDoMapa; j++)
    //        for (var i = 0; i < _tamanhoDoMapa - 1; i++)
    //            e.Graphics.FillRectangle(coresVsCasa[_arquivo[j][i]], _tamanhoDoQuadrado * i, _tamanhoDoQuadrado * j, _tamanhoDoQuadrado, _tamanhoDoQuadrado);

    //    DesenhaLinhasDoMapa(e);
    //}

    //private void DesenhaLinhasDoMapa(PaintEventArgs e)
    //{
    //    var linha = new Pen(Brushes.Black);
    //    var action = new Action<int>[]
    //    {
    //            i => e.Graphics.DrawLine(linha, i, 0, i, _tamanhoDoQuadrado*_tamanhoDoMapa),
    //            i => e.Graphics.DrawLine(linha, 0, i, _tamanhoDoQuadrado*_tamanhoDoMapa, i)
    //    };
    //    foreach (var printLineDirectionMethod in action)
    //        for (var i = 1; i < _tamanhoDoMapa; i++)
    //            printLineDirectionMethod(i * _tamanhoDoQuadrado);
    //}

    //private void MostraCustoDoCaminho(PaintEventArgs e)
    //{
    //    e.Graphics.DrawString("Minutos: " + _pontuacaoAEstrela, new Font(FontFamily.GenericSerif, 27, FontStyle.Bold), Brushes.Indigo, new PointF(250, 665));
    //}

    //void DesenhaCaminho(PaintEventArgs e)
    //{
    //    Func<int, int> f = d => d * _tamanhoDoQuadrado;

    //    if (_caminho != null && _caminho.Count > 1) for (int i = 0; i < _caminho.Count; i++)
    //        {
    //            Terreno o = _caminho[i];
    //            e.Graphics.FillRectangle
    //            (
    //                new SolidBrush(Color.FromArgb(128, Color.BlueViolet)),
    //                f(o.x),
    //                f(o.y),
    //                _tamanhoDoQuadrado,
    //                _tamanhoDoQuadrado
    //            );
    //        }
    //}
    #endregion Interface
}
