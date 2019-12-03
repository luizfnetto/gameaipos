using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrabalhoIA
{
    public class Terreno
    {
        public int x, y;
        public TipoTerreno tipo;

        public Terreno(int x, int y, TipoTerreno tipo)
        {
            this.x = x;
            this.y = y;
            this.tipo = tipo;
        }
    }

    public enum TipoTerreno
    {
        Montanhoso,
        Plano,
        Rochoso,
        Inicial,
        Final,
        BaseAntiAerea,
        BaseInimiga
    }
}
