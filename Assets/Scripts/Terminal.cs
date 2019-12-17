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
    public double _thresholdForRatio = 30.0; // E.g. _aStarScore = 280, _thresholdForRatio = 10, totaltime = _aStarScore/_thresholdForRatio = 28s
    //Color hackedNodeColor = new Color(33.0f/255.0f, 96.0f/255.0f, 48.0f/255.0f); 242, 135, 5
    Color hackedNodeColor = new Color(242.0f/255.0f, 135.0f/255.0f, 5.0f/255.0f);
    // Hashes
    private readonly Dictionary<NodeType, double> _timeByNode = new Dictionary<NodeType, double>
    {
        {NodeType.NoiseField, 200},
        {NodeType.PlainField, 1},
        {NodeType.Scrambled, 25},
        {NodeType.InitialConn, 0},
        {NodeType.LocalMainFrame, 0},
        {NodeType.StrongSysDef, 150},
        {NodeType.NormalSysDef, 0} // Will be set later
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
        //Source: https://color.adobe.com/search?q=scifi#
        {NodeType.NoiseField, new Color(12.0f/255.0f, 12.0f/255.0f, 12.0f/255.0f)},
        {NodeType.PlainField, new Color(58.0f/255.0f, 142/255.0f, 216/255.0f)},
        {NodeType.Scrambled, new Color(9/255.0f, 58/255.0f, 89/255.0f)},
        {NodeType.InitialConn, new Color(1, 0.54901963f, 0)},
        {NodeType.LocalMainFrame, new Color(0.7529412f, 1, 0)},
        {NodeType.StrongSysDef, new Color(4.0f/255.0f, 36.0f/255.0f, 216.0f/255.0f)},
        {NodeType.NormalSysDef, new Color(30.0f/255.0f, 92.0f/255.0f, 216.0f/255.0f)}
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
        _startingPoint = _graph.PegarVertices().Single(node => node.type == NodeType.InitialConn);
        var fim = _graph.PegarVertices().Single(node => node.type == NodeType.LocalMainFrame);

        _graph.AEstrela(_startingPoint, fim, Heuristica, (caminho, pontuacao) =>
        {
            _path = caminho;
            _aStarScore = pontuacao;
        });
    }
    
    #region Grafo
    private double Heuristica(Node node)
    {
        var heuristica = Math.Abs(_startingPoint.x - node.x) + Math.Abs(_startingPoint.y - node.y);
        if (node.type == NodeType.NormalSysDef)
        {
            var baseInimiga = (NormalSysDef)node;
            return heuristica + baseInimiga.difficulty / 1.3;
        }
        return heuristica;
    }

    private Grafo<Node, double> CriaGrafoDoMapa()
    {
        LoadField();
        var dificuldadesBases = new List<int>
            {
                 120, 110, 100,  95, 85, 90, 80, 75, 65, 70, 60
            };

        var grafo = CriaGrafo(dificuldadesBases, _nodeTypeByLetter);

        return grafo;
    }

    //Carregamento de arquivo
    private void LoadField()
    {
        _nodeSysField = new string[] {
               "NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN",
               "NNN...N.F.N...N..............NNNNNNNNNNNNN",
               "NNNN.N.NNN.N.NNNNNNNNNNNNN...NNNNNNNNNNNNN",
               "NNNNNNN...NNNNNNNNNNNNNN...NNNNNNNNNNNNNNN",
               "NNNNNNNEENNNNNNNNNNNNN...NNNNNNNNNNNNNNNNN",
               "NNNNNNNEENNNNNNNNNNNNNNN...NNNNNNNNNNNNNNN",
               "NNNNNNNSSNNNNNNNNNNNNNNNNN...NNNNNNNNNNNNN",
               "NNNNNNNEENNNNNNNNNNNNNNNNNNN...NNNNNNNNNNN",
               "NNNNNNNSSNNNNNNNNNNNNNNNNNNNN...NNNNNNNNNN",
               "NNNNNNN..NNNNNNNNNNNNNNNNNNNN...NNNNNNNNNN",
               "NNNNNN..NNNNNNNNNNNNNNNNNNN...NNNNNNNNNNNN",
               "NNNN...NNNNNNNNNNNNNNNNNN...NNNNNNNNNNNNNN",
               "NNNNNNS...NNNNNNNNNNNNN...NNNNNNNNNNNNNNNN",
               "NNNN...NNNNNNNNNNNNNN...NNNNN...NNNNNNNNNN",
               "NNNNN...NNNNNNNNNNN...NNNNN...N...NNNNNNNN",
               "NNNNNN...NNNNNNNNNNNN...N...NNNNN...NNNNNN",
               "NNNNNNN...NNNNNNNNNNNNN...NNNNNNNNN...NNNN",
               "NNNNNN...NNNNNNNNNNNNNNNNNNNNNNNNN...NNNNN",
               "NNNN....NNNNNNNNNNNNNNNNNNN...NNNNNN...NNN",
               "NNNSSSSSNNNNNNNNNNNNNNNNNN..N...NNNNN...NN",
               "NNNN...NNNNNNNNNNNNNNNNNN..NNN...NNNNNN...",
               "NNNNNN....NNNNNNNNNNNNNN..NNNNN.........NN",
               "NNNNNNNNN....NNN...NNNN..NNNNNNEENNNNNNNNN",
               "NNNNNN....NNNNN.SSS..NNN..NN....NNNNNNNNNN",
               "NNNNNN....NNNN.SSSSSS.NNNNNNN.S...NNNNNNNN",
               "NNNNNNN..NNNNNN.SSSSS.NNNNNNN....NNNNNNNNN",
               "NNNNNNNSSNNNNNNNN....NNNNNN.S..NNNNNNNNNNN",
               "NNNNNNNN...NNNN....NNNNN....NNNNNNNNNNNNNN",
               "NNNNNNNN..NNNNNNN....NNNNNN....NNNNNNNNNNN",
               "NNNNNNNN..NNNNNNNNN...NNNNNNNNN....NNNNNNN",
               "NNNNNN..ENNNNNNNN....NNNNNNNNNNNEEN....NNN",
               "NNNNN.NN.NNNNNNNNN..NNNNNNNNN....NNNNNNNNN",
               "NNNNNNN....NNNNNNN..NNNNNNNNNN....NNNNNNNN",
               "NNNNNNNNN....NNNNN..NNNNNNNNNN....NNNNNNNN",
               "NNNNN....NNNNNNNNN..NNNNNNNN.N..NNNNNNNNNN",
               "NN....NNNNNNNNNNNN..NNNNNN....NNNNNNNNNNNN",
               "NNNNN....NNNNNNNNN..NNNNN....NNNNNNNNNNNNN",
               "NNNNNNNNN....NNNNN..NN....NNNNNNNNNNNNNNNN",
               "NNNNNNNNNNNNN....N..NN....NNNNNNNNNNNNNNNN",
               "NNNNNNNNNNNNNNNN.......NNNNNNNNNNNNNNNNNNN",
               "NNNNNNNNNNNNNNNNNN..I..NNNNNNNNNNNNNNNNNNN",
               "NNNNNNNNNNNNNNNNN.....NNNNNNNNNNNNNNNNNNNN"};
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
                        grafo.AdicionarAresta(corrente, _timeByNode[corrente.type], noAdjacente);
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
            SetaCorRegiao(tex, o.x, o.y, hackedNodeColor);
        }
        tex.Apply();
    }

    public bool IsHacked ()
    {
        return _hacked;
    }
    #endregion Render
}
