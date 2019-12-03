using System;

namespace TrabalhoIA
{
    public class Aviao
    {
        private readonly string _nome;
        private readonly double _poderDeFogo;
        private int _pontosDeEnergia;

        public Aviao(string nome, double poderDeFogo, int pontosDeEnergia)
        {
            _nome = nome;
            _poderDeFogo = poderDeFogo;
            _pontosDeEnergia = pontosDeEnergia;
        }

        public string Nome
        {
            get { return _nome; }
        }

        public double PoderDeFogo
        {
            get { return _poderDeFogo; }
        }

        public int PontosDeEnergia
        {
            get { return _pontosDeEnergia; }
        }

        public void DiminuirPontoDeEnergia()
        {
            if (_pontosDeEnergia == 0)
            {
                throw new Exception("Nao pode mais diminuir vida");
            }
            _pontosDeEnergia --;
        }
    }
}