namespace TrabalhoIA
{
    public class BaseInimiga : Terreno
    {
        public double dificuldade;

        public BaseInimiga(int x, int y, double dificuldade) : base(x, y, TipoTerreno.BaseInimiga)
        {
            this.dificuldade = dificuldade;
        }
    }
}