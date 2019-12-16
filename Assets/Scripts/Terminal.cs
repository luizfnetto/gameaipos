using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using GameAIPos;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{
    #region variables
    // Variaveis do Grafo
    private double _aStarScore;
    private Node _startingPoint;
    private List<Node> _path;
    private Grafo<Node, double> _graph;

    // Variaveis de Entrada
    private string[] _nodeSysField;
    private readonly int _fieldSize = 42; /*Obs: mapa atual é 41 x 42*/

    // Variaveis de Render
    private int texEscala = 10;
    private int _totalRenderPathSteps = 0;
    private int _renderPathStepSize = 1;
    Texture2D _screenTex = null; // Textura com dimensoes _fieldSize * texEscala: pixel perfect não funciona

    // Variaveis de controle
    private bool _running = false;
    private bool _timerTriggered = false;
    private bool _hacked = false;
    private float _timer = 0.0f;
    private float _timeStep = 1.0f;
    private double _thresholdForRatio = 10.0; // E.g. _aStarScore = 280, _thresholdForRatio = 10, totaltime = _aStarScore/_thresholdForRatio = 28s

    // Hashes
    private readonly Dictionary<NodeType, double> _timeByNode = new Dictionary<NodeType, double>
    {
        {NodeType.NoiseField, 200},
        {NodeType.PlainField, 1},
        {NodeType.Scrambled, 5},
        {NodeType.InitialConn, 0},
        {NodeType.LocalMainFrame, 0},
        {NodeType.StrongSysDef, 50},
        {NodeType.NormalSysDef, 0}
    };

    Dictionary<char, NodeType> _nodeTypeByLetter = new Dictionary<char, NodeType>
    {
        {'N', NodeType.NoiseField},
        {'.', NodeType.PlainField},
        {'S', NodeType.Scrambled},
        {'I', NodeType.InitialConn},
        {'F', NodeType.LocalMainFrame},
        {'H', NodeType.StrongSysDef},
        {'E', NodeType.NormalSysDef}
    };

    Dictionary<NodeType, Color> _colorByNodeType = new Dictionary<NodeType, Color>
    {
        {NodeType.NoiseField, new Color(0.47843137f, 0.47843137f, 0.47843137f)},
        {NodeType.PlainField, new Color(0.754717f, 0.754717f, 0.754717f)},
        {NodeType.Scrambled, new Color(0.31132078f, 0.31132078f, 0.31132078f)},
        {NodeType.InitialConn, new Color(1, 0.54901963f, 0)},
        {NodeType.LocalMainFrame, new Color(0.7529412f, 1, 0)},
        {NodeType.StrongSysDef, new Color(1, 0, 0)},
        {NodeType.NormalSysDef, new Color(1,1,0)}
    };

    #endregion variables

    #region Unity3D
    // Start is called before the first frame update
    void Start()
    {
        _graph = CriaGrafoDoMapa();
        CriaTelaTextura();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_running)
        {
            _running = true;
            ProcessaAEstrela();
            print("Pontuação do AEstrela:" + _aStarScore);
            double tempo = _aStarScore / _thresholdForRatio;
            //_renderPathStepSize = _path.Count / (int)tempo;
            _timeStep = (float)tempo / _path.Count;
        }

        if (_timerTriggered)
        {
            AtualizaTelaComCaminho(_screenTex);
            _timer += Time.deltaTime;
            if (_path != null && _totalRenderPathSteps >= _path.Count)
            {
                if (_timer > 0.5f)
                {
                    _timerTriggered = false;
                    _hacked = true;
                }
            }
            else
            {
                if (_timer > _timeStep)
                {
                    _timer -= _timeStep;
                    _totalRenderPathSteps += _renderPathStepSize;
                }
            }
        }
    }

    public void StartHacking ()
    {
        // Se ja esta sendo processado, retorna
        if (_timerTriggered) 
            return;

        // Delay de 3 segundos antes de comecar o hacking
        _timer += Time.deltaTime;
        if (_timer > 3)
        {
            _timer = 0;
            _timerTriggered = true;
            UnityEngine.Debug.Log("Start Terminal");
        }

    }
    #endregion Unity3D

    private void ProcessaAEstrela()
    {
        _startingPoint = _graph.PegarVertices().Single(node => node.tipo == NodeType.InitialConn);
        var fim = _graph.PegarVertices().Single(node => node.tipo == NodeType.LocalMainFrame);

        _graph.AEstrela(_startingPoint, fim, Heuristica, (caminho, pontuacao) =>
        {
            _path = caminho;
            _aStarScore = pontuacao;
        });
    }
    
    #region Grafo
    private double Heuristica(Node terreno)
    {
        var heuristica = Math.Abs(_startingPoint.x - terreno.x) + Math.Abs(_startingPoint.y - terreno.y);
        if (terreno.tipo == NodeType.NormalSysDef)
        {
            var baseInimiga = (NormalSysDef)terreno;
            return heuristica + baseInimiga.difficulty / 1.3;
        }
        return heuristica;
    }

    private Grafo<Node, double> CriaGrafoDoMapa()
    {
        CarregaMapaEscolhido();
        var dificuldadesBases = new List<int>
            {
                 120, 110, 100,  95, 85, 90, 80, 75, 65, 70, 60
            };

        var grafo = CriaGrafo(dificuldadesBases, _nodeTypeByLetter);

        return grafo;
    }

    //Carregamento de arquivo
    private void CarregaMapaEscolhido()
    {
        _nodeSysField = new string[] {
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNNNNNF.............................NNNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN.NNNNN",
             "NNNNN................NN................NNNN",
             "NNNNN..................................NNNN",
             "NNNNN................NN................NNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN................NN................NNNN",
             "NNNNN..................................NNNN",
             "NNNNN................NN................NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN.NNNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNN................NN................NNNN",
             "NNNNN..................................NNNN",
             "NNNNN................NN................NNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN................NN................NNNN",
             "NNNNN..................................NNNN",
             "NNNNN................NN................NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN..NNNN",
             "NNNNN................NN................NNNN",
             "NNNNN..................................NNNN",
             "NNNNN................NN................NNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNN................NN................NNNN",
             "NNNNN................................I.NNNN",
             "NNNNN................NN................NNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
             "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN"};
        //_nodeSysField = new string[] {
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN................................F.NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN.NNNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNN..................................NNNN",
        //        "NNNN................................I.NNNN",
        //        "NNNN..................................NNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //        "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN"};
        //_nodeSysField = new string[] {
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN..........N....SS.........S...S...NNNN",
        //       "NNNN.SSS.S.S..E..S....SSS.S.S.S.S...F.NNNN",
        //       "NNNN.....S.S..N..S.SS.....S.S.S.S.S...NNNN",
        //       "NNNN.SSNNNNNNNNNNNNNNHNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN.S.NNNNNNNNNNNNNNHNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN.S..S......N.....SS..S.....N...S..NNNN",
        //       "NNNN.S..S.SSS..E..S..SS..S.SSS.E.S.SS.NNNN",
        //       "NNNN........S..N..S..........S.N.S....NNNN",
        //       "NNNNHNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN.S.NNNN",
        //       "NNNNHNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //       "NNNNHNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNENNNNN",
        //       "NNNNHNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //       "NNNNHNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN...NNNN",
        //       "NNNN....S.N..S.....S.....S.N..S...S.S.NNNN",
        //       "NNNN....S.E.SS..S..S.S...S.E.SS...S.S.NNNN",
        //       "NNNNSS....N.....S.....S....N..........NNNN",
        //       "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNHNNNNNNNN",
        //       "NNNNSS.NNNNNNNNNNNNNNNNNNNNNNNNNNHNNNNNNNN",
        //       "NNNN...NNNNNNNNNNNNNNNNNNNNNNNNNNHNNNNNNNN",
        //       "NNNN.S.NNNNNNNNNNNNNNNNNNNNNNNNNNHNNNNNNNN",
        //       "NNNN.S....N.....S.....S....N..........NNNN",
        //       "NNNN.SS.S.E..SS.SSS.SSSS.S.E..S.SS....NNNN",
        //       "NNNN....S.N.......S.S....S.N.....S.SS.NNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNHNNNNNNNNNNNN...NNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNHNNNNNNNNNNNNSS.NNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNHNNNNNNNNNNNN...NNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNHNNNNNNNNNNNNSS.NNNN",
        //       "NNNN....S....S....N......S....S..N..S.NNNN",
        //       "NNNNSSS.S.S..S.SS.E..SSS.S.S..SS.E..S.NNNN",
        //       "NNNN......S.......N..S.....S.....N....NNNN",
        //       "NNNN.SSNNNNNNNNNNNNNNHNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN...NNNNNNNNNNNNNNHNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN.SSNNNNNNNNNNNNNNHNNNNNNNNNNNNNNNNNNNN",
        //       "NNNN..S......S...S..N..S......S.S.....NNNN",
        //       "NNNN..SSS.S..S..SSS.E..SSS.S..S.SS..I.NNNN",
        //       "NNNN......S.........N......S..........NNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
        //       "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN"};
    }

    //Criação de grafo
    private Grafo<Node, double> CriaGrafo(IReadOnlyList<int> dificuldadesBases, Dictionary<char, NodeType> _nodeTypeByLetter)
    {

        var grafo = new Grafo<Node, double>();

        AdicionarVertices(dificuldadesBases, _nodeTypeByLetter, grafo);
        AdicionarArestas(grafo);
        return grafo;
    }

    private void AdicionarVertices(IReadOnlyList<int> dificuldadesBases, Dictionary<char, NodeType> _nodeTypeByLetter,
        Grafo<Node, double> grafo)
    {

        var indexBaseInimiga = 0;
        for (var y = 0; y < _fieldSize; y++)
            for (var x = 0; x < _fieldSize; x++)
            {
                var corrente = _nodeSysField[y][x];
                grafo.AdicionarVetice(corrente != 'E'
                    ? new Node(x, y, _nodeTypeByLetter[corrente])
                    : new NormalSysDef(x, y, dificuldadesBases[indexBaseInimiga++]));
            }
    }

    private void AdicionarArestas(Grafo<Node, double> grafo)
    {
        for (var y = 0; y < _fieldSize; y++)
            for (var x = 0; x < _fieldSize; x++)
            {
                var corrente = PegarTerrenoDeGrafoEmCoordenada(grafo, x, y);

                foreach (var coordinates in new[]
                {
                        new[] { 1, 0},
                        new[] {-1, 0},
                        new[] { 0, 1},
                        new[] { 0,-1}
                    })
                {
                    var noAdjacente = PegarTerrenoDeGrafoEmCoordenada(grafo, x + coordinates[0], y + coordinates[1]);

                    if (noAdjacente != null)
                        grafo.AdicionarAresta(corrente, _timeByNode[corrente.tipo], noAdjacente);
                }
            }
    }

    private Node PegarTerrenoDeGrafoEmCoordenada(Grafo<Node, double> graph, int x, int y)
    {
        if (x >= 0 && x < _fieldSize && y >= 0 && y < _fieldSize)
            return graph.PegarVertices().Single(vertex => vertex.x == x && vertex.y == y);

        return null;
    }
    #endregion Grafo

    #region Render

    private void CriaTelaTextura()
    {
        _screenTex = CriaTextura();
        Sprite sprite = Sprite.Create(_screenTex, new Rect(0, 0, _screenTex.width, _screenTex.height), new Vector2(0.5f, 0.5f), 100);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    private Texture2D CriaTextura()
    {
        Texture2D tex = new Texture2D(_fieldSize * texEscala, _fieldSize * texEscala, TextureFormat.RGBA32, false, false);

        for (var j = 0; j < _fieldSize; j++)
        {
            for (var i = 0; i < _fieldSize; i++)
            {
                NodeType t = _nodeTypeByLetter[_nodeSysField[j][i]];
                SetaCorRegiao(tex, i, j, _colorByNodeType[t]);
            }
        }
        tex.Apply();
        return tex;
    }

    private void SetaCorRegiao (Texture2D tex, int i, int j, Color cor)
    {
        for (int y = j * texEscala; y < (j+1) * texEscala; y++)
        {
            for (int x = i*texEscala; x < (i+1) * texEscala; x++)
            {
                tex.SetPixel(x, y, cor);
            }
        }
    }

    private void AtualizaTelaComCaminho (Texture2D tex )
    {
        if (_path == null || _path.Count <= 0)
            return;

        for (int i = 0; i < _path.Count && i < _totalRenderPathSteps; i++)
        {
            Node o = _path[i];
            SetaCorRegiao(tex, o.x, o.y, Color.magenta);
        }
        tex.Apply();
    }

    public bool IsHacked ()
    {
        return _hacked;
    }
    #endregion Render
}
