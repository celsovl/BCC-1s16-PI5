using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Comum
{
    public class Tiro
    {
        private Vetor canhao;
        private Vetor velocidade;
        public const double VELOCIDADEMEDIA = 1175;
        private double disparadoEm;
        const double GRAVIDADE = 9.80665;

        public Tiro(Vetor canhao, double anguloAzimute, double anguloElevacao, double disparadoEm)
        {
            this.canhao = canhao;
            this.disparadoEm = disparadoEm;
            velocidade = new Vetor(1, 0, 0).RotateZ(anguloAzimute).RotateY(anguloElevacao) * VELOCIDADEMEDIA;
        }

        public Vetor PosicaoEm(double tempo)
        {
            const double GRAVIDADE = 9.80665;
            Vetor g = new Vetor(0, 0, GRAVIDADE);
            double t = tempo - disparadoEm;

            return canhao + (t * velocidade) - (g * t * t) / 2;
        }

        public bool Viajando(double tempo)
        {
            return PosicaoEm(tempo).Z >= 0;
        }
    }
}
