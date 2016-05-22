using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Canhao
    {
        public Canhao(Vetor posicao)
        {
            Posicao = posicao;
            TirosRestantes = 4;
            Tiros = new Tiro[TirosRestantes];
        }

        public int TirosRestantes
        {
            get;
            private set;
        }

        public Tiro[] Tiros
        {
            get;
            private set;
        }

        public Vetor Posicao
        {
            get;
            private set;
        }

        public void Iniciar()
        {
        }

        public void Atirar(double tempo, double anguloAzimute, double anguloElevacao)
        {
            Tiros[4 - TirosRestantes] = new Tiro(Posicao, anguloAzimute, anguloElevacao, tempo);
            TirosRestantes--;
        }
    }
}
