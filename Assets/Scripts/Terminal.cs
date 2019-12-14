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
    private readonly int _tamanhoDoQuadrado = 17;

    // Variaveis de Render
    private int texEscala = 10; 
    Texture2D texturaTela = null; // Textura com dimensoes _tamanhoDoMapa * texEscala: pixel perfect não funciona

    // Variaveis de controle
    private bool _rodando = false;

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
        }
        AtualizaTelaComCaminho(texturaTela);
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
    #endregion Grafo

    #region Render

    private void CriaTelaTextura()
    {
        texturaTela = CriaTextura();
        Sprite sprite = Sprite.Create(texturaTela, new Rect(0, 0, texturaTela.width, texturaTela.height), new Vector2(0.5f, 0.5f), 100);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    private Texture2D CriaTextura()
    {
        Texture2D tex = new Texture2D(_tamanhoDoMapa * texEscala, _tamanhoDoMapa * texEscala, TextureFormat.RGBA32, false, false);

        for (var j = 0; j < _tamanhoDoMapa; j++)
        {
            for (var i = 0; i < _tamanhoDoMapa - 1; i++)
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

        for (int i = 0; i < _caminho.Count; i++)
        {
            Terreno o = _caminho[i];
            SetaCorRegiao(tex, o.x, o.y, Color.magenta);
        }
        tex.Apply();
    }
    #endregion Render
}
