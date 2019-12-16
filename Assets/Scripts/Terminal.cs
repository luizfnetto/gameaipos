using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using TrabalhoIA;
using UnityEngine.UI;

public class Terminal : MonoBehaviour
{
    #region variaveis
    // Variaveis do Grafo
    private double _pontuacaoAEstrela;
    private Terreno _inicio;
    private List<Terreno> _caminho;
    private Grafo<Terreno, double> _grafo;

    // Variaveis de Entrada
    private string[] _arquivo;
    private readonly int _tamanhoDoMapa = 42; /*Obs: mapa atual é 41 x 42*/

    // Variaveis de Render
    private int texEscala = 10;
    private int _passosDesenhoCaminhoTotal = 0;
    private int _tamanhoPassoDesenhoCaminho = 1;
    Texture2D _texturaTela = null; // Textura com dimensoes _tamanhoDoMapa * texEscala: pixel perfect não funciona

    // Variaveis de controle
    private bool _rodando = false;
    private bool _timerDisparado = false;
    private float _timer = 0.0f;
    private float _passoDeTempo = 1.0f;
    private double _razaoDeTempoGrafo = 10.0; // E.g. pontuacaoAEstrela = 280, _razaoDeTempoGrafo = 10, tempoTotalDoTimer = pontuacaoAEstrela/_razaoDeTempoGrafo = 28s

    // Hashes
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

    Dictionary<char, TipoTerreno> tiposDeTerrenoPorLetra = new Dictionary<char, TipoTerreno>
    {
        {'M', TipoTerreno.Montanhoso},
        {'.', TipoTerreno.Plano},
        {'R', TipoTerreno.Rochoso},
        {'I', TipoTerreno.Inicial},
        {'F', TipoTerreno.Final},
        {'C', TipoTerreno.BaseAntiAerea},
        {'B', TipoTerreno.BaseInimiga}
    };

    Dictionary<TipoTerreno, Color> corPorTerreno = new Dictionary<TipoTerreno, Color>
    {
        {TipoTerreno.Montanhoso, new Color(0.47843137f, 0.47843137f, 0.47843137f)},
        {TipoTerreno.Plano, new Color(0.754717f, 0.754717f, 0.754717f)},
        {TipoTerreno.Rochoso, new Color(0.31132078f, 0.31132078f, 0.31132078f)},
        {TipoTerreno.Inicial, new Color(1, 0.54901963f, 0)},
        {TipoTerreno.Final, new Color(0.7529412f, 1, 0)},
        {TipoTerreno.BaseAntiAerea, new Color(1, 0, 0)},
        {TipoTerreno.BaseInimiga, new Color(1,1,0)}
    };

    #endregion variaveis

    #region Unity3D
    // Start is called before the first frame update
    void Start()
    {
        _grafo = CriaGrafoDoMapa();
        CriaTelaTextura();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_rodando)
        {
            _rodando = true;
            ProcessaAEstrela();
            print("Pontuação do AEstrela:" + _pontuacaoAEstrela);
            double tempo = _pontuacaoAEstrela / 10.0;
            //_tamanhoPassoDesenhoCaminho = _caminho.Count / (int)tempo;
            _passoDeTempo = (float)tempo / _caminho.Count;
        }

        if (_timerDisparado)
        {
            AtualizaTelaComCaminho(_texturaTela);
            _timer += Time.deltaTime;
            if (_timer > _passoDeTempo)
            {
                _timer -= _passoDeTempo;
                _passosDesenhoCaminhoTotal += _tamanhoPassoDesenhoCaminho;
            }
        }
    }

    public void StartHacking ()
    {
        // Se ja esta sendo processado, retorna
        if (_timerDisparado) 
            return;

        // Delay de 3 segundos antes de comecar o hacking
        _timer += Time.deltaTime;
        if (_timer > 3)
        {
            _timer = 0;
            _timerDisparado = true;
        }
    }
    #endregion Unity3D

    private void ProcessaAEstrela()
    {
        _inicio = _grafo.PegarVertices().Single(node => node.tipo == TipoTerreno.Inicial);
        var fim = _grafo.PegarVertices().Single(node => node.tipo == TipoTerreno.Final);

        _grafo.AEstrela(_inicio, fim, Heuristica, (caminho, pontuacao) =>
        {
            _caminho = caminho;
            _pontuacaoAEstrela = pontuacao;
        });
    }
    
    #region Grafo
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

    private Grafo<Terreno, double> CriaGrafoDoMapa()
    {
        CarregaMapaEscolhido();
        var dificuldadesBases = new List<int>
            {
                 120, 110, 100,  95, 85, 90, 80, 75, 65, 70, 60
            };

        var grafo = CriaGrafo(dificuldadesBases, tiposDeTerrenoPorLetra);

        return grafo;
    }

    //Carregamento de arquivo
    private void CarregaMapaEscolhido()
    {
        // _arquivo = new string[] {
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMMMMMF.............................MMMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.MMMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM..................................MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM..................................MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.MMMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM..................................MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM..................................MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM..MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM..................................MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMM................................I.MMMM",
        //     "MMMMM................MM................MMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
        //     "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"};
    // _arquivo = new string[] {
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM................................F.MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.MMMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMM..................................MMMM",
    //        "MMMM................................I.MMMM",
    //        "MMMM..................................MMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
    //        "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"};
    _arquivo = new string[] {
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
           "MMMM..........M....RR.........R...R...MMMM",
           "MMMM.RRR.R.R..B..R....RRR.R.R.R.R...F.MMMM",
           "MMMM.....R.R..M..R.RR.....R.R.R.R.R...MMMM",
           "MMMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
           "MMMM.R.MMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
           "MMMM.R..R......M.....RR..R.....M...R..MMMM",
           "MMMM.R..R.RRR..B..R..RR..R.RRR.B.R.RR.MMMM",
           "MMMM........R..M..R..........R.M.R....MMMM",
           "MMMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.R.MMMM",
           "MMMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
           "MMMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMBMMMMM",
           "MMMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
           "MMMMCMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM...MMMM",
           "MMMM....R.M..R.....R.....R.M..R...R.R.MMMM",
           "MMMM....R.B.RR..R..R.R...R.B.RR...R.R.MMMM",
           "MMMMRR....M.....R.....R....M..........MMMM",
           "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMMM",
           "MMMMRR.MMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMMM",
           "MMMM...MMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMMM",
           "MMMM.R.MMMMMMMMMMMMMMMMMMMMMMMMMMCMMMMMMMM",
           "MMMM.R....M.....R.....R....M..........MMMM",
           "MMMM.RR.R.B..RR.RRR.RRRR.R.B..R.RR....MMMM",
           "MMMM....R.M.......R.R....R.M.....R.RR.MMMM",
           "MMMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMM...MMMM",
           "MMMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMRR.MMMM",
           "MMMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMM...MMMM",
           "MMMMMMMMMMMMMMMMMMMMMMCMMMMMMMMMMMMRR.MMMM",
           "MMMM....R....R....M......R....R..M..R.MMMM",
           "MMMMRRR.R.R..R.RR.B..RRR.R.R..RR.B..R.MMMM",
           "MMMM......R.......M..R.....R.....M....MMMM",
           "MMMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
           "MMMM...MMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
           "MMMM.RRMMMMMMMMMMMMMMCMMMMMMMMMMMMMMMMMMMM",
           "MMMM..R......R...R..M..R......R.R.....MMMM",
           "MMMM..RRR.R..R..RRR.B..RRR.R..R.RR..I.MMMM",
           "MMMM......R.........M......R..........MMMM",
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM",
           "MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM"};
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
            for (var x = 0; x < _tamanhoDoMapa; x++)
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
            for (var x = 0; x < _tamanhoDoMapa; x++)
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
                        grafo.AdicionarAresta(corrente, _tempoGastoPorTerreno[corrente.tipo], noAdjacente);
                }
            }
    }

    private Terreno PegarTerrenoDeGrafoEmCoordenada(Grafo<Terreno, double> graph, int x, int y)
    {
        if (x >= 0 && x < _tamanhoDoMapa && y >= 0 && y < _tamanhoDoMapa)
            return graph.PegarVertices().Single(vertex => vertex.x == x && vertex.y == y);

        return null;
    }
    #endregion Grafo

    #region Render

    private void CriaTelaTextura()
    {
        _texturaTela = CriaTextura();
        Sprite sprite = Sprite.Create(_texturaTela, new Rect(0, 0, _texturaTela.width, _texturaTela.height), new Vector2(0.5f, 0.5f), 100);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    private Texture2D CriaTextura()
    {
        Texture2D tex = new Texture2D(_tamanhoDoMapa * texEscala, _tamanhoDoMapa * texEscala, TextureFormat.RGBA32, false, false);

        for (var j = 0; j < _tamanhoDoMapa; j++)
        {
            for (var i = 0; i < _tamanhoDoMapa; i++)
            {
                TipoTerreno t = tiposDeTerrenoPorLetra[_arquivo[j][i]];
                SetaCorRegiao(tex, i, j, corPorTerreno[t]);
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
        if (_caminho == null || _caminho.Count <= 0)
            return;

        for (int i = 0; i < _caminho.Count && i < _passosDesenhoCaminhoTotal; i++)
        {
            Terreno o = _caminho[i];
            SetaCorRegiao(tex, o.x, o.y, Color.magenta);
        }
        tex.Apply();
    }
    #endregion Render
}
